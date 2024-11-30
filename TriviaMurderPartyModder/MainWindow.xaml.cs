using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

using TriviaMurderPartyModder.Dialogs;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace TriviaMurderPartyModder {
    /// <summary>
    /// Main application window, collecting frames for all editable files.
    /// </summary>
    public partial class MainWindow : Window {
        readonly FinalRounders finalRoundList = [];
        readonly WorstDrawings worstDrawingList = [];
        readonly WorstResponses worstResponseList = [];

        /// <summary>
        /// Main application window, collecting frames for all editable files.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            finalRoundLast.EnableIfHasSave(Settings.Default.lastFinalRound);
            worstDrawingLast.EnableIfHasSave(Settings.Default.lastWorstDrawing);
            worstResponseLast.EnableIfHasSave(Settings.Default.lastWorstResponse);
            gameBrowser.SelectedPath = Settings.Default.lastGameLocation;
            finalRounders.ItemsSource = finalRoundList;
            worstDrawings.ItemsSource = worstDrawingList;
            worstResponses.ItemsSource = worstResponseList;
            worstDrawings.CellEditEnding += WorstDrawings_CellEditEnding;
        }

        void MoveRight(object _, KeyEventArgs e) => WPFExtensions.MoveRight(e);

        void Window_Closing(object _, CancelEventArgs e) {
            e.Cancel = !questionEditor.OnClose() || !finalRoundList.UnsavedPrompt() ||
                !worstDrawingList.UnsavedPrompt() || !worstResponseList.UnsavedPrompt();
            if (!e.Cancel) {
                if (!string.IsNullOrEmpty(finalRoundList.FileName)) {
                    Settings.Default.lastFinalRound = finalRoundList.FileName;
                }
                if (!string.IsNullOrEmpty(worstDrawingList.FileName)) {
                    Settings.Default.lastWorstDrawing = worstDrawingList.FileName;
                }
                if (!string.IsNullOrEmpty(worstResponseList.FileName)) {
                    Settings.Default.lastWorstResponse = worstResponseList.FileName;
                }
                if (!string.IsNullOrEmpty(gameBrowser.SelectedPath)) {
                    Settings.Default.lastGameLocation = gameBrowser.SelectedPath;
                }
                Settings.Default.Save();
            }
        }

        void ImportAll(object _, RoutedEventArgs e) {
            if (gameBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string directory = Path.Combine(gameBrowser.SelectedPath, "games", "TriviaDeath", "content");
                questionEditor.ImportReference(directory);
                finalRoundList.ImportReference(directory);
                worstDrawingList.ImportReference(directory);
                worstResponseList.ImportReference(directory);
            }
        }

        /// <summary>
        /// Used for finding The Jackbox Party Pack 3's root folder.
        /// </summary>
        readonly static FolderBrowserDialog gameBrowser = new() { Description = Properties.Resources.selectPack };
    }
}