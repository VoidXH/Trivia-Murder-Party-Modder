using System.Windows;
using System.Windows.Controls;

using TriviaMurderPartyModder.Data;

namespace TriviaMurderPartyModder.Files {
    public static class DataFileExtensions {
        public static void ReleaseCheck<T>(this DataFile<T> list) where T : Entry {
            if (list.FileName == null)
                list.Issue(Properties.Resources.noSavedFileRC);
            for (int i = 0, end = list.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (list[i].ID == list[j].ID) {
                        list.Issue(string.Format(Properties.Resources.multipleIDs, list[i].ID));
                        return;
                    }
                }
                if (!Parsing.CheckAudio(list.DataFolderPath, list[i].ID)) {
                    list.Issue(string.Format(Properties.Resources.missingAudio, list[i].ID));
                    return;
                }
            }
            MessageBox.Show(Properties.Resources.checkSuccess, Properties.Resources.checkResult);
        }

        /// <summary>
        /// When a row was deleted from a <paramref name="file"/>'s related <paramref name="grid"/>, update the file.
        /// </summary>
        /// <param name="file">The kept database of file contents</param>
        /// <param name="grid">The grid currently displaying the currents of the <paramref name="file"/></param>
        public static void RemoveElement<T>(this DataFile<T> file, DataGrid grid) {
            if (grid.SelectedItem == null) {
                file.Issue(Properties.Resources.noRemovable);
                return;
            }
            file.Remove((T)grid.SelectedItem);
        }
    }
}