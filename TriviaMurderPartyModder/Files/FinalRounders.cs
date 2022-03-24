using System.IO;
using System.Text;
using System.Windows;

using TriviaMurderPartyModder.Data;

namespace TriviaMurderPartyModder.Files {
    public class FinalRounders : DataFile<FinalRounder> {
        protected override string ReferenceFileName => "TDFinalRound.jet";

        public FinalRounders() : base("final round topics") { }

        protected override void Add(string fileName) {
            string contents = File.ReadAllText(fileName);
            int position = 0;
            while ((position = contents.IndexOf("\"x\"", position) + 3) != 2) {
                int id = contents.IndexOf("\"id\"", position) + 4;
                int text = contents.IndexOf("\"text\"", position) + 6;
                int choices = contents.IndexOf("\"choices\"", position) + 9;
                if (id == 3 || text == 5 || choices == 8)
                    continue;
                id = contents.IndexOf(':', id) + 1;
                FinalRounder imported = new FinalRounder(int.Parse(contents.Substring(id, contents.IndexOf(',', id) - id).Trim()),
                    Parsing.GetTextEntry(ref contents, contents.IndexOf(':', text) + 1));
                choices = contents.IndexOf('[', choices);
                position = Parsing.FindArrayEnd(ref contents, choices);
                string choiceCut = contents.Substring(choices + 1, position - choices - 2);
                choices = 0;
                while (true) {
                    int correct = choiceCut.IndexOf("\"correct\"", choices) + 9;
                    text = choiceCut.IndexOf("\"text\"", choices) + 6;
                    if (correct == 8 || text == 5)
                        break;
                    string correctResult = choiceCut.Substring(correct, text - correct - 6);
                    imported.Items.Add(new FinalRounderChoice(correctResult.Contains("true"),
                        Parsing.GetTextEntry(ref choiceCut, choices = choiceCut.IndexOf(':', text) + 1)));
                }
                Add(imported);
            }
        }

        public static void FinalRoundIssue(string text) =>
            MessageBox.Show(text, "Final round issue", MessageBoxButton.OK, MessageBoxImage.Error);

        protected override bool SaveAs(string name) {
            StringBuilder output = new StringBuilder("{\"episodeid\":1253,\"content\":[");
            for (int i = 0, end = Count; i < end; ++i) {
                FinalRounder q = this[i];
                output.Append("{\"x\":false,\"id\":").Append(q.ID);
                if (string.IsNullOrWhiteSpace(q.Text)) {
                    FinalRoundIssue(string.Format("No text given for topic ID {0}.", q.ID));
                    return false;
                }
                output.Append(",\"text\":\"").Append(Parsing.MakeTextCompatible(q.Text)).Append("\",\"pic\": false,\"choices\":[");
                for (int choice = 0, choices = this[i].Items.Count; choice < choices; ++choice) {
                    FinalRounderChoice item = (FinalRounderChoice)this[i].Items[choice];
                    if (choice != 0)
                        output.Append("},{");
                    else
                        output.Append("{");
                    if (item.Correct)
                        output.Append("\"correct\":true,");
                    else
                        output.Append("\"correct\":false,");
                    if (string.IsNullOrEmpty(item.Text)) {
                        FinalRoundIssue(string.Format("Choice without text found for topic \"{0}\".", q.Text));
                        return false;
                    }
                    output.Append("\"text\":\"").Append(Parsing.MakeTextCompatible(item.Text)).Append("\"");
                }
                if (i != end - 1)
                    output.Append("}]},");
                else
                    output.Append("}]}]}");
            }
            File.WriteAllText(name, output.ToString());
            return true;
        }
    }
}