using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class PartitionDelegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var part = value as WebTVPartition;

            if (part != null)
            {
                if (part.delegate_filename != null && part.delegate_filename != "")
                {
                    return part.delegate_filename;
                }
                else
                {
                    return "-";
                }
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
