using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class PartitionRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var part = value as WebTVPartition;

            if (part != null)
            {
                return string.Format("0x{0:X10}", (part.sector_start * part.disk.sector_bytes_length))
                     + "-"
                     + string.Format("0x{0:X10}", ((part.sector_start + part.sector_length) * part.disk.sector_bytes_length));
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
