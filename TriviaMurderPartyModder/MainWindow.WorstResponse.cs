﻿using System.Windows;
using System.Windows.Controls;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder {
    public partial class MainWindow {
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
    }
}