using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;

namespace TriviaMurderPartyModder.Data {
    public abstract class DataFile<T> : ObservableCollection<T> {
        static readonly OpenFileDialog opener = new OpenFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };
        static readonly SaveFileDialog saver = new SaveFileDialog { Filter = "Trivia Murder Party database (*.jet)|*.jet" };

        readonly string items;

        public bool Unsaved { get; set; }
        public string FileName { get; private set; }

        public DataFile(string items) => this.items = items;

        protected abstract void Add(string fileName);
        protected abstract bool SaveAs(string name);

        public bool UnsavedPrompt() {
            if (Unsaved)
                return MessageBox.Show(string.Format("You have unsaved {0}. Do you want to discard them?", items), "Unsaved " + items,
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            return true;
        }

        public void Import(bool clear) {
            if (!clear || UnsavedPrompt()) {
                if (opener.ShowDialog() == true) {
                    if (clear)
                        Clear();
                    Add(FileName = opener.FileName);
                }
                Unsaved = !clear;
            }
        }

        public void ImportFrom(string path) {
            if (!UnsavedPrompt())
                return;
            Clear();
            Add(FileName = path);
            Unsaved = false;
        }

        public void Save() {
            if (FileName == null)
                SaveAs();
            else if (SaveAs(FileName))
                Unsaved = false;
        }

        public void SaveAs() {
            if (saver.ShowDialog() == true && SaveAs(saver.FileName)) {
                FileName = saver.FileName;
                Unsaved = false;
            }
        }

        protected override void InsertItem(int index, T item) {
            Unsaved = true;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index) {
            Unsaved = true;
            base.RemoveItem(index);
        }
    }
}