using System.IO;
using System.Windows.Controls;

namespace TriviaMurderPartyModder.Data {
    public class FinalRounder : TreeViewItem {
        public FinalRounder() { }

        public FinalRounder(int id, string text) {
            this.id = id;
            Text = text;
        }

        public int ID {
            get => id;
            set {
                id = value;
                Header = ToString();
            }
        }
        int id;

        public string Text {
            get => text;
            set {
                text = value;
                Header = ToString();
            }
        }
        string text;

        public override string ToString() => string.Format("[{0}] {1}", ID, Text);
    }
}