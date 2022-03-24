using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;
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

        void Questions_CellEditEnding(object _, DataGridCellEditEndingEventArgs e) => questionList.Unsaved = true;
        void QuestionImport(object _, RoutedEventArgs e) => questionList.Import(true);
        void QuestionImportLastSave(object _, RoutedEventArgs e) => questionList.ImportFrom(Settings.Default.lastQuestion);
        void QuestionMerge(object _, RoutedEventArgs e) => questionList.Import(false);
        void QuestionSave(object _, RoutedEventArgs e) => questionList.Save();
        void QuestionSaveAs(object _, RoutedEventArgs e) => questionList.SaveAs();

        void QuestionReleaseCheck(object _, RoutedEventArgs e) {
            string questionFileDir = null;
            if (questionList.FileName != null)
                questionFileDir = questionList.DataFolderPath;
            for (int i = 0, end = questionList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (questionList[i].ID == questionList[j].ID) {
                        questionList.Issue(string.Format(Properties.Resources.multipleIDs, questionList[i].ID));
                        return;
                    }
                }
                if (questionList.FileName != null && !Parsing.CheckAudio(questionFileDir, questionList[i].ID)) {
                    questionList.Issue(string.Format(Properties.Resources.missingAudio, questionList[i].ID));
                    return;
                }
            }
            MessageBox.Show(Properties.Resources.checkSuccess, Properties.Resources.checkResult);
        }

        void ImportQuestionAudio(AudioType type) =>
            ((Question)questions.SelectedItem).ImportAudio(questionList, type, LoadAudio(questions, questionList));

        void QuestionAudio(object _, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Q);
        void QuestionIntroAudio(object _, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Intro);
        void QuestionRemove(object _, RoutedEventArgs e) => RemoveElement(questions, questionList);

        void SelectFinalQuestion(FinalRounder question) {
            selectedTopic = question;
            topicId.Text = question.ID.ToString();
            topic.Text = question.Text;
        }

        void DeselectChoice() {
            selectedChoice = null;
            choiceCorrect.IsChecked = false;
            choiceAnswer.Text = string.Empty;
        }

        void FinalRoundSelection(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem selected = (TreeViewItem)((TreeView)sender).SelectedItem;
            if (selected is FinalRounder topic) {
                DeselectChoice();
                SelectFinalQuestion(topic);
            } else if (selected != null) {
                selectedChoice = (FinalRounderChoice)selected;
                choiceCorrect.IsChecked = selectedChoice.Correct;
                choiceAnswer.Text = selectedChoice.Text;
                SelectFinalQuestion((FinalRounder)selected.Parent);
            }
        }

        void AddTopic(object _, RoutedEventArgs e) {
            FinalRounder newTopic = new FinalRounder(0, "New topic");
            finalRoundList.Add(newTopic);
            newTopic.IsSelected = true;
            topic.SelectAll();
            topic.Focus();
        }

        void AddTopicChoice(object _, RoutedEventArgs e) {
            if (selectedTopic != null) {
                FinalRounderChoice choice = new FinalRounderChoice(false, "New choice");
                selectedTopic.Items.Add(choice);
                selectedTopic.IsExpanded = true;
                choice.IsSelected = true;
                choiceAnswer.SelectAll();
                choiceAnswer.Focus();
                finalRoundList.Unsaved = true;
            }
        }

        void AddTopicChoices(object _, RoutedEventArgs e) {
            if (selectedTopic != null) {
                BulkOption form = new BulkOption();
                bool? result = form.ShowDialog();
                if (result.HasValue && result.Value) {
                    selectedTopic.IsExpanded = true;
                    finalRoundList.Unsaved = true;
                    string[] correct = form.CorrectValues, incorrect = form.IncorrectValues;
                    for (int i = 0; i < correct.Length; ++i) {
                        FinalRounderChoice choice = new FinalRounderChoice(true, correct[i]);
                        selectedTopic.Items.Add(choice);
                    }
                    for (int i = 0; i < incorrect.Length; ++i) {
                        FinalRounderChoice choice = new FinalRounderChoice(false, incorrect[i]);
                        selectedTopic.Items.Add(choice);
                    }
                }
            }
        }

        void AddTopicAudio(object _, RoutedEventArgs e) =>
            selectedTopic.ImportTopicAudio(finalRoundList, LoadAudio(questions, questionList));

        void TopicIDChange(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox)sender;
            if (!int.TryParse(box.Text, out int id)) {
                box.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                return;
            }
            box.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            if (selectedTopic != null) {
                selectedTopic.ID = id;
                finalRoundList.Unsaved = true;
            }
        }

        void TopicChange(object sender, TextChangedEventArgs e) {
            if (selectedTopic != null) {
                selectedTopic.Text = ((TextBox)sender).Text;
                finalRoundList.Unsaved = true;
            }
        }

        void RemoveTopic(object _, RoutedEventArgs e) {
            if (selectedTopic != null) {
                DeselectChoice();
                finalRoundList.Remove(selectedTopic);
            }
        }

        void ChoiceCorrect(object sender, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedChoice.Correct = ((CheckBox)sender).IsChecked.Value;
                finalRoundList.Unsaved = true;
            }
        }

        void ChoiceText(object sender, TextChangedEventArgs e) {
            if (selectedChoice != null) {
                selectedChoice.Text = ((TextBox)sender).Text;
                finalRoundList.Unsaved = true;
            }
        }

        void RemoveChoice(object _, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedTopic.Items.Remove(selectedChoice);
                DeselectChoice();
                finalRoundList.Unsaved = true;
            }
        }

        void FinalRoundImport(object _, RoutedEventArgs e) => finalRoundList.Import(true);
        void FinalRoundImportLastSave(object _, RoutedEventArgs e) =>
            finalRoundList.ImportFrom(Settings.Default.lastFinalRound);
        void FinalRoundMerge(object _, RoutedEventArgs e) => finalRoundList.Import(false);
        void FinalRoundSave(object _, RoutedEventArgs e) => finalRoundList.Save();
        void FinalRoundSaveAs(object _, RoutedEventArgs e) => finalRoundList.SaveAs();

        void FinalRoundReleaseCheck(object _, RoutedEventArgs e) {
            string finalRoundFileDir = null;
            if (finalRoundList.FileName != null)
                finalRoundFileDir = finalRoundList.DataFolderPath;
            for (int i = 0, end = finalRoundList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (finalRoundList[i].ID == finalRoundList[j].ID) {
                        FinalRounders.FinalRoundIssue(string.Format(Properties.Resources.multipleIDs, finalRoundList[i].ID));
                        return;
                    }
                }
                if (finalRoundList[i].Items.Count < 3) {
                    FinalRounders.FinalRoundIssue(string.Format("{0} has less than 3 choices.", finalRoundList[i].Text));
                    return;
                }
                if (finalRoundList.FileName != null && !Parsing.CheckAudio(finalRoundFileDir, finalRoundList[i].ID)) {
                    FinalRounders.FinalRoundIssue(string.Format(Properties.Resources.missingAudio, finalRoundList[i].ID));
                    return;
                }
            }
            MessageBox.Show(Properties.Resources.checkSuccess, Properties.Resources.checkResult);
        }

        void WorstDrawings_CellEditEnding(object _, DataGridCellEditEndingEventArgs e) => worstDrawingList.Unsaved = true;
        void WorstDrawingImport(object _, RoutedEventArgs e) => worstDrawingList.Import(true);
        void WorstDrawingImportLastSave(object _, RoutedEventArgs e) =>
            worstDrawingList.ImportFrom(Settings.Default.lastWorstDrawing);
        void WorstDrawingMerge(object _, RoutedEventArgs e) => worstDrawingList.Import(false);
        void WorstDrawingSave(object _, RoutedEventArgs e) => worstDrawingList.Save();
        void WorstDrawingSaveAs(object _, RoutedEventArgs e) => worstDrawingList.SaveAs();

        void WorstDrawingReleaseCheck(object _, RoutedEventArgs e) {
            string fileDir = null;
            if (worstDrawingList.FileName != null)
                fileDir = worstDrawingList.DataFolderPath;
            for (int i = 0, end = worstDrawingList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (worstDrawingList[i].ID == worstDrawingList[j].ID) {
                        WorstDrawings.DrawingIssue(string.Format(Properties.Resources.multipleIDs, worstDrawingList[i].ID));
                        return;
                    }
                }
                if (worstDrawingList.FileName != null && !Parsing.CheckAudio(fileDir, worstDrawingList[i].ID)) {
                    WorstDrawings.DrawingIssue(string.Format(Properties.Resources.missingAudio, worstDrawingList[i].ID));
                    return;
                }
            }
            MessageBox.Show(Properties.Resources.checkSuccess, Properties.Resources.checkResult);
        }

        void WorstDrawingAudio(object _, RoutedEventArgs e) =>
            ((WorstDrawing)worstDrawings.SelectedItem).ImportAudio(worstDrawingList, LoadAudio(questions, questionList));
        void WorstDrawingRemove(object _, RoutedEventArgs e) => RemoveElement(worstDrawings, worstDrawingList);

        void WorstResponses_CellEditEnding(object _, DataGridCellEditEndingEventArgs e) => worstResponseList.Unsaved = true;
        void WorstResponseImport(object _, RoutedEventArgs e) => worstResponseList.Import(true);
        void WorstResponseImportLastSave(object _, RoutedEventArgs e) =>
            worstResponseList.ImportFrom(Settings.Default.lastWorstResponse);
        void WorstResponseMerge(object _, RoutedEventArgs e) => worstResponseList.Import(false);
        void WorstResponseSave(object _, RoutedEventArgs e) => worstResponseList.Save();
        void WorstResponseSaveAs(object _, RoutedEventArgs e) => worstResponseList.SaveAs();

        void WorstResponseReleaseCheck(object _, RoutedEventArgs e) {
            string fileDir = null;
            if (worstResponseList.FileName != null)
                fileDir = worstResponseList.DataFolderPath;
            for (int i = 0, end = worstResponseList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (worstResponseList[i].ID == worstResponseList[j].ID) {
                        WorstResponses.ResponseIssue(string.Format(Properties.Resources.multipleIDs, worstResponseList[i].ID));
                        return;
                    }
                }
                if (worstResponseList.FileName != null && !Parsing.CheckAudio(fileDir, worstResponseList[i].ID)) {
                    WorstResponses.ResponseIssue(string.Format(Properties.Resources.missingAudio, worstResponseList[i].ID));
                    return;
                }
            }
            MessageBox.Show(Properties.Resources.checkSuccess, Properties.Resources.checkResult);
        }

        void WorstResponseAudio(object _, RoutedEventArgs e) =>
            ((WorstResponse)worstResponses.SelectedItem).ImportAudio(worstResponseList, LoadAudio(questions, questionList));
        void WorstResponseRemove(object _, RoutedEventArgs e) => RemoveElement(worstResponses, worstResponseList);
    }
}