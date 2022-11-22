using System.Windows;
using System.Windows.Controls;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder {
    public partial class MainWindow {
        void ImportQuestionAudio(AudioType type) {
            if (!(questions.SelectedItem is Question question)) {
                return;
            }
            question.ImportAudio(questionList.DataFolderPath, type, LoadAudio(questions, questionList));
            hasIntro.IsChecked = question.GetIntroAudio(questionList.DataFolderPath);
        }

        void RemoveQuestionAudio(AudioType type) {
            if (!(questions.SelectedItem is Question question)) {
                return;
            }
            question.RemoveAudio(questionList.DataFolderPath, type);
            hasIntro.IsChecked = question.GetIntroAudio(questionList.DataFolderPath);
        }

        void QuestionSelected(object _, SelectionChangedEventArgs e) {
            if (!(questions.SelectedItem is Question question)) {
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
        void QuestionReleaseCheck(object _, RoutedEventArgs e) => ReleaseCheck(questionList);
        void QuestionEqualize(object _, RoutedEventArgs e) => questionList.Equalize();
        void QuestionAudio(object _, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Q);
        void QuestionIntroAudio(object _, RoutedEventArgs e) => ImportQuestionAudio(AudioType.Intro);
        void RemoveIntroAudio(object _, RoutedEventArgs e) => RemoveQuestionAudio(AudioType.Intro);
        void QuestionRemove(object _, RoutedEventArgs e) => RemoveElement(questions, questionList);
    }
}