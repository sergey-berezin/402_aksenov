using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;
using NugetAnsNetw;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class DialogEntry
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public string FileText { get; set; }
    }
    public partial class MainWindow : Window
    {
        AnsNetwComp ansModel;
        CancellationTokenSource cts;
        string text;
        CancellationTokenSource ansCts;
        List<DialogEntry> dialogHistory;
        string historyFilePath = "dialog_history.json";
        string fileName;

        public MainWindow()
        {
            InitializeComponent();
            cts = new CancellationTokenSource();
            ansModel = new AnsNetwComp(cts.Token);
            ModelLoadAsync();
            text = null;
            LoadDialogHistory();
        }
        private async void ModelLoadAsync()
        {
            sendButton.IsEnabled = false;
            cancelButton.IsEnabled = false;
            try
            {
                await ansModel.MakeSession();
                chatTextBox.Text += "ansModel is loaded.\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errror: {ex.Message}");
            }
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*\";";

            if (openFileDialog.ShowDialog() == true)
            {
                fileName = openFileDialog.FileName;
                text = File.ReadAllText(fileName);

                chatTextBox.Text += "Text is loaded.\n";
                chatTextBox.Text += "--------------------------------------------------------------------------.\n";
                chatTextBox.Text += text + "\n";
                chatTextBox.Text += "--------------------------------------------------------------------------.\n";

                sendButton.IsEnabled = true;
            }
        }
        private async void sendButton_Click(object sender, RoutedEventArgs e)
        {
            cancelButton.IsEnabled = true;
            if (text == null)
            {
                chatTextBox.Text += $"You need to upload the text\n";
                cancelButton.IsEnabled = false;
                sendButton.IsEnabled = true;
                return;
            }

            string quest = questionTextBox.Text;
            ansCts = new CancellationTokenSource();

            try
            {
                var previousEntry = dialogHistory.FirstOrDefault(entry => entry.Question == quest && entry.FileText == fileName);
                if (previousEntry != null)
                {
                    chatTextBox.Text += $"Question: {quest}\n";
                    chatTextBox.Text += $"Answer (from history): {previousEntry.Answer}\n";
                }
                else
                {
                    var ans = await ansModel.AnsweringAsync(text, quest, ansCts.Token);
                    chatTextBox.Text += $"Question: {quest}\n";
                    chatTextBox.Text += $"Answer: {ans}\n";

                    dialogHistory.Add(new DialogEntry { Question = quest, Answer = ans, FileText = fileName });
                    SaveDialogHistory();
                }
            }
            catch (Exception ex)
            {
                if (ansCts.Token.IsCancellationRequested)
                {
                    chatTextBox.Text += "Session is cancelled.\n";
                    cancelButton.IsEnabled = false;
                    sendButton.IsEnabled = true;
                    return;
                }
                MessageBox.Show($"err: {ex.Message}");
            }
            cancelButton.IsEnabled = false;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            ansCts.Cancel();
            ansCts.Dispose();
        }
        private void LoadDialogHistory()
        {
            if (File.Exists(historyFilePath))
            {
                string json = File.ReadAllText(historyFilePath);
                dialogHistory = JsonConvert.DeserializeObject<List<DialogEntry>>(json);
                DisplayDialogHistory();
            }
            else
            {
                dialogHistory = new List<DialogEntry>();
            }
        }
        private void DisplayDialogHistory()
        {
            foreach (var entry in dialogHistory)
            {
                chatTextBox.Text += $"Question: {entry.Question}\n";
                chatTextBox.Text += $"Answer: {entry.Answer}\n";
                chatTextBox.Text += "-------------------------------------\n";
            }
        }
        private void SaveDialogHistory()
        {
            string json = JsonConvert.SerializeObject(dialogHistory, Formatting.Indented);
            File.WriteAllText(historyFilePath, json);
        }
        private void clearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            dialogHistory.Clear();
            SaveDialogHistory();
            chatTextBox.Text = "Dialog history is cleared.\n";
        }
    }
}
