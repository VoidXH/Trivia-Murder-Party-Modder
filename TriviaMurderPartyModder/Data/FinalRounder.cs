using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace TriviaMurderPartyModder.Data {
    public class FinalRounder : TreeViewItem {
        public FinalRounder() { }

        public FinalRounder(int id, string text) {
            this.id = id;
            Text = text;
        }

        public int ID {
            get => id;
            set {
                id = value;
                Header = ToString();
            }
        }
        int id;

        public string Text {
            get => text;
            set {
                text = value;
                Header = ToString();
            }
        }
        string text;

        public void ImportTopicAudio(string topicFile, string audioFile) {
            string folder = Path.Combine(Path.GetDirectoryName(topicFile), "TDFinalRound", ID.ToString());
            Directory.CreateDirectory(folder);
            string dataFile = Path.Combine(folder, "data.jet");
            if (!File.Exists(dataFile)) {
                File.WriteAllText(Path.Combine(folder, "data.jet"),
                    "{\"fields\":[{\"t\":\"B\",\"v\":\"true\",\"n\":\"HasQ\"},{\"t\":\"A\",\"v\":\"aud\",\"n\":\"Q\"}]}");
                File.Copy(audioFile, Path.Combine(folder, "aud.ogg"));
            } else if (!File.Exists(dataFile) || MessageBox.Show("This topic already has topic audio. Do you want to overwrite?", "Overwrite",
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes) {
                string dataContents = File.ReadAllText(dataFile);
                int q = dataContents.IndexOf("\"Q\""), preFile = dataContents.LastIndexOf(",", q);
                int postFile = dataContents.LastIndexOf("\"", preFile);
                preFile = dataContents.LastIndexOf("\"", postFile - 1) + 1;
                File.Delete(Path.Combine(folder, dataContents.Substring(preFile, postFile - preFile) + ".ogg"));
                File.WriteAllText(dataFile, dataContents.Substring(0, preFile) + "aud" + dataContents.Substring(postFile));
                File.Copy(audioFile, Path.Combine(folder, "aud.ogg"));
            }
        }

        public override string ToString() => string.Format("[{0}] {1}", ID, Text);
    }
}