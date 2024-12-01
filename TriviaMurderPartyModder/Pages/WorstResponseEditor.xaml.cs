using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder.Pages {
    /// <summary>
    /// Interaction logic for WorstResponseEditor.xaml
    /// </summary>
    public partial class WorstResponseEditor : UserControl {
        readonly WorstResponses worstResponseList = [];

        public WorstResponseEditor() {
            InitializeComponent();
            worstResponseLast.EnableIfHasSave(Settings.Default.lastWorstResponse);
            worstResponses.ItemsSource = worstResponseList;
            worstResponses.CellEditEnding += WorstResponses_CellEditEnding;
        }

        public void ImportReference(string contentPath) => worstResponseList.ImportReference(contentPath);

        public bool OnClose() {
            bool cancel = worstResponseList.UnsavedPrompt();
            if (!cancel && !string.IsNullOrEmpty(worstResponseList.FileName)) {
                Settings.Default.lastWorstResponse = worstResponseList.FileName;
            }
            return cancel;
        }

        void WorstResponses_CellEditEnding(object _, DataGridCellEditEndingEventArgs e) => worstResponseList.Unsaved = true;
        void WorstResponseImport(object _, RoutedEventArgs e) => worstResponseList.Import(true);
        void WorstResponseImportLastSave(object _, RoutedEventArgs e) =>
            worstResponseList.ImportFrom(Settings.Default.lastWorstResponse);
        void WorstResponseMerge(object _, RoutedEventArgs e) => worstResponseList.Import(false);
        void WorstResponseSave(object _, RoutedEventArgs e) => worstResponseList.Save();
        void WorstResponseSaveAs(object _, RoutedEventArgs e) => worstResponseList.SaveAs();
        void WorstResponseReleaseCheck(object _, RoutedEventArgs e) => worstResponseList.ReleaseCheck();
        void WorstResponseAudio(object _, RoutedEventArgs e) =>
            ((WorstResponse)worstResponses.SelectedItem).ImportAudio(worstResponseList.DataFolderPath,
                AudioHandling.LoadAudio(worstResponses, worstResponseList));
        void WorstResponseRemove(object _, RoutedEventArgs e) => worstResponseList.RemoveElement(worstResponses);

        void MoveRight(object _, KeyEventArgs e) => WPFExtensions.MoveRight(e);
    }
}