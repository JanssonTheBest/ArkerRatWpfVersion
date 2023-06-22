using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArkerRatWpfVersion
{
    public static class GlobalVariables
    {
        public static readonly object _lock = new object();
        public static int byteSize = 1024;

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
