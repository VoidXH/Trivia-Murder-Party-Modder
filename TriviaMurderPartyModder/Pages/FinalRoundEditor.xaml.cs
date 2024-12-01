using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using TriviaMurderPartyModder.Data;
using TriviaMurderPartyModder.Dialogs;
using TriviaMurderPartyModder.Files;
using TriviaMurderPartyModder.Properties;

namespace TriviaMurderPartyModder.Pages {
    /// <summary>
    /// Interaction logic for FinalRoundEditor.xaml
    /// </summary>
    public partial class FinalRoundEditor : UserControl {
        readonly FinalRounders finalRoundList = [];

        FinalRounder selectedTopic;
        FinalRounderChoice selectedChoice;

        public FinalRoundEditor() {
            InitializeComponent();
            finalRoundLast.EnableIfHasSave(Settings.Default.lastFinalRound);
            finalRounders.ItemsSource = finalRoundList;
        }

        public void ImportReference(string contentPath) => finalRoundList.ImportReference(contentPath);

        public bool OnClose() {
            bool cancel = finalRoundList.UnsavedPrompt();
            if (!cancel && !string.IsNullOrEmpty(finalRoundList.FileName)) {
                Settings.Default.lastFinalRound = finalRoundList.FileName;
            }
            return cancel;
        }

        void SelectFinalQuestion(FinalRounder question) {
            selectedTopic = question;
            topicId.Text = question.ID.ToString();
            topic.Text = question.Text;
        }

        void DeselectChoice() {
            selectedChoice = null;
            choiceCorrect.IsChecked = false;
            choiceAnswer.Text = string.Empty;
        }

        void FinalRoundSelection(object sender, RoutedPropertyChangedEventArgs<object> e) {
            TreeViewItem selected = (TreeViewItem)((TreeView)sender).SelectedItem;
            if (selected is FinalRounder topic) {
                DeselectChoice();
                SelectFinalQuestion(topic);
            } else if (selected != null) {
                selectedChoice = (FinalRounderChoice)selected;
                choiceCorrect.IsChecked = selectedChoice.Correct;
                choiceAnswer.Text = selectedChoice.Text;
                SelectFinalQuestion((FinalRounder)selected.Parent);
            }
        }

        void AddTopic(object _, RoutedEventArgs e) {
            FinalRounder newTopic = new(0, "New topic");
            finalRoundList.Add(newTopic);
            newTopic.IsSelected = true;
            topic.SelectAll();
            topic.Focus();
        }

        void AddTopicChoice(object _, RoutedEventArgs e) {
            if (selectedTopic != null) {
                FinalRounderChoice choice = new(false, "New choice");
                selectedTopic.Items.Add(choice);
                selectedTopic.IsExpanded = true;
                choice.IsSelected = true;
                choiceAnswer.SelectAll();
                choiceAnswer.Focus();
                finalRoundList.Unsaved = true;
            }
        }

        void AddTopicChoices(object _, RoutedEventArgs e) {
            if (selectedTopic != null) {
                BulkOption form = new();
                bool? result = form.ShowDialog();
                if (result.HasValue && result.Value) {
                    selectedTopic.IsExpanded = true;
                    finalRoundList.Unsaved = true;
                    string[] correct = form.CorrectValues, incorrect = form.IncorrectValues;
                    for (int i = 0; i < correct.Length; ++i) {
                        FinalRounderChoice choice = new(true, correct[i]);
                        selectedTopic.Items.Add(choice);
                    }
                    for (int i = 0; i < incorrect.Length; ++i) {
                        FinalRounderChoice choice = new(false, incorrect[i]);
                        selectedTopic.Items.Add(choice);
                    }
                }
            }
        }

        void AddTopicAudio(object _, RoutedEventArgs e) {
            string file = AudioHandling.LoadAudio(finalRounders, finalRoundList);
            if (file != null) {
                selectedTopic.ImportTopicAudio(finalRoundList.DataFolderPath, file);
            }
        }

        void TopicIDChange(object sender, TextChangedEventArgs e) {
            TextBox box = (TextBox)sender;
            if (!int.TryParse(box.Text, out int id)) {
                box.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                return;
            }
            box.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            if (selectedTopic != null) {
                selectedTopic.ID = id;
                finalRoundList.Unsaved = true;
            }
        }

        void TopicChange(object sender, TextChangedEventArgs e) {
            if (selectedTopic != null) {
                selectedTopic.Text = ((TextBox)sender).Text;
                finalRoundList.Unsaved = true;
            }
        }

        void RemoveTopic(object _, RoutedEventArgs e) {
            if (selectedTopic != null) {
                DeselectChoice();
                finalRoundList.Remove(selectedTopic);
            }
        }

        void ChoiceCorrect(object sender, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedChoice.Correct = ((CheckBox)sender).IsChecked.Value;
                finalRoundList.Unsaved = true;
            }
        }

        void ChoiceText(object sender, TextChangedEventArgs e) {
            if (selectedChoice != null) {
                selectedChoice.Text = ((TextBox)sender).Text;
                finalRoundList.Unsaved = true;
            }
        }

        void RemoveChoice(object _, RoutedEventArgs e) {
            if (selectedChoice != null) {
                selectedTopic.Items.Remove(selectedChoice);
                DeselectChoice();
                finalRoundList.Unsaved = true;
            }
        }

        void FinalRoundImport(object _, RoutedEventArgs e) => finalRoundList.Import(true);
        void FinalRoundImportLastSave(object _, RoutedEventArgs e) =>
            finalRoundList.ImportFrom(Settings.Default.lastFinalRound);
        void FinalRoundMerge(object _, RoutedEventArgs e) => finalRoundList.Import(false);
        void FinalRoundSave(object _, RoutedEventArgs e) => finalRoundList.Save();
        void FinalRoundSaveAs(object _, RoutedEventArgs e) => finalRoundList.SaveAs();

        void FinalRoundReleaseCheck(object _, RoutedEventArgs e) {
            string finalRoundFileDir = null;
            if (finalRoundList.FileName != null)
                finalRoundFileDir = finalRoundList.DataFolderPath;
            for (int i = 0, end = finalRoundList.Count; i < end; ++i) {
                for (int j = i + 1; j < end; ++j) {
                    if (finalRoundList[i].ID == finalRoundList[j].ID) {
                        FinalRounders.FinalRoundIssue(string.Format(Properties.Resources.multipleIDs, finalRoundList[i].ID));
                        return;
                    }
                }
                if (finalRoundList[i].Items.Count < 3) {
                    FinalRounders.FinalRoundIssue(string.Format("{0} has less than 3 choices.", finalRoundList[i].Text));
                    return;
                }
                if (finalRoundList.FileName != null && !Parsing.CheckAudio(finalRoundFileDir, finalRoundList[i].ID)) {
                    FinalRounders.FinalRoundIssue(string.Format(Properties.Resources.missingAudio, finalRoundList[i].ID));
                    return;
                }
            }
            MessageBox.Show(Properties.Resources.checkSuccess, Properties.Resources.checkResult);
        }
    }
}
