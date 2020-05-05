using System.Collections.ObjectModel;
using System.IO;

namespace TriviaMurderPartyModder.Data {
    public class Questions : ObservableCollection<Question> {
        public void Add(string fileName) {
            string contents = File.ReadAllText(fileName);
            int position = 0;
            while ((position = contents.IndexOf("\"x\"", position) + 3) != 2) {
                int id = contents.IndexOf("\"id\"", position) + 4;
                int text = contents.IndexOf("\"text\"", position) + 6;
                int choices = contents.IndexOf("\"choices\"", position) + 9;
                if (id == -1 || text == -1 || choices == -1)
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
    }
}