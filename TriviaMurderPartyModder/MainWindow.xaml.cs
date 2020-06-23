using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TriviaMurderPartyModder.Data;

namespace TriviaMurderPartyModder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly Questions questionList = new Questions();
        readonly FinalRounders finalRoundList = new FinalRounders();

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
            questions.ItemsSource = questionList;
            finalRounders.ItemsSource = finalRoundList;
            questions.CellEditEnding += Questions_CellEditEnding;
        }

        void MoveRight(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && e.OriginalSource is UIElement source) {
                e.Handled = true;
                source.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        void Window_Closing(object sender, CancelEventArgs e) {
            e.Cancel = !questionList.UnsavedPrompt() || !finalRoundList.UnsavedPrompt();
            if (!e.Cancel) {
                Properties.Settings.Default.lastQuestion = questionList.FileName;
                Properties.Settings.Default.lastFinalRound = finalRoundList.FileName;
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
            OpenFileDialog opener = new OpenFileDialog { Filter = "Ogg Vorbis Audio (*.ogg)|*.ogg" };
            if (opener.ShowDialog() == true)
                return opener.FileName;
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
            } else {
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

        void AddTopicAudio(object sender, RoutedEventArgs e) {
            if (finalRounders.SelectedItem == null) {
                Questions.QuestionIssue("Select the topic to import the audio of.");
                return;
            }
            if (finalRoundList.FileName == null) {
                Questions.QuestionIssue("The final round file has to exist first. Export your work or import an existing final round file.");
                return;
            }
            OpenFileDialog opener = new OpenFileDialog { Filter = "Ogg Vorbis Audio (*.ogg)|*.ogg" };
            if (opener.ShowDialog() == true)
                selectedTopic.ImportTopicAudio(finalRoundList.FileName, opener.FileName);
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
                        Questions.QuestionIssue(string.Format("There are multiple {0} IDs.", finalRoundList[i].ID));
                        return;
                    }
                }
                if (finalRoundList.FileName != null && !Parsing.CheckAudio(finalRoundFileDir, finalRoundList[i].ID)) {
                    Questions.QuestionIssue(string.Format("Audio files are missing for topic ID {0}.", finalRoundList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This topic set is compatible with the game.", "Release check result");
        }
    }
}