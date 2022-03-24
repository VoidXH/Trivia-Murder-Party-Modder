﻿namespace TriviaMurderPartyModder.Data {
    public class WorstResponse {
        public int ID { get; set; }

        public string Question {
            get => question;
            set {
                question = value;
                if (jet != null)
                    jet.SetValues(new string[]{"QuestionText", "Category"}, value);
            }
        }
        string question;

        DataJet jet;

        public WorstResponse() => question = string.Empty;

        public WorstResponse(int id, string question) {
            ID = id;
            this.question = question;
        }

        public void ImportAudio(DataFile<WorstResponse> list, string audioFile) {
            DataJet.Get(ref jet, list.DataFolderPath, ID, string.Format(
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