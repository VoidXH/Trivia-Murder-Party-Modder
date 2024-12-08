using System.IO;
using System.Text.Json.Nodes;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    /// <summary>
    /// Audio file existence and path container.
    /// </summary>
    /// <param name="dataFolder">Subgame folder containing the data folders for each entry</param>
    /// <param name="id">ID of the data entry for the subgame</param>
    /// <param name="defaultContents">Contents of an empty file for the current subgame with no audio files added</param>
    public class DataJet(string dataFolder, int id, string defaultContents = null) {
        /// <summary>
        /// The folder where this data.jet file is located.
        /// </summary>
        public string Folder { get; } = Path.Combine(dataFolder, id.ToString());

        /// <summary>
        /// Cached raw contents of the file.
        /// </summary>
        public string Contents {
            get {
                string dataPath = Path.Combine(Folder, "data.jet");
                contents ??= (File.Exists(dataPath) ? File.ReadAllText(dataPath) : defaultContents);
                return contents;
            }
            set {
                contents = value;
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Path.Combine(Folder, "data.jet"), contents);
            }
        }
        string contents;

        public static void GetIfNotLoaded(ref DataJet jet, string dataFolder, int id, string defaultContents = null) =>
            jet ??= new DataJet(dataFolder, id, defaultContents);

        /// <summary>
        /// Get the corresponding data block from a data.jet <paramref name="document"/> to a certain audio <paramref name="type"/>.
        /// </summary>
        static JsonNode GetAudioSettings(JsonNode document, AudioType type) => GetByName(document, type.ToString());

        /// <summary>
        /// Get a field from the <see cref="Contents"/> by name.
        /// </summary>
        static JsonNode GetByName(JsonNode document, string name) {
            JsonArray fields = document["fields"].AsArray();
            for (int i = 0, c = fields.Count; i < c; i++) {
                if (fields[i]["n"].GetValue<string>() == name) {
                    return fields[i];
                }
            }
            return null;
        }

        public bool GetAudioFileActive(AudioType type) {
            JsonNode value = GetByName(JsonNode.Parse(Contents), "Has" + type)["v"];
            return value != null && value.GetValue<string>() == "true";
        }

        public void SetValue(string name, string value) => ReplaceValue(name, value);

        public void SetValues(string[] names, string value) {
            for (int i = 0; i < names.Length; i++) {
                ReplaceValue(names[i], value);
            }
        }

        /// <summary>
        /// Delete the old audio file and optionally set a new by <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Game event associated with the audio file</param>
        /// <param name="sourceFile">Audio file to be added - if null, the event will be removed</param>
        public void SetAudioFile(AudioType type, string sourceFile) {
            JsonNode parsed = JsonNode.Parse(Contents),
                set = GetAudioSettings(parsed, type),
                oldFile = set["v"];
            if (oldFile != null) {
                string oldPath = Path.Combine(Folder, oldFile.GetValue<string>() + ".ogg");
                if (File.Exists(oldPath)) {
                    if (sourceFile != null && MessageBox.Show("There is already an audio file. Do you want to overwrite?", "Overwrite",
                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No) {
                        return;
                    }
                    File.Delete(oldPath);
                }
            }

            if (sourceFile != null) {
                set["v"] = type.ToString();
                Directory.CreateDirectory(Folder);
                File.Copy(sourceFile, Path.Combine(Folder, type + ".ogg"), true);
            } else if (oldFile != null) {
                set.AsObject().Remove("v");
            }

            JsonNode enableNode = GetByName(parsed, "Has" + type);
            if (enableNode != null) { // Questions always have question audio, this is redundant for them, thus, not set
                enableNode["v"] = sourceFile != null ? "true" : "false";
            }
            Contents = parsed.ToJsonString();
        }

        /// <summary>
        /// Remove the audio for an in-game event by <paramref name="type"/>.
        /// </summary>
        public void RemoveAudioFile(AudioType type) {
            SetAudioFile(type, null);
            MessageBox.Show(type + " audio was removed for the selected entry.");
        }

        string ReplaceValue(int v, string value) {
            int vEnd = contents.IndexOf(',', v + 2),
                qm2 = contents.LastIndexOf('"', vEnd - 1) - 1,
                qm1 = contents.LastIndexOf('"', qm2);
            string oldValue = contents.Substring(qm1 + 1, qm2 - qm1);
            Contents = contents.Remove(v, vEnd - v).Insert(v, $"\"v\":\"{value}\"");
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