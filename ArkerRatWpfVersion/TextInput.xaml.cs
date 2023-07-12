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
    /// Interaction logic for TextInput.xaml
    /// </summary>
    public partial class TextInput : UserControl
    {
        public TextInput(string header)
        {
            Header.Content= header;
            InitializeComponent();
        }

        private void SendResultButton_Click(object sender, RoutedEventArgs e)
        {

        }
        public string text { get; set; }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            text = result.Text;

            if (this.Parent is Panel parentContainer)
                parentContainer.Children.Remove(this);
            else if (this.Parent is Window window)
                window.Close();

        }
    }
}
