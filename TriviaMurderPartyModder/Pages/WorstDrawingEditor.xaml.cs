using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder.Pages {
    /// <summary>
    /// Interaction logic for WorstDrawingEditor.xaml
    /// </summary>
    public partial class WorstDrawingEditor : UserControl {
        readonly WorstDrawings worstDrawingList = [];

        public WorstDrawingEditor() {
            InitializeComponent();
            worstDrawingLast.EnableIfHasSave(Settings.Default.lastWorstDrawing);
            worstDrawings.ItemsSource = worstDrawingList;
            worstDrawings.CellEditEnding += WorstDrawings_CellEditEnding;
        }

        public void ImportReference(string contentPath) => worstDrawingList.ImportReference(contentPath);

        public bool OnClose() {
            bool cancel = worstDrawingList.UnsavedPrompt();
            if (!cancel && !string.IsNullOrEmpty(worstDrawingList.FileName)) {
                Settings.Default.lastWorstDrawing = worstDrawingList.FileName;
            }
            return cancel;
        }

        void WorstDrawings_CellEditEnding(object _, DataGridCellEditEndingEventArgs e) => worstDrawingList.Unsaved = true;
        void WorstDrawingImport(object _, RoutedEventArgs e) => worstDrawingList.Import(true);
        void WorstDrawingImportLastSave(object _, RoutedEventArgs e) =>
            worstDrawingList.ImportFrom(Settings.Default.lastWorstDrawing);
        void WorstDrawingMerge(object _, RoutedEventArgs e) => worstDrawingList.Import(false);
        void WorstDrawingSave(object _, RoutedEventArgs e) => worstDrawingList.Save();
        void WorstDrawingSaveAs(object _, RoutedEventArgs e) => worstDrawingList.SaveAs();
        void WorstDrawingReleaseCheck(object _, RoutedEventArgs e) => worstDrawingList.ReleaseCheck();
        void WorstDrawingAudio(object _, RoutedEventArgs e) =>
            ((WorstDrawing)worstDrawings.SelectedItem).ImportAudio(worstDrawingList.DataFolderPath,
                AudioHandling.LoadAudio(worstDrawings, worstDrawingList));
        void WorstDrawingRemove(object _, RoutedEventArgs e) => worstDrawingList.RemoveElement(worstDrawings);

        void MoveRight(object _, KeyEventArgs e) => WPFExtensions.MoveRight(e);
    }
}