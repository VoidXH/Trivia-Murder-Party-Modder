﻿using System.Windows.Controls;

namespace TriviaMurderPartyModder.Data {
    public class FinalRounder : TreeViewItem {
        public int ID {
            get => id;
            set {
                id = value;
                jet = null; // Will be loaded from a different folder
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

        public FinalRounder(int id, string text) {
            this.id = id;
            Text = text;
        }

        DataJet jet;

        public void ImportTopicAudio(string dataFolder, string audioFile) {
            if (audioFile == null) {
                return;
            }
            DataJet.GetIfNotLoaded(ref jet, dataFolder, id,
                "{\"fields\":[{\"t\":\"B\",\"v\":\"false\",\"n\":\"HasQ\"},{\"t\":\"A\",\"n\":\"Q\"}]}");
            jet.SetAudioFile(AudioType.Q, audioFile);
        }

        public override string ToString() => string.Format("[{0}] {1}", ID, Text);
    }
}