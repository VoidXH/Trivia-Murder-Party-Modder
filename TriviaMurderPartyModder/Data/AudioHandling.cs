using Microsoft.Win32;
using System.Windows.Controls;

using TriviaMurderPartyModder.Files;

namespace TriviaMurderPartyModder.Data {
    public static class AudioHandling {
        public static string LoadAudio<T>(DataGrid grid, DataFile<T> list) {
            if (grid.SelectedItem == null) {
                list.Issue(Properties.Resources.noAudioImportSelection);
                return null;
            }
            return FinalizeLoadAudio(list);
        }

        public static string LoadAudio<T>(TreeView tree, DataFile<T> list) {
            if (tree.SelectedItem == null) {
                list.Issue(Properties.Resources.noAudioImportSelection);
                return null;
            }
            return FinalizeLoadAudio(list);
        }

        static string FinalizeLoadAudio<T>(DataFile<T> list) {
            if (list.FileName == null) {
                list.Issue(Properties.Resources.noSavedFile);
                return null;
            }
            if (audioBrowser.ShowDialog() == true)
                return audioBrowser.FileName;
            return null;
        }

        static readonly OpenFileDialog audioBrowser = new OpenFileDialog { Filter = Properties.Resources.oggFilter };
    }
}