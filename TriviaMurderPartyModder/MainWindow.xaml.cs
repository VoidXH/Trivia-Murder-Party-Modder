using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TriviaMurderPartyModder.Data;

namespace TriviaMurderPartyModder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        readonly Questions questionList = new Questions();

        bool unsavedChanges = false;
        string questionFile = null;

        bool UnsavedChangesPrompt() {
            if (unsavedChanges)
                return MessageBox.Show("You have unsaved changes. Do you want to discard them?", "Unsaved changes",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            return true;
        }

        public MainWindow() {
            InitializeComponent();
            questions.ItemsSource = questionList;
            questions.CellEditEnding += Questions_CellEditEnding;
        }

        private void Questions_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => unsavedChanges = true;

        void MoveRight(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && e.OriginalSource is UIElement source) {
                e.Handled = true;
                source.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        void Window_Closing(object sender, CancelEventArgs e) => e.Cancel = !UnsavedChangesPrompt();

        void Import(bool clear) {
            if (!clear || UnsavedChangesPrompt()) {
                OpenFileDialog opener = new OpenFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };
                if (opener.ShowDialog() == true) {
                    if (clear)
                        questionList.Clear();
                    questionList.Add(questionFile = opener.FileName);
                }
                unsavedChanges = !clear;
            }
        }

        void QuestionImport(object sender, RoutedEventArgs e) => Import(true);

        void QuestionMerge(object sender, RoutedEventArgs e) => Import(false);

        void QuestionSave(object sender, RoutedEventArgs e) {
            if (questionFile == null)
                QuestionSaveAs(sender, e);
            else if (questionList.SaveAs(questionFile))
                unsavedChanges = false;
        }

        void QuestionSaveAs(object sender, RoutedEventArgs e) {
            SaveFileDialog saver = new SaveFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };
            if (saver.ShowDialog() == true && questionList.SaveAs(saver.FileName)) {
                questionFile = saver.FileName;
                unsavedChanges = false;
            }
        }

        void ReleaseCheck(object sender, RoutedEventArgs e) {
            for (int i = 0, end = questionList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (questionList[i].ID == questionList[j].ID) {
                        Questions.QuestionIssue(string.Format("There are multiple {0} IDs.", questionList[i].ID));
                        return;
                    }
                }
                if (questionFile != null && !questionList[i].CheckAudio(questionFile)) {
                    Questions.QuestionIssue(string.Format("Audio files are missing for question ID {0}.", questionList[i].ID));
                    return;
                }
            }
            MessageBox.Show("Release check successful. This question set is compatible with the game.", "Release check result");
        }

        string LoadAudio() {
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
            string file = LoadAudio();
            if (file != null)
                ((Question)questions.SelectedItem).ImportQuestionAudio(questionFile, file);
        }

        void QuestionIntroAudio(object sender, RoutedEventArgs e) {
            string file = LoadAudio();
            if (file != null)
                ((Question)questions.SelectedItem).ImportIntroAudio(questionFile, file);
        }

        void QuestionRemove(object sender, RoutedEventArgs e) {
            if (questions.SelectedItem == null) {
                Questions.QuestionIssue("Select the question to remove.");
                return;
            }
            questionList.Remove((Question)questions.SelectedItem);
            unsavedChanges = true;
        }
    }
}