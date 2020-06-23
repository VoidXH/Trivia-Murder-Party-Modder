using System.IO;
using System.Text;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public class WorstDrawings : DataFile<WorstDrawing> {
        public WorstDrawings() : base("drawing topics") { }

        protected override void Add(string fileName) {
            string contents = File.ReadAllText(fileName);
            int position = 0;
            while ((position = contents.IndexOf("\"x\"", position) + 3) != 2) {
                int id = contents.IndexOf("\"id\"", position) + 4;
                int category = contents.IndexOf("\"category\"", position) + 10;
                if (id == 3 || category == 9)
                    continue;
                id = contents.IndexOf(':', id) + 1;
                WorstDrawing imported = new WorstDrawing(int.Parse(contents.Substring(id, contents.IndexOf(',', id) - id).Trim()),
                    Parsing.GetTextEntry(ref contents, contents.IndexOf(':', category) + 1));
                Add(imported);
            }
        }

        public static void DrawingIssue(string text) => MessageBox.Show(text, "Drawing issue", MessageBoxButton.OK, MessageBoxImage.Error);

        protected override bool SaveAs(string name) {
            StringBuilder output = new StringBuilder("{\"content\":[{");
            for (int i = 0, end = Count; i < end; ++i) {
                WorstDrawing wd = this[i];
                output.Append("\"x\":false,\"id\":").Append(wd.ID);
                if (string.IsNullOrWhiteSpace(wd.Category)) {
                    DrawingIssue(string.Format("No category given for drawing ID {0}.", wd.ID));
                    return false;
                }
                output.Append(",\"category\":\"").Append(Parsing.MakeTextCompatible(wd.Category));
                if (i != end - 1)
                    output.Append("\"},{");
                else
                    output.Append("\"}],\"episodeid\":1258}");
            }
            File.WriteAllText(name, output.ToString());
            return true;
        }
    }
}