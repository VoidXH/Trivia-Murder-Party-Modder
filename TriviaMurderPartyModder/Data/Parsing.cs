using System.IO;

namespace TriviaMurderPartyModder.Data {
    public static class Parsing {
        public static bool CheckAudio(string folder, int id) {
            folder = Path.Combine(folder, id.ToString());
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

        public static int FindArrayEnd(ref string source, int from) {
            int depth = 0;
            while (source.Length != from) {
                switch (source[from]) {
                    case '"':
                        while (source[++from] != '"') ;
                        break;
                    case '[':
                        ++depth;
                        break;
                    case ']':
                        if (depth == 1)
                            return from;
                        --depth;
                        break;
                    default:
                        break;
                }
                ++from;
            }
            return from;
        }

        public static string GetTextEntry(ref string source, int from) {
            while (source[from++] != '\"') ;
            int to = from;
            while (!(source[to] == '\"' && source[to - 1] != '\\')) ++to;
            return source.Substring(from, to - from).Replace("\\\"", "\"");
        }

        public static string MakeTextCompatible(string source) =>
            source.Replace('ő', 'ö').Replace('Ő', 'Ö').Replace('ű', 'ü').Replace('Ű', 'Ü').Replace("\"", "\\\"");
    }
}