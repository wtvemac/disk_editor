using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    public partial class Wine
    {
        public static bool is_in_wine()
        {
            var hNTDLL = Wine.GetModuleHandle("ntdll.dll");

            if (hNTDLL == IntPtr.Zero)
            {
                // Running a non-NT OS. This isn't possible?
                return false;
            }

            var wine_address = Wine.GetProcAddress(hNTDLL, "wine_get_version");

            return (wine_address != IntPtr.Zero);
        }
    }
}
