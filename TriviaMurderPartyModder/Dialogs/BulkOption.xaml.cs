using System;
using System.Windows;

namespace TriviaMurderPartyModder.Dialogs {
    /// <summary>
    /// Interaction logic for BulkOption.xaml
    /// </summary>
    public partial class BulkOption : Window {
        static readonly string[] splits = new[] { "\r\n", "\r", "\n" };
        public BulkOption() => InitializeComponent();

        public string[] CorrectValues => correct.Text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        public string[] IncorrectValues => incorrect.Text.Split(splits, StringSplitOptions.RemoveEmptyEntries);

        void Add(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        void Cancel(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}