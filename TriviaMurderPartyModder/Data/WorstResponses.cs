using System.IO;
using System.Text;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public class WorstResponses : DataFile<WorstResponse> {
        public WorstResponses() : base("drawing topics") { }

        protected override void Add(string fileName) {
            string contents = File.ReadAllText(fileName);
            int position = 0;
            while ((position = contents.IndexOf("\"x\"", position) + 3) != 2) {
                int id = contents.IndexOf("\"id\"", position) + 4;
                int category = contents.IndexOf("\"category\"", position) + 10;
                if (id == 3 || category == 9)
                    continue;
                id = contents.IndexOf(':', id) + 1;
                WorstResponse imported = new WorstResponse(int.Parse(contents.Substring(id, contents.IndexOf(',', id) - id).Trim()),
                    Parsing.GetTextEntry(ref contents, contents.IndexOf(':', category) + 1));
                Add(imported);
            }
        }

        public static void ResponseIssue(string text) => MessageBox.Show(text, "Response issue", MessageBoxButton.OK, MessageBoxImage.Error);

        protected override bool SaveAs(string name) {
            StringBuilder output = new StringBuilder("{\"content\":[{");
            for (int i = 0, end = Count; i < end; ++i) {
                WorstResponse wd = this[i];
                output.Append("\"x\":false,\"id\":").Append(wd.ID);
                if (string.IsNullOrWhiteSpace(wd.Question)) {
                    ResponseIssue(string.Format("No question given for response ID {0}.", wd.ID));
                    return false;
                }
                output.Append(",\"category\":\"").Append(Parsing.MakeTextCompatible(wd.Question)).Append("\",\"bumper\":\"\"}");
                if (i != end - 1)
                    output.Append(",{");
                else
                    output.Append("],\"episodeid\":1242}");
            }
            File.WriteAllText(name, output.ToString());
            return true;
        }
    }
}