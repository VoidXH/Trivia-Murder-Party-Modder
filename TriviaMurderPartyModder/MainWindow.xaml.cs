using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Text;
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

        string MakeTextCompatible(string source) => source.Replace('ő', 'ö').Replace('ű', 'ü').Replace("\"", "\\\"");

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

        void QuestionIssue(string text) => MessageBox.Show(text, "Question issue", MessageBoxButton.OK, MessageBoxImage.Error);

        void QuestionExport(object sender, RoutedEventArgs e) {
            SaveFileDialog saver = new SaveFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };
            if (saver.ShowDialog() == true) {
                StringBuilder output = new StringBuilder("{\"episodeid\":1244,\"content\":[");
                for (int i = 0, end = questionList.Count; i < end; ++i) {
                    Question q = questionList[i];
                    output.Append("{\"x\":false,\"id\":").Append(q.ID);
                    if (q.Text == null) {
                        QuestionIssue(string.Format("No text given for question ID {0}.", q.ID));
                        return;
                    }
                    output.Append(",\"text\":\"").Append(MakeTextCompatible(q.Text)).Append("\",\"pic\": false,\"choices\":[");
                    if (q.Correct < 1 || q.Correct > 4) {
                        QuestionIssue(string.Format("No correct answer set for question \"{0}\".", q.Text));
                        return;
                    }
                    for (int answer = 1; answer <= 4; ++answer) {
                        if (answer != 1)
                            output.Append("},{");
                        else
                            output.Append("{");
                        if (answer == q.Correct)
                            output.Append("\"correct\":true,");
                        if (q[answer] == null) {
                            QuestionIssue(string.Format("No answer {0} for question \"{1}\".", answer, q.Text));
                            return;
                        }
                        output.Append("\"text\":\"").Append(MakeTextCompatible(q[answer])).Append("\"");
                    }
                    if (i != end - 1)
                        output.Append("}]},");
                    else
                        output.Append("}]}]}");
                }
                File.WriteAllText(questionFile = saver.FileName, output.ToString());
                unsavedChanges = false;
            }
        }

        void QuestionImportAudio(object sender, RoutedEventArgs e) {
            if (questions.SelectedItem == null) {
                QuestionIssue("Select the question to import the audio of.");
                return;
            }
            if (questionFile == null) {
                QuestionIssue("The question file has to exist first. Export your work or import an existing question file.");
                return;
            }
            OpenFileDialog opener = new OpenFileDialog { Filter = "Ogg Vorbis Audio (*.ogg)|*.ogg" };
            if (opener.ShowDialog() == true)
                ((Question)questions.SelectedItem).ImportAudio(questionFile, opener.FileName);
        }

        void QuestionRemove(object sender, RoutedEventArgs e) {
            if (questions.SelectedItem == null) {
                QuestionIssue("Select the question to remove.");
                return;
            }
            questionList.Remove((Question)questions.SelectedItem);
            unsavedChanges = true;
        }
    }
}