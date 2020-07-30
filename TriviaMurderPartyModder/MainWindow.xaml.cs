using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;

namespace TriviaMurderPartyModder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly OpenFileDialog audioBrowser = new OpenFileDialog { Filter = "Ogg Vorbis Audio (*.ogg)|*.ogg" };

        readonly Questions questionList = new Questions();
        readonly FinalRounders finalRoundList = new FinalRounders();
        readonly WorstDrawings worstDrawingList = new WorstDrawings();
        readonly WorstResponses worstResponseList = new WorstResponses();

        FinalRounder selectedTopic;
        FinalRounderChoice selectedChoice;

        public MainWindow() {
            InitializeComponent();
            string lastQuestion = Properties.Settings.Default.lastQuestion;
            if (string.IsNullOrEmpty(lastQuestion) || !File.Exists(lastQuestion))
                questionLast.IsEnabled = false;
            string lastFinalRound = Properties.Settings.Default.lastFinalRound;
            if (string.IsNullOrEmpty(lastFinalRound) || !File.Exists(lastFinalRound))
                finalRoundLast.IsEnabled = false;
            string lastWorstDrawing = Properties.Settings.Default.lastWorstDrawing;
            if (string.IsNullOrEmpty(lastWorstDrawing) || !File.Exists(lastWorstDrawing))
                worstDrawingLast.IsEnabled = false;
            string lastWorstResponse = Properties.Settings.Default.lastWorstResponse;
            if (string.IsNullOrEmpty(lastWorstResponse) || !File.Exists(lastWorstResponse))
                worstResponseLast.IsEnabled = false;
            questions.ItemsSource = questionList;
            finalRounders.ItemsSource = finalRoundList;
            worstDrawings.ItemsSource = worstDrawingList;
            worstResponses.ItemsSource = worstResponseList;
            questions.CellEditEnding += Questions_CellEditEnding;
            worstDrawings.CellEditEnding += WorstDrawings_CellEditEnding;
        }

        void LoadAudio() {
        }

        void MoveRight(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && e.OriginalSource is UIElement source) {
                e.Handled = true;
                source.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        void Window_Closing(object sender, CancelEventArgs e) {
            e.Cancel = !questionList.UnsavedPrompt() || !finalRoundList.UnsavedPrompt() || !worstDrawingList.UnsavedPrompt();
            if (!e.Cancel) {
                Properties.Settings.Default.lastQuestion = questionList.FileName;
                Properties.Settings.Default.lastFinalRound = finalRoundList.FileName;
                Properties.Settings.Default.lastWorstDrawing = worstDrawingList.FileName;
                Properties.Settings.Default.lastWorstResponse = worstResponseList.FileName;
                Properties.Settings.Default.Save();
            }
        }

        void Questions_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => questionList.Unsaved = true;
        void QuestionImport(object sender, RoutedEventArgs e) => questionList.Import(true);
        void QuestionImportLastSave(object sender, RoutedEventArgs e) => questionList.ImportFrom(Properties.Settings.Default.lastQuestion);
        void QuestionMerge(object sender, RoutedEventArgs e) => questionList.Import(false);
        void QuestionSave(object sender, RoutedEventArgs e) => questionList.Save();
        void QuestionSaveAs(object sender, RoutedEventArgs e) => questionList.SaveAs();

        void QuestionReleaseCheck(object sender, RoutedEventArgs e) {
            string questionFileDir = null;
            if (questionList.FileName != null)
                questionFileDir = Path.Combine(Path.GetDirectoryName(questionList.FileName), "TDQuestion");
            for (int i = 0, end = questionList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (questionList[i].ID == questionList[j].ID) {
                        Questions.QuestionIssue(string.Format("There are multiple {0} IDs.", questionList[i].ID));
                        return;
                    }
                }
                if (questionList.FileName != null && !Parsing.CheckAudio(questionFileDir, questionList[i].ID)) {
                    Questions.QuestionIssue(string.Format("Audio files are missing for question ID {0}.", questionList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This question set is compatible with the game.", "Release check result");
        }

        string LoadQuestionAudio() {
            if (questions.SelectedItem == null) {
                Questions.QuestionIssue("Select the question to import the audio of.");
                return null;
            }
            if (questionList.FileName == null) {
                Questions.QuestionIssue("The question file has to exist first. Export your work or import an existing question file.");
                return null;
            }
            if (audioBrowser.ShowDialog() == true)
                return audioBrowser.FileName;
            return null;
        }

        void ImportQuestionAudio(AudioType type) {
            string file = LoadQuestionAudio();
            if (file != null)
                ((Question)questions.SelectedItem).ImportAudio(questionList.FileName, type, file);
        }

        void QuestionAudio(object sender, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Q);

        void QuestionIntroAudio(object sender, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Intro);

        void QuestionRemove(object sender, RoutedEventArgs e) {
            if (questions.SelectedItem == null) {
                Questions.QuestionIssue("Select the question to remove.");
                return;
            }
            questionList.Remove((Question)questions.SelectedItem);
        }

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

        void AddTopic(object sender, RoutedEventArgs e) {
            FinalRounder newTopic = new FinalRounder(0, "New topic");
            finalRoundList.Add(newTopic);
            newTopic.IsSelected = true;
            topic.SelectAll();
            topic.Focus();
        }

        void AddTopicChoice(object sender, RoutedEventArgs e) {
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

        void AddTopicChoices(object sender, RoutedEventArgs e) {
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

        void AddTopicAudio(object sender, RoutedEventArgs e) {
            if (finalRounders.SelectedItem == null) {
                FinalRounders.FinalRoundIssue("Select the topic to import the audio of.");
                return;
            }
            if (finalRoundList.FileName == null) {
                FinalRounders.FinalRoundIssue("The final round file has to exist first. Export your work or import an existing final round file.");
                return;
            }
            if (audioBrowser.ShowDialog() == true)
                selectedTopic.ImportTopicAudio(finalRoundList.FileName, audioBrowser.FileName);
        }

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

        void RemoveTopic(object sender, RoutedEventArgs e) {
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

        void RemoveChoice(object sender, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedTopic.Items.Remove(selectedChoice);
                DeselectChoice();
                finalRoundList.Unsaved = true;
            }
        }

        void FinalRoundImport(object sender, RoutedEventArgs e) => finalRoundList.Import(true);
        void FinalRoundImportLastSave(object sender, RoutedEventArgs e) => finalRoundList.ImportFrom(Properties.Settings.Default.lastFinalRound);
        void FinalRoundMerge(object sender, RoutedEventArgs e) => finalRoundList.Import(false);
        void FinalRoundSave(object sender, RoutedEventArgs e) => finalRoundList.Save();
        void FinalRoundSaveAs(object sender, RoutedEventArgs e) => finalRoundList.SaveAs();

        void FinalRoundReleaseCheck(object sender, RoutedEventArgs e) {
            string finalRoundFileDir = null;
            if (finalRoundList.FileName != null)
                finalRoundFileDir = Path.Combine(Path.GetDirectoryName(finalRoundList.FileName), "TDFinalRound");
            for (int i = 0, end = finalRoundList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (finalRoundList[i].ID == finalRoundList[j].ID) {
                        FinalRounders.FinalRoundIssue(string.Format("There are multiple {0} IDs.", finalRoundList[i].ID));
                        return;
                    }
                }
                if (finalRoundList[i].Items.Count < 3) {
                    FinalRounders.FinalRoundIssue(string.Format("{0} has less than 3 answers.", finalRoundList[i].Text));
                    return;
                }
                if (finalRoundList.FileName != null && !Parsing.CheckAudio(finalRoundFileDir, finalRoundList[i].ID)) {
                    FinalRounders.FinalRoundIssue(string.Format("Audio files are missing for topic ID {0}.", finalRoundList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This topic set is compatible with the game.", "Release check result");
        }

        void WorstDrawings_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => worstDrawingList.Unsaved = true;
        void WorstDrawingImport(object _, RoutedEventArgs e) => worstDrawingList.Import(true);
        void WorstDrawingImportLastSave(object _, RoutedEventArgs e) => worstDrawingList.ImportFrom(Properties.Settings.Default.lastWorstDrawing);
        void WorstDrawingMerge(object _, RoutedEventArgs e) => worstDrawingList.Import(false);
        void WorstDrawingSave(object _, RoutedEventArgs e) => worstDrawingList.Save();
        void WorstDrawingSaveAs(object _, RoutedEventArgs e) => worstDrawingList.SaveAs();

        void WorstDrawingReleaseCheck(object sender, RoutedEventArgs e) {
            string fileDir = null;
            if (worstDrawingList.FileName != null)
                fileDir = Path.Combine(Path.GetDirectoryName(worstDrawingList.FileName), "TDWorstDrawing");
            for (int i = 0, end = worstDrawingList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (worstDrawingList[i].ID == worstDrawingList[j].ID) {
                        WorstDrawings.DrawingIssue(string.Format("There are multiple {0} IDs.", worstDrawingList[i].ID));
                        return;
                    }
                }
                if (worstDrawingList.FileName != null && !Parsing.CheckAudio(fileDir, worstDrawingList[i].ID)) {
                    WorstDrawings.DrawingIssue(string.Format("Audio files are missing for drawing ID {0}.", worstDrawingList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This drawing set is compatible with the game.", "Release check result");
        }

        void WorstDrawingAudio(object sender, RoutedEventArgs e) {
            if (worstDrawings.SelectedItem == null) {
                WorstDrawings.DrawingIssue("Select the drawing to import the audio of.");
                return;
            }
            if (worstDrawingList.FileName == null) {
                WorstDrawings.DrawingIssue("The drawings file has to exist first. Export your work or import an existing drawings file.");
                return;
            }
            if (audioBrowser.ShowDialog() == true)
                ((WorstDrawing)worstDrawings.SelectedItem).ImportAudio(worstDrawingList.FileName, audioBrowser.FileName);
        }

        void WorstDrawingRemove(object sender, RoutedEventArgs e) {
            if (worstDrawings.SelectedItem == null) {
                WorstDrawings.DrawingIssue("Select the drawing to remove.");
                return;
            }
            worstDrawingList.Remove((WorstDrawing)worstDrawings.SelectedItem);
        }

        void WorstResponses_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => worstResponseList.Unsaved = true;
        void WorstResponseImport(object _, RoutedEventArgs e) => worstResponseList.Import(true);
        void WorstResponseImportLastSave(object _, RoutedEventArgs e) => worstResponseList.ImportFrom(Properties.Settings.Default.lastWorstResponse);
        void WorstResponseMerge(object _, RoutedEventArgs e) => worstResponseList.Import(false);
        void WorstResponseSave(object _, RoutedEventArgs e) => worstResponseList.Save();
        void WorstResponseSaveAs(object _, RoutedEventArgs e) => worstResponseList.SaveAs();

        void WorstResponseReleaseCheck(object sender, RoutedEventArgs e) {
            string fileDir = null;
            if (worstResponseList.FileName != null)
                fileDir = Path.Combine(Path.GetDirectoryName(worstResponseList.FileName), "TDWorstResponse");
            for (int i = 0, end = worstResponseList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (worstResponseList[i].ID == worstResponseList[j].ID) {
                        WorstResponses.ResponseIssue(string.Format("There are multiple {0} IDs.", worstResponseList[i].ID));
                        return;
                    }
                }
                if (worstResponseList.FileName != null && !Parsing.CheckAudio(fileDir, worstResponseList[i].ID)) {
                    WorstResponses.ResponseIssue(string.Format("Audio files are missing for response ID {0}.", worstResponseList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This response set is compatible with the game.", "Release check result");
        }

        void WorstResponseAudio(object sender, RoutedEventArgs e) {
            if (worstResponses.SelectedItem == null) {
                WorstResponses.ResponseIssue("Select the response to import the audio of.");
                return;
            }
            if (worstResponseList.FileName == null) {
                WorstResponses.ResponseIssue("The response file has to exist first. Export your work or import an existing response file.");
                return;
            }
            if (audioBrowser.ShowDialog() == true)
                ((WorstResponse)worstResponses.SelectedItem).ImportAudio(worstResponseList.FileName, audioBrowser.FileName);
        }

        void WorstResponseRemove(object sender, RoutedEventArgs e) {
            if (worstResponses.SelectedItem == null) {
                WorstResponses.ResponseIssue("Select the response to remove.");
                return;
            }
            worstResponseList.Remove((WorstResponse)worstResponses.SelectedItem);
        }
    }
}