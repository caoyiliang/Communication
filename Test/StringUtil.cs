using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public static class StringUtil
    {
        public static string BytesToString(byte[] data)
        {
            return BytesToString(data, data.Length);
        }

        public static string BytesToString(byte[] data, int count)
        {
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                string s = ByteToString(data[i]);
                sb.Append(s);
                if (i != count - 1) sb.Append(" ");
            }
            return sb.ToString();
        }

        public static string ByteToString(byte value)
        {
            return value.ToString("X2");
        }
    }
}
