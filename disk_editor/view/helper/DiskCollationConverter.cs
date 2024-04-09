using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class DiskCollationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var disk = value as WebTVDisk;

            if (disk != null)
            {
                var disk_collation = "";

                switch (disk.layout)
                {
                    case DiskLayout.UTV:
                        disk_collation += "UTV";
                        break;

                    case DiskLayout.WEBSTAR:
                        disk_collation += "Web*";
                        break;

                    case DiskLayout.LC2:
                        disk_collation += "LC2";
                        break;
                }

                switch (disk.byte_transform)
                {
                    case DiskByteTransform.BIT16SWAP:
                        disk_collation += " 16-bit Swapped"; // 16-bit little endian (32-bit middle endian)
                        break;

                    case DiskByteTransform.BIT32SWAP:
                        disk_collation += " 32-bit Swapped"; // 32-bit little endian
                        break;

                    case DiskByteTransform.BIT1632SWAP:
                        disk_collation += " 16+32-bit Swapped"; // 16-bit little endian then 32-bit little endian
                        break;

                    case DiskByteTransform.NOSWAP:
                        disk_collation += " No Swapping"; // 32-bit big endian
                        break;
                }

                if (disk_collation != "")
                {
                    return disk_collation;
                }
                else 
                {
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
