using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class DiskTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var disk = value as WebTVDisk;

            if (disk != null)
            {
                switch (disk.type)
                {
                    case DiskType.IMAGE:
                        return "Image File";

                    case DiskType.PHYSICAL:
                        return "Physical Disk";

                    default:
                        return "Unknown";
                }
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
