using System;
using System.IO;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public class Question {
        public int ID { get; set; }

        public string Text { get; set; }

        public string Answer1 { get; set; }

        public string Answer2 { get; set; }

        public string Answer3 { get; set; }

        public string Answer4 { get; set; }

        public int Correct { get; set; }

        public string this[int key] {
            get {
                switch (key) {
                    case 1:
                        return Answer1;
                    case 2:
                        return Answer2;
                    case 3:
                        return Answer3;
                    case 4:
                        return Answer4;
                    default:
                        throw new Exception("Out of range");
                }
            }
            set {
                switch (key) {
                    case 1:
                        Answer1 = value;
                        break;
                    case 2:
                        Answer2 = value;
                        break;
                    case 3:
                        Answer3 = value;
                        break;
                    case 4:
                        Answer4 = value;
                        break;
                    default:
                        break;
                }
            }
        }

        bool GetAudioAvailability(string dataFile, string kind) {
            int has = dataFile.IndexOf("Has" + kind);
            int yes = dataFile.LastIndexOf("true", has), no = dataFile.LastIndexOf("false", has);
            return yes > no;
        }

        public bool CheckAudio(string questionFile) {
            string folder = Path.Combine(Path.GetDirectoryName(questionFile), "TDQuestion", ID.ToString());
            string dataFile = Path.Combine(folder, "data.jet");
            if (!File.Exists(dataFile))
                return false;
            string dataContents = File.ReadAllText(dataFile);
            int pos = 0;
            while ((pos = dataContents.IndexOf("\"v\"", pos) + 3) != 2) {
                int valueStart = dataContents.IndexOf('"', pos) + 1;
                pos = dataContents.IndexOf('"', valueStart);
                string value = dataContents.Substring(valueStart, pos - valueStart);
                if (value.Equals("true") || value.Equals("false"))
                    continue;
                if (!File.Exists(Path.Combine(folder, value + ".ogg")))
                    return false;
            }
            return true;
        }

        public void ImportIntroAudio(string questionFile, string audioFile) {
            string folder = Path.Combine(Path.GetDirectoryName(questionFile), "TDQuestion", ID.ToString());
            string dataFile = Path.Combine(folder, "data.jet");
            if (!File.Exists(dataFile)) {
                MessageBox.Show("You have to add question audio first.", "Question issue", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string dataContents = File.ReadAllText(dataFile);
            if (GetAudioAvailability(dataContents, "Intro")) {
                if (MessageBox.Show("This question already has question audio. Do you want to overwrite?", "Overwrite",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                    return;
                int intro = dataContents.IndexOf("\"Intro"), preFile = dataContents.LastIndexOf(",", intro);
                int postFile = dataContents.LastIndexOf("\"", preFile);
                preFile = dataContents.LastIndexOf("\"", postFile - 1) + 1;
                File.Delete(Path.Combine(folder, dataContents.Substring(preFile, postFile - preFile) + ".ogg"));
                File.WriteAllText(dataFile, dataContents.Substring(0, preFile) + "pre" + dataContents.Substring(postFile));
            } else {
                int intro = dataContents.IndexOf("\"HasIntro"), change = dataContents.LastIndexOf("\"false", intro);
                dataContents = dataContents.Substring(0, change) + "\"true\"" + dataContents.Substring(change + 7);
                intro = dataContents.IndexOf("\"Intro");
                int n = dataContents.LastIndexOf("\"n\"", intro);
                File.WriteAllText(dataFile, dataContents.Substring(0, n) + "\"v\":\"pre\"," + dataContents.Substring(n));
            }
            File.Copy(audioFile, Path.Combine(folder, "pre.ogg"));
        }

        public void ImportQuestionAudio(string questionFile, string audioFile) {
            string folder = Path.Combine(Path.GetDirectoryName(questionFile), "TDQuestion", ID.ToString());
            Directory.CreateDirectory(folder);
            string dataFile = Path.Combine(folder, "data.jet");
            if (!File.Exists(dataFile)) {
                File.WriteAllText(Path.Combine(folder, "data.jet"),
                    "{\"fields\":[{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasIntro\"},{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasPic\"}," +
                    "{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasVamp\"},{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasChoices\"}," +
                    "{\"t\":\"A\",\"v\":\"aud\",\"n\":\"Q\"},{\"t\":\"A\",\"n\":\"Intro\"},{\"t\":\"A\",\"n\":\"Choices\"}," +
                    "{\"t\":\"A\",\"n\":\"Vamp\"},{\"t\":\"G\",\"n\":\"Pic\"}]}");
                File.Copy(audioFile, Path.Combine(folder, "aud.ogg"));
            } else if (!File.Exists(dataFile) || MessageBox.Show("This question already has question audio. Do you want to overwrite?", "Overwrite",
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
    }
}