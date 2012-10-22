using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAP_Serial.Utilities
{
    internal class StringEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Compare(x, y, true) == 0;
        }

        public int GetHashCode(string obj)
        {
            return base.GetHashCode();
        }
    }
}
