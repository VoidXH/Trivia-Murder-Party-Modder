using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Windows.Controls;

namespace TriviaMurderPartyModder.Dialogs {
    public static class WPFExtensions {
        /// <summary>
        /// Enable the &quot;load last&quot; button if the last edited file is set and exists.
        /// </summary>
        /// <param name="button">Last file loader button</param>
        /// <param name="setting">The file path saved to the corresponding <see cref="Settings"/> value</param>
        public static void EnableIfHasSave(this Button button, string setting) {
            if (string.IsNullOrEmpty(setting) || !File.Exists(setting)) {
                button.IsEnabled = false;
            }
        }

        /// <summary>
        /// Move to the next UI element when enter is pressed. Best for linearly editing <see cref="DataGrid"/>s.
        /// </summary>
        public static void MoveRight(KeyEventArgs e) {
            if (e.Key == Key.Enter && e.OriginalSource is UIElement source) {
                e.Handled = true;
                source.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}