using System.Windows;
using System.Windows.Controls;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder {
    public partial class MainWindow {
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
    }
}