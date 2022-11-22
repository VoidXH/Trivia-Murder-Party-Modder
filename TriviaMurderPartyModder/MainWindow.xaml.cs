using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace TriviaMurderPartyModder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly OpenFileDialog audioBrowser = new OpenFileDialog { Filter = Properties.Resources.oggFilter };
        readonly FolderBrowserDialog gameBrowser = new FolderBrowserDialog() { Description = Properties.Resources.selectPack };

        readonly Questions questionList = new Questions();
        readonly FinalRounders finalRoundList = new FinalRounders();
        readonly WorstDrawings worstDrawingList = new WorstDrawings();
        readonly WorstResponses worstResponseList = new WorstResponses();

        FinalRounder selectedTopic;
        FinalRounderChoice selectedChoice;

        public MainWindow() {
            InitializeComponent();
            string lastQuestion = Settings.Default.lastQuestion;
            if (string.IsNullOrEmpty(lastQuestion) || !File.Exists(lastQuestion))
                questionLast.IsEnabled = false;
            string lastFinalRound = Settings.Default.lastFinalRound;
            if (string.IsNullOrEmpty(lastFinalRound) || !File.Exists(lastFinalRound))
                finalRoundLast.IsEnabled = false;
            string lastWorstDrawing = Settings.Default.lastWorstDrawing;
            if (string.IsNullOrEmpty(lastWorstDrawing) || !File.Exists(lastWorstDrawing))
                worstDrawingLast.IsEnabled = false;
            string lastWorstResponse = Settings.Default.lastWorstResponse;
            if (string.IsNullOrEmpty(lastWorstResponse) || !File.Exists(lastWorstResponse))
                worstResponseLast.IsEnabled = false;
            gameBrowser.SelectedPath = Settings.Default.lastGameLocation;
            questions.ItemsSource = questionList;
            finalRounders.ItemsSource = finalRoundList;
            worstDrawings.ItemsSource = worstDrawingList;
            worstResponses.ItemsSource = worstResponseList;
            questions.CellEditEnding += Questions_CellEditEnding;
            worstDrawings.CellEditEnding += WorstDrawings_CellEditEnding;
        }

        void MoveRight(object _, KeyEventArgs e) {
            if (e.Key == Key.Enter && e.OriginalSource is UIElement source) {
                e.Handled = true;
                source.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        void Window_Closing(object _, CancelEventArgs e) {
            e.Cancel = !questionList.UnsavedPrompt() || !finalRoundList.UnsavedPrompt() ||
                !worstDrawingList.UnsavedPrompt() || !worstResponseList.UnsavedPrompt();
            if (!e.Cancel) {
                if (!string.IsNullOrEmpty(questionList.FileName))
                    Settings.Default.lastQuestion = questionList.FileName;
                if (!string.IsNullOrEmpty(finalRoundList.FileName))
                    Settings.Default.lastFinalRound = finalRoundList.FileName;
                if (!string.IsNullOrEmpty(worstDrawingList.FileName))
                    Settings.Default.lastWorstDrawing = worstDrawingList.FileName;
                if (!string.IsNullOrEmpty(worstResponseList.FileName))
                    Settings.Default.lastWorstResponse = worstResponseList.FileName;
                if (!string.IsNullOrEmpty(gameBrowser.SelectedPath))
                    Settings.Default.lastGameLocation = gameBrowser.SelectedPath;
                Settings.Default.Save();
            }
        }

        void ImportAll(object _, RoutedEventArgs e) {
            if (gameBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                string directory = Path.Combine(gameBrowser.SelectedPath, "games", "TriviaDeath", "content");
                questionList.ImportReference(directory);
                finalRoundList.ImportReference(directory);
                worstDrawingList.ImportReference(directory);
                worstResponseList.ImportReference(directory);
            }
        }

        string LoadAudio<T>(DataGrid grid, DataFile<T> list) {
            if (grid.SelectedItem == null) {
                list.Issue(Properties.Resources.noAudioImportSelection);
                return null;
            }
            if (list.FileName == null) {
                list.Issue(Properties.Resources.noSavedFile);
                return null;
            }
            if (audioBrowser.ShowDialog() == true)
                return audioBrowser.FileName;
            return null;
        }

        static void RemoveElement<T>(DataGrid grid, DataFile<T> file) {
            if (grid.SelectedItem == null) {
                file.Issue(Properties.Resources.noRemovable);
                return;
            }
            file.Remove((T)grid.SelectedItem);
        }

        static void ReleaseCheck<T>(DataFile<T> list) where T : Entry {
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
    }
}