using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace disk_editor
{
    internal class Registry
    {
        public static string base_path = "HKEY_CURRENT_USER\\SOFTWARE\\webtv_disk_editor";

        public static object get(string location)
        {
            string[] parts = location.Split(new char[] { '\\', '/' });

            var path = "";
            var setting = parts[parts.Length - 1];

            if(parts.Length > 1)
            {
                path = "\\" + String.Join("\\", parts, 0, (parts.Length - 1));
            }

            try
            {
                return Microsoft.Win32.Registry.GetValue(Registry.base_path + path, setting, false);

            }
            catch (Exception)
            {
                return "";
            }
        }

        public static bool set(string location, object value)
        {
            string[] parts = location.Split(new char[] { '\\', '/' });

            var path = "";
            var setting = parts[parts.Length - 1];

            if (parts.Length > 1)
            {
                path = "\\" + String.Join("\\", parts, 0, (parts.Length - 1));
            }

            try
            {
                Microsoft.Win32.Registry.SetValue(Registry.base_path + path, setting, value);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
