using System.IO;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public class DataJet {
        public string Folder { get; }

        public string Contents {
            get {
                string dataPath = Path.Combine(Folder, "data.jet");
                if (contents == null) {
                    contents = (File.Exists(dataPath) ? File.ReadAllText(dataPath) : defaultContents);
                }
                return contents;
            }
            set {
                contents = value;
                Commit();
            }
        }
        string contents;
        readonly string defaultContents;

        public DataJet(string path, int id, string defaultContents = null) {
            Folder = Path.Combine(path, id.ToString());
            this.defaultContents = defaultContents;
        }

        public static void GetIfNotLoaded(ref DataJet jet, string dataFolder, int id, string defaultContents = null) {
            if (jet == null) {
                jet = new DataJet(dataFolder, id, defaultContents);
            }
        }

        public bool GetAudioFileActive(AudioType type) {
            int pos = Contents.LastIndexOf($"\"Has{type}\""), v = contents.LastIndexOf("\"v\"", pos);
            return v != -1 && GetValue(v).Equals("true");
        }

        public void SetValue(string name, string value) {
            ReplaceValue(name, value);
            Commit();
        }

        public void SetValues(string[] names, string value) {
            for (int i = 0; i < names.Length; i++) {
                ReplaceValue(names[i], value);
            }
            Commit();
        }

        public void SetAudioFile(AudioType type, string sourceFile) {
            string defaultName = type.ToString();
            int pos = Contents.LastIndexOf($"\"{defaultName}\""), v = contents.LastIndexOf("\"v\"", pos);
            if (contents.IndexOf('{', v, pos - v) != -1) { // Has no file, there is an array element separation between
                contents = contents.Insert(contents.LastIndexOf(',', pos - 4), $",\"v\":\"{defaultName}\"");
            } else {
                if (MessageBox.Show("There is already an audio file. Do you want to overwrite?", "Overwrite",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No) {
                    return;
                }
                File.Delete(Path.Combine(Folder, ReplaceValue(v, defaultName) + ".ogg"));
            }
            SetValue("Has" + defaultName, "true");
            File.Copy(sourceFile, Path.Combine(Folder, defaultName + ".ogg"));
        }

        public void RemoveAudioFile(AudioType type) {
            SetValue("Has" + type.ToString(), "false");
            MessageBox.Show(type.ToString() + " audio was removed for the selected entry.");
        }

        void Commit() {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, "data.jet"), contents);
        }

        string GetValue(int afterIndex) {
            int vEnd = contents.IndexOf(',', afterIndex + 2),
                qm2 = contents.LastIndexOf('"', vEnd - 1) - 1,
                qm1 = contents.LastIndexOf('"', qm2);
            return contents.Substring(qm1 + 1, qm2 - qm1);
        }

        string ReplaceValue(int v, string value) {
            int vEnd = contents.IndexOf(',', v + 2),
                qm2 = contents.LastIndexOf('"', vEnd - 1) - 1,
                qm1 = contents.LastIndexOf('"', qm2);
            string oldValue = contents.Substring(qm1 + 1, qm2 - qm1);
            contents = contents.Remove(v, vEnd - v).Insert(v, $"\"v\":\"{value}\"");
            return oldValue;
        }

        void ReplaceValue(string name, string value) {
            int pos = Contents.LastIndexOf($"\"{name}\"");
            if (pos != -1) {
                ReplaceValue(contents.LastIndexOf("\"v\"", pos), value);
            }
        }
    }
}