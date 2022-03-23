using System.IO;
using System.Text;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public class Questions : DataFile<Question> {
        protected override string ReferenceFileName => "TDQuestion.jet";

        public Questions() : base("questions") { }

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
                Question imported = new Question {
                    ID = int.Parse(contents.Substring(id, contents.IndexOf(',', id) - id).Trim()),
                    Text = Parsing.GetTextEntry(ref contents, contents.IndexOf(':', text) + 1)
                };
                choices = contents.IndexOf('[', choices) + 1;
                int correct = contents.IndexOf("\"correct\"", choices);
                for (int answer = 1; answer <= 4; ++answer) {
                    choices = contents.IndexOf('{', choices) + 1;
                    if (choices <= correct)
                        imported.Correct = answer;
                    imported[answer] = Parsing.GetTextEntry(ref contents, contents.IndexOf("\"text\"", choices) + 6);
                    choices = contents.IndexOf('}', choices) + 1;
                }
                Add(imported);
            }
        }

        public static void QuestionIssue(string text) => MessageBox.Show(text, "Question issue", MessageBoxButton.OK, MessageBoxImage.Error);

        protected override bool SaveAs(string name) {
            StringBuilder output = new StringBuilder("{\"episodeid\":1244,\"content\":[");
            for (int i = 0, end = Count; i < end; ++i) {
                Question q = this[i];
                output.Append("{\"x\":false,\"id\":").Append(q.ID);
                if (string.IsNullOrWhiteSpace(q.Text)) {
                    QuestionIssue(string.Format("No text given for question ID {0}.", q.ID));
                    return false;
                }
                output.Append(",\"text\":\"").Append(Parsing.MakeTextCompatible(q.Text)).Append("\",\"pic\": false,\"choices\":[");
                if (q.Correct < 1 || q.Correct > 4) {
                    QuestionIssue(string.Format("No correct answer set for question \"{0}\".", q.Text));
                    return false;
                }
                for (int answer = 1; answer <= 4; ++answer) {
                    if (answer != 1)
                        output.Append("},{");
                    else
                        output.Append("{");
                    if (answer == q.Correct)
                        output.Append("\"correct\":true,");
                    if (string.IsNullOrWhiteSpace(q[answer])) {
                        QuestionIssue(string.Format("No answer {0} for question \"{1}\".", answer, q.Text));
                        return false;
                    }
                    output.Append("\"text\":\"").Append(Parsing.MakeTextCompatible(q[answer])).Append("\"");
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