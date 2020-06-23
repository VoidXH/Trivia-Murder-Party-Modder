namespace TriviaMurderPartyModder.Data {
    public class WorstDrawing {
        public int ID { get; set; }

        public string Category {
            get => category;
            set {
                category = value;
                if (jet != null)
                    jet.SetValue("QuestionText", value);
            }
        }
        string category;

        DataJet jet;

        public WorstDrawing() => category = string.Empty;

        public WorstDrawing(int id, string category) {
            ID = id;
            this.category = category;
        }

        public void ImportAudio(string topicFile, string audioFile) {
            DataJet.Get(ref jet, ref topicFile, EntryType.TDWorstDrawing, ID, string.Format(
                "{{\"fields\":[{{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasJokeAudio\"}},{{\"t\":\"S\",\"v\":\"{0}\",\"n\":\"QuestionText\"}}," +
                "{{\"t\":\"S\",\"v\":\"\",\"n\":\"AlternateSpellings\"}},{{\"t\":\"A\",\"n\":\"JokeAudio\"}}]}}", Category));
            jet.SetAudioFile(AudioType.JokeAudio, audioFile);
        }
    }
}