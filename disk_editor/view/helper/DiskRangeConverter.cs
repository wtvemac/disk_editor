using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class DiskRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var disk = value as WebTVDisk;

            if (disk != null)
            {
                return string.Format("0x{0:X10}", 0)
                     + "-"
                     + string.Format("0x{0:X10}", disk.size_bytes);
            }
            else
            {
                return "Bad";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
