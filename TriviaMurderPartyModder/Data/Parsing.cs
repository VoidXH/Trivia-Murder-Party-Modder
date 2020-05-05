namespace TriviaMurderPartyModder.Data {
    public static class Parsing {
        public static string GetTextEntry(ref string source, int from) {
            while (source[from++] != '\"') ;
            int to = from;
            while (!(source[to] == '\"' && source[to - 1] != '\\')) ++to;
            return source.Substring(from, to - from).Replace("\\\"", "\"");
        }
    }
}