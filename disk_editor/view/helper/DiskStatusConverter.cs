using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class DiskStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var disk = value as WebTVDisk;

            if (disk != null)
            {
                switch (disk.state)
                {
                    case DiskState.BROKEN:
                        return "Broken";

                    case DiskState.INVALID_PARTITION_MAP_CHECKSUM:
                        return "Table checksum failure";

                    case DiskState.NO_PARTITION_TABLE:
                        return "No partition table";

                    case DiskState.NO_PARTITIONS:
                        return "No partitions";

                    case DiskState.ODD_TRANSFORM:
                        return "Odd swapping";

                    case DiskState.HEALTHY:
                        return "Healthy";

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
