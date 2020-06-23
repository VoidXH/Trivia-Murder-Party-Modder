using System.IO;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public class DataJet {
        public string Folder { get; }

        public string Contents {
            get {
                string dataPath = Path.Combine(Folder, "data.jet");
                if (contents == null) {
                    if (File.Exists(dataPath))
                        return contents = File.ReadAllText(dataPath);
                    return contents = defaultContents;
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

        public DataJet(string sourceFile, EntryType type, int id, string defaultContents = null) {
            Folder = Path.Combine(Path.GetDirectoryName(sourceFile), type.ToString(), id.ToString());
            this.defaultContents = defaultContents;
        }

        void Commit() {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, "data.jet"), contents);
        }

        string ReplaceValue(int v, string value) {
            int vEnd = contents.IndexOf(',', v + 2), qm2 = contents.LastIndexOf('"', vEnd - 1) - 1, qm1 = contents.LastIndexOf('"', qm2);
            string oldValue = contents.Substring(qm1 + 1, qm2 - qm1);
            contents = contents.Remove(v, vEnd - v).Insert(v, string.Format("\"v\":\"{0}\"", value));
            return oldValue;
        }

        void ReplaceValue(string name, string value) {
            int pos = contents.LastIndexOf(string.Format("\"{0}\"", name));
            if (pos != -1)
                ReplaceValue(contents.LastIndexOf("\"v\"", pos), value);
        }

        public void SetValue(string name, string value) {
            ReplaceValue(name, value);
            Commit();
        }

        public void SetAudioFile(AudioType type, string sourceFile) {
            string defaultName = type.ToString();
            int pos = Contents.LastIndexOf(string.Format("\"{0}\"", defaultName)), v = contents.LastIndexOf("\"v\"", pos);
            if (contents.IndexOf('{', v, pos - v) != -1) // Has no file, there is an array element separation between
                contents = contents.Insert(contents.LastIndexOf(',', pos - 4), string.Format(",\"v\":\"{0}\"", defaultName));
            else {
                if (MessageBox.Show("There is already an audio file. Do you want to overwrite?", "Overwrite",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                    return;
                File.Delete(Path.Combine(Folder, ReplaceValue(v, defaultName) + ".ogg"));
            }
            SetValue("Has" + type.ToString(), "true");
            File.Copy(sourceFile, Path.Combine(Folder, defaultName + ".ogg"));
        }

        public static void Get(ref DataJet jet, ref string sourceFile, EntryType type, int id, string defaultContents = null) {
            if (jet == null)
                jet = new DataJet(sourceFile, type, id, defaultContents);
        }
    }
}