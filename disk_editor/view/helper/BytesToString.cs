using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disk_editor
{
    static class BytesToString
    {
        static public String bytes_to_si(ulong bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (bytes == 0)
            {
                return "0 " + units[0];
            }
            else
            {
                int unit_index = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1000)));
                double resoled_bytes = Math.Round(bytes / Math.Pow(1000, unit_index), 1);
                return resoled_bytes.ToString() + " " + units[unit_index];
            }
        }

        static public String bytes_to_iec(ulong bytes)
        {
            string[] units = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB" };

            if (bytes == 0)
            {
                return "0 " + units[0];
            }
            else 
            {
                int unit_index = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double resoled_bytes = Math.Round(bytes / Math.Pow(1024, unit_index), 1);
                return resoled_bytes.ToString() + " " + units[unit_index];
            }
        }
    }
}
