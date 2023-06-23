using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArkerRatWpfVersion
{
    /// <summary>
    /// Interaction logic for ArkerNotification.xaml
    /// </summary>
    public partial class ArkerNotification : UserControl
    {
        public ArkerNotification(string header,string message)
        {
            InitializeComponent();
            MessageFromArker.Content = message;
            Header.Content = header;

            //Auto close
            Task.Run(async () =>
            {
                await Task.Delay(5000);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CloseWindow(null, null);
                });
            });
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if(this.Parent is Panel parentContainer)
                parentContainer.Children.Remove(this);
            else if(this.Parent is Window window)
                window.Close();

        }
    }
}
