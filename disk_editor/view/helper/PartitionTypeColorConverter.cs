using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace disk_editor
{
    class PartitionTypeColorConverter : IValueConverter
    {
        public SolidColorBrush get_type_color(WebTVPartition part, PartitionType type, PartitionType delegated_type)
        {
            switch (type)
            {
                case PartitionType.FREE:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FF00"));

                case PartitionType.ONE:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008000"));

                case PartitionType.FAT16:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008080"));

                case PartitionType.RAW:
                case PartitionType.RAW2:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000080"));

                case PartitionType.DVRFS: // also == PartitionType.DELEGATED
                    if (delegated_type != PartitionType.NONE && delegated_type != PartitionType.DELEGATED)
                    {
                        return this.get_type_color(part, delegated_type, PartitionType.NONE);
                    }
                    else
                    {
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#310080"));
                    }

                case PartitionType.UNALLOCATED:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));

                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var part = value as WebTVPartition;

            if (part != null)
            {
                return this.get_type_color(part, part.type, part.delegated_type);
            }
            else
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
            }
        }
   
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
