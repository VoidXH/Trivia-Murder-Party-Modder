namespace TriviaMurderPartyModder.Data {
    public class WorstResponse : Entry {
        public string Question {
            get => question;
            set {
                question = value;
                jet?.SetValues(["QuestionText", "Category"], value);
            }
        }
        string question;

        public WorstResponse() => question = string.Empty;

        public WorstResponse(int id, string question) {
            ID = id;
            this.question = question;
        }

        public void ImportAudio(string dataFolder, string audioFile) {
            if (audioFile == null) {
                return;
            }
            DataJet.GetIfNotLoaded(ref jet, dataFolder, ID, string.Format(
                "{{\"fields\":[{{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasBumperAudio\"}},{{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasBumperType\"}}," +
                "{{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasCorrectAudio\"}},{{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasQuestionAudio\"}}," +
                "{{\"t\":\"S\",\"v\":\"\",\"n\":\"Suggestions\"}},{{\"t\":\"S\",\"v\":\"{0}\",\"n\":\"Category\"}}," +
                "{{\"t\":\"S\",\"v\":\"\",\"n\":\"CorrectText\"}},{{\"t\":\"S\",\"v\":\"None\",\"n\":\"BumperType\"}}," +
                "{{\"t\":\"S\",\"v\":\"{0}\",\"n\":\"QuestionText\"}},{{\"t\":\"S\",\"v\":\"\",\"n\":\"AlternateSpellings\"}}," +
                "{{\"t\":\"A\",\"n\":\"BumperAudio\"}},{{\"t\":\"A\",\"n\":\"CorrectAudio\"}},{{\"t\":\"A\",\"n\":\"QuestionAudio\"}}]}}", Question));
            jet.SetAudioFile(AudioType.QuestionAudio, audioFile);
        }
    }
}