using System.Windows.Controls;

namespace TriviaMurderPartyModder.Data {
    public class FinalRounderChoice : TreeViewItem {
        public FinalRounderChoice() { }

        public FinalRounderChoice(bool correct, string text) {
            this.correct = correct;
            Text = text;
        }

        public bool Correct {
            get => correct;
            set {
                correct = value;
                Header = ToString();
            }
        }
        bool correct;

        public string Text {
            get => text;
            set {
                text = value;
                Header = ToString();
            }
        }
        string text;

        public override string ToString() {
            if (Correct)
                return Text + " (correct)";
            return Text;
        }
    }
}