using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder.Pages {
    /// <summary>
    /// Main question list editor.
    /// </summary>
    public partial class QuestionEditor : UserControl {
        readonly Questions questionList = [];

        public QuestionEditor() {
            InitializeComponent();
            questionLast.EnableIfHasSave(Settings.Default.lastQuestion);
            questions.ItemsSource = questionList;
            questions.CellEditEnding += Questions_CellEditEnding;
        }

        public void ImportReference(string contentPath) => questionList.ImportReference(contentPath);

        public bool OnClose() {
            bool cancel = questionList.UnsavedPrompt();
            if (!cancel && !string.IsNullOrEmpty(questionList.FileName)) {
                Settings.Default.lastQuestion = questionList.FileName;
            }
            return cancel;
        }

        void ImportQuestionAudio(AudioType type) {
            if (questions.SelectedItem is not Question question) {
                return;
            }
            question.ImportAudio(questionList.DataFolderPath, type, AudioHandling.LoadAudio(questions, questionList));
            hasIntro.IsChecked = question.GetIntroAudio(questionList.DataFolderPath);
        }

        void RemoveQuestionAudio(AudioType type) {
            if (questions.SelectedItem is not Question question) {
                return;
            }
            question.RemoveAudio(questionList.DataFolderPath, type);
            hasIntro.IsChecked = question.GetIntroAudio(questionList.DataFolderPath);
        }

        void QuestionSelected(object _, SelectionChangedEventArgs e) {
            if (questions.SelectedItem is not Question question) {
                return;
            }
            hasIntro.IsChecked = question.GetIntroAudio(questionList.DataFolderPath);
        }

        void Questions_CellEditEnding(object _, DataGridCellEditEndingEventArgs e) => questionList.Unsaved = true;
        void QuestionImport(object _, RoutedEventArgs e) => questionList.Import(true);
        void QuestionImportLastSave(object _, RoutedEventArgs e) => questionList.ImportFrom(Settings.Default.lastQuestion);
        void QuestionMerge(object _, RoutedEventArgs e) => questionList.Import(false);
        void QuestionSave(object _, RoutedEventArgs e) => questionList.Save();
        void QuestionSaveAs(object _, RoutedEventArgs e) => questionList.SaveAs();
        void QuestionReleaseCheck(object _, RoutedEventArgs e) => questionList.ReleaseCheck();
        void QuestionEqualize(object _, RoutedEventArgs e) => questionList.Equalize();
        void QuestionAudio(object _, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Q);
        void QuestionIntroAudio(object _, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Intro);
        void RemoveIntroAudio(object _, RoutedEventArgs e) => RemoveQuestionAudio(AudioType.Intro);
        void QuestionRemove(object _, RoutedEventArgs e) => questionList.RemoveElement(questions);

        void MoveRight(object _, KeyEventArgs e) => WPFExtensions.MoveRight(e);
    }
}