using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArkerRatWpfVersion
{
    public static class GlobalVariables
    {
        public static readonly object _lock = new object();
        public static int byteSize = 1024;

    }

    public static class GlobalMethods
    {
        private static TaskCompletionSource<bool> windowClosedTcs;
        public static async Task ShowNotification(string header, string message)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                if (windowClosedTcs != null && !windowClosedTcs.Task.IsCompleted)
                {
                    await windowClosedTcs.Task;
                }

                await Task.Delay(300); 

                ArkerNotification toast = new ArkerNotification(header, message);
                Window notificationWindow = new Window()
                {
                    Width = 340,
                    Height = 100,
                    Background = System.Windows.Media.Brushes.Transparent,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    Topmost = true,
                    ShowInTaskbar = false,
                    Left = SystemParameters.PrimaryScreenWidth - 350,  
                    Top = SystemParameters.PrimaryScreenHeight - 120
                };

                notificationWindow.Content = toast;

                notificationWindow.Closed += (sender, args) =>
                {
                    if (windowClosedTcs != null && !windowClosedTcs.Task.IsCompleted)
                    {
                        windowClosedTcs.SetResult(true); // Notify that the window is closed
                    }
                };

                windowClosedTcs = new TaskCompletionSource<bool>();

                notificationWindow.Show();
            });
        }

    }


    public class DataItemClient
    {
        public string ClientName { get; set; }
        public string OS { get; set; }
        public string IPAddress { get; set; }
        public string MS { get; set; }

        public string Tag { get; set; }
        public ContextMenu ContextMenu { get; set; }
    }

    public class DataItemPort
    {
        public string Port { get; set; }
        public string Status { get; set; }

        public string Tag { get; set; }
        public ContextMenu ContextMenu { get; set; }
    }



}
