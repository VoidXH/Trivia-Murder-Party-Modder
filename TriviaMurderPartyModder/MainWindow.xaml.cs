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
        readonly OpenFileDialog opener = new OpenFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };
        readonly SaveFileDialog saver = new SaveFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };

        readonly Questions questionList = new Questions();
        readonly FinalRounders finalRoundList = new FinalRounders();

        bool unsavedQuestion, unsavedTopic;
        FinalRounder selectedTopic;
        FinalRounderChoice selectedChoice;
        string questionFile;
        string finalRoundFile;

        bool UnsavedPrompt(bool hasUnsaved, string items) {
            if (hasUnsaved)
                return MessageBox.Show(string.Format("You have unsaved {0}. Do you want to discard them?", items), "Unsaved " + items,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            return true;
        }

        bool UnsavedQuestionPrompt() => UnsavedPrompt(unsavedQuestion, "questions");
        bool UnsavedTopicPrompt() => UnsavedPrompt(unsavedTopic, "final round topics");

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

        private void Questions_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => unsavedQuestion = true;

        void MoveRight(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && e.OriginalSource is UIElement source) {
                e.Handled = true;
                source.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        void Window_Closing(object sender, CancelEventArgs e) {
            e.Cancel = !UnsavedQuestionPrompt() || !UnsavedTopicPrompt();
            if (!e.Cancel) {
                Properties.Settings.Default.lastQuestion = questionFile;
                Properties.Settings.Default.lastFinalRound = finalRoundFile;
                Properties.Settings.Default.Save();
            }
        }

        void QuestionImport(bool clear) {
            if (!clear || UnsavedQuestionPrompt()) {
                if (opener.ShowDialog() == true) {
                    if (clear)
                        questionList.Clear();
                    questionList.Add(questionFile = opener.FileName);
                }
                unsavedQuestion = !clear;
            }
        }

        void QuestionImport(object sender, RoutedEventArgs e) => QuestionImport(true);

        void QuestionImportLastSave(object sender, RoutedEventArgs e) {
            if (UnsavedQuestionPrompt()) {
                questionList.Clear();
                questionList.Add(questionFile = Properties.Settings.Default.lastQuestion);
                unsavedQuestion = false;
                questionLast.IsEnabled = false;
            }
        }

        void QuestionMerge(object sender, RoutedEventArgs e) => QuestionImport(false);

        void QuestionSave(object sender, RoutedEventArgs e) {
            if (questionFile == null)
                QuestionSaveAs(sender, e);
            else if (questionList.SaveAs(questionFile))
                unsavedQuestion = false;
        }

        void QuestionSaveAs(object sender, RoutedEventArgs e) {
            if (saver.ShowDialog() == true && questionList.SaveAs(saver.FileName)) {
                questionFile = saver.FileName;
                unsavedQuestion = false;
            }
        }

        void ReleaseCheck(object sender, RoutedEventArgs e) {
            string questionFileDir = null;
            if (questionFile != null)
                questionFileDir = Path.Combine(Path.GetDirectoryName(questionFile), "TDQuestion");
            for (int i = 0, end = questionList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (questionList[i].ID == questionList[j].ID) {
                        Questions.QuestionIssue(string.Format("There are multiple {0} IDs.", questionList[i].ID));
                        return;
                    }
                }
                if (questionFile != null && !Parsing.CheckAudio(questionFileDir, questionList[i].ID)) {
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
            if (questionFile == null) {
                Questions.QuestionIssue("The question file has to exist first. Export your work or import an existing question file.");
                return null;
            }
            OpenFileDialog opener = new OpenFileDialog { Filter = "Ogg Vorbis Audio (*.ogg)|*.ogg" };
            if (opener.ShowDialog() == true)
                return opener.FileName;
            return null;
        }

        void QuestionAudio(object sender, RoutedEventArgs e) {
            string file = LoadQuestionAudio();
            if (file != null)
                ((Question)questions.SelectedItem).ImportQuestionAudio(questionFile, file);
        }

        void QuestionIntroAudio(object sender, RoutedEventArgs e) {
            string file = LoadQuestionAudio();
            if (file != null)
                ((Question)questions.SelectedItem).ImportIntroAudio(questionFile, file);
        }

        void QuestionRemove(object sender, RoutedEventArgs e) {
            if (questions.SelectedItem == null) {
                Questions.QuestionIssue("Select the question to remove.");
                return;
            }
            questionList.Remove((Question)questions.SelectedItem);
            unsavedQuestion = true;
        }

        void FinalRoundImport(bool clear) {
            if (!clear || UnsavedTopicPrompt()) {
                if (opener.ShowDialog() == true) {
                    if (clear)
                        finalRoundList.Clear();
                    finalRoundList.Add(finalRoundFile = opener.FileName);
                }
                unsavedTopic = !clear;
            }
        }

        void FinalRoundImportLastSave(object sender, RoutedEventArgs e) {
            if (UnsavedQuestionPrompt()) {
                finalRoundList.Clear();
                finalRoundList.Add(questionFile = Properties.Settings.Default.lastFinalRound);
                unsavedQuestion = false;
                finalRoundLast.IsEnabled = false;
            }
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
            unsavedTopic = true;
        }

        void AddTopicChoice(object sender, RoutedEventArgs e) {
            if (selectedTopic != null) {
                FinalRounderChoice choice = new FinalRounderChoice(false, "New choice");
                selectedTopic.Items.Add(choice);
                selectedTopic.IsExpanded = true;
                choice.IsSelected = true;
                choiceAnswer.SelectAll();
                choiceAnswer.Focus();
                unsavedTopic = true;
            }
        }

        void AddTopicAudio(object sender, RoutedEventArgs e) {
            if (finalRounders.SelectedItem == null) {
                Questions.QuestionIssue("Select the topic to import the audio of.");
                return;
            }
            if (finalRoundFile == null) {
                Questions.QuestionIssue("The final round file has to exist first. Export your work or import an existing final round file.");
                return;
            }
            OpenFileDialog opener = new OpenFileDialog { Filter = "Ogg Vorbis Audio (*.ogg)|*.ogg" };
            if (opener.ShowDialog() == true)
                selectedTopic.ImportTopicAudio(finalRoundFile, opener.FileName);
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
                unsavedTopic = true;
            }
        }

        void TopicChange(object sender, TextChangedEventArgs e) {
            if (selectedTopic != null) {
                selectedTopic.Text = ((TextBox)sender).Text;
                unsavedTopic = true;
            }
        }

        void RemoveTopic(object sender, RoutedEventArgs e) {
            if (selectedTopic != null) {
                DeselectChoice();
                finalRoundList.Remove(selectedTopic);
                unsavedTopic = true;
            }
        }

        void ChoiceCorrect(object sender, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedChoice.Correct = ((CheckBox)sender).IsChecked.Value;
                unsavedTopic = true;
            }
        }

        void ChoiceText(object sender, TextChangedEventArgs e) {
            if (selectedChoice != null) {
                selectedChoice.Text = ((TextBox)sender).Text;
                unsavedTopic = true;
            }
        }

        void RemoveChoice(object sender, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedTopic.Items.Remove(selectedChoice);
                DeselectChoice();
                unsavedTopic = true;
            }
        }

        void FinalRoundImport(object sender, RoutedEventArgs e) => FinalRoundImport(true);

        void FinalRoundMerge(object sender, RoutedEventArgs e) => FinalRoundImport(false);

        void FinalRoundSave(object sender, RoutedEventArgs e) {
            if (finalRoundFile == null)
                FinalRoundSaveAs(sender, e);
            else if (finalRoundList.SaveAs(finalRoundFile))
                unsavedTopic = false;
        }

        void FinalRoundSaveAs(object sender, RoutedEventArgs e) {
            if (saver.ShowDialog() == true && finalRoundList.SaveAs(saver.FileName)) {
                finalRoundFile = saver.FileName;
                unsavedTopic = false;
            }
        }

        void FinalRoundReleaseCheck(object sender, RoutedEventArgs e) {
            string finalRoundFileDir = null;
            if (finalRoundFile != null)
                finalRoundFileDir = Path.Combine(Path.GetDirectoryName(finalRoundFile), "TDFinalRound");
            for (int i = 0, end = finalRoundList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (finalRoundList[i].ID == finalRoundList[j].ID) {
                        Questions.QuestionIssue(string.Format("There are multiple {0} IDs.", finalRoundList[i].ID));
                        return;
                    }
                }
                if (finalRoundFile != null && !Parsing.CheckAudio(finalRoundFileDir, finalRoundList[i].ID)) {
                    Questions.QuestionIssue(string.Format("Audio files are missing for topic ID {0}.", finalRoundList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This topic set is compatible with the game.", "Release check result");
        }
    }
}