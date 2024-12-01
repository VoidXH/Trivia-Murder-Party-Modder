using System.ComponentModel;
using System.IO;
using System.Windows;

using TriviaMurderPartyModder.Properties;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace TriviaMurderPartyModder {
    /// <summary>
    /// Main application window, collecting frames for all editable files.
    /// </summary>
    public partial class MainWindow : Window {
        /// <summary>
        /// Main application window, collecting frames for all editable files.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            gameBrowser.SelectedPath = Settings.Default.lastGameLocation;
        }

        /// <summary>
        /// Check for unsaved changes before the user tries to close the window, and save the last loaded file paths if the window can close.
        /// </summary>
        void Window_Closing(object _, CancelEventArgs e) {
            e.Cancel = !questionEditor.OnClose() || !finalRoundEditor.OnClose() ||
                !worstDrawingEditor.OnClose() || !worstResponseEditor.OnClose();
            if (!e.Cancel) {
                if (!string.IsNullOrEmpty(gameBrowser.SelectedPath)) {
                    Settings.Default.lastGameLocation = gameBrowser.SelectedPath;
                }
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// When selecting The Jackbox Party Pack 3's root folder, open the corresponding game files for all editors.
        /// </summary>
        void ImportAll(object _, RoutedEventArgs e) {
            if (gameBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string directory = Path.Combine(gameBrowser.SelectedPath, "games", "TriviaDeath", "content");
                questionEditor.ImportReference(directory);
                finalRoundEditor.ImportReference(directory);
                worstDrawingEditor.ImportReference(directory);
                worstResponseEditor.ImportReference(directory);
            }
        }

        /// <summary>
        /// Used for finding The Jackbox Party Pack 3's root folder.
        /// </summary>
        readonly static FolderBrowserDialog gameBrowser = new() { Description = Properties.Resources.selectPack };
    }
}