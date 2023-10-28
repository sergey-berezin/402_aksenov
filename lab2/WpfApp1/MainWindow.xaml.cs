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
using NugetAnsNetw;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AnsNetwComp ansModel;
        CancellationTokenSource cts;
        string text;
        CancellationTokenSource ansCts;

        public MainWindow()
        {
            InitializeComponent();
            cts = new CancellationTokenSource();
            ansModel = new AnsNetwComp(cts.Token);
            ModelLoadAsync();
            text = null;
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
                string fileName = openFileDialog.FileName;
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
                var ans = await ansModel.AnsweringAsync(text, quest, ansCts.Token);

                chatTextBox.Text += $"Question: {quest}\n";
                chatTextBox.Text += $"Answer: {ans}\n";
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
    }
}
