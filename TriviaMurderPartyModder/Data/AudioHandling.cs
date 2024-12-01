using Microsoft.Win32;
using System.Windows.Controls;

using TriviaMurderPartyModder.Files;

namespace TriviaMurderPartyModder.Data {
    /// <summary>
    /// Audio file operations.
    /// </summary>
    public static class AudioHandling {
        /// <summary>
        /// Find an audio file on the disk for a <see cref="DataGrid"/> <see cref="Entry"/>.
        /// </summary>
        /// <param name="grid">Check if this <see cref="DataGrid"/> has any selected <see cref="Entry"/>, if not, we can't add this file to anything</param>
        /// <param name="list">Check if the <see cref="DataFile{T}"/> is saved (because audio files can only be saved next to it)</param>
        public static string LoadAudio<T>(DataGrid grid, DataFile<T> list) {
            if (grid.SelectedItem == null) {
                list.Issue(Properties.Resources.noAudioImportSelection);
                return null;
            }
            return FinalizeLoadAudio(list);
        }

        /// <summary>
        /// Find an audio file on the disk for a <see cref="TreeView"/> <see cref="Entry"/>.
        /// </summary>
        /// <param name="tree">Check if this <see cref="TreeView"/> has any selected <see cref="Entry"/>, if not, we can't add this file to anything</param>
        /// <param name="list">Check if the <see cref="DataFile{T}"/> is saved (because audio files can only be saved next to it)</param>
        public static string LoadAudio<T>(TreeView tree, DataFile<T> list) {
            if (tree.SelectedItem == null) {
                list.Issue(Properties.Resources.noAudioImportSelection);
                return null;
            }
            return FinalizeLoadAudio(list);
        }

        /// <summary>
        /// After initial checks have passed for audio loading, do the final checks and prompt the user to load a file.
        /// </summary>
        /// <param name="list">Check if the <see cref="DataFile{T}"/> is saved (because audio files can only be saved next to it)</param>
        /// <returns>The audio file path if selected and the <see cref="DataFile{T}"/> exists, null otherwise.</returns>
        static string FinalizeLoadAudio<T>(DataFile<T> list) {
            if (list.FileName == null) {
                list.Issue(Properties.Resources.noSavedFile);
                return null;
            }
            if (audioBrowser.ShowDialog() == true) {
                return audioBrowser.FileName;
            }
            return null;
        }

        /// <summary>
        /// File browser popup displayed to the user.
        /// </summary>
        static readonly OpenFileDialog audioBrowser = new() { Filter = Properties.Resources.oggFilter };
    }
}