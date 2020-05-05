using System;

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
    }
}