using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkerRatWpfVersion
{
    public static class GlobalVariables
    {
        public static readonly object _lock = new object();
        public static int byteSize = 4096;
    }
}
