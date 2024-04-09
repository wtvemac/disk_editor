using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class PartitionTypeConverter : IValueConverter
    {
        public string get_type_text(WebTVPartition part, PartitionType type, PartitionType delegated_type)
        {
            switch (type)
            {
                case PartitionType.FREE:
                    return "FREE";

                case PartitionType.ONE:
                    return "OOO";

                case PartitionType.FAT16:
                    return "FAT16";

                case PartitionType.DVRFS: // also == PartitionType.DELEGATED
                    if (delegated_type != PartitionType.NONE && delegated_type != PartitionType.DELEGATED)
                    {
                        return this.get_type_text(part, delegated_type, PartitionType.NONE);
                    }
                    else
                    {
                        return "DVRFS";
                    }

                case PartitionType.RAW:
                case PartitionType.RAW2:
                    if (part.sector_start == 0)
                    {
                        return "BOOT";
                    }
                    else
                    {
                        return "APPROM";
                    }

                case PartitionType.UNALLOCATED:
                    return "UNALLOCATED";

                default:
                    return "UNKNOWN [" + (type).ToString("X") + "]";
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var part = value as WebTVPartition;

            if (part != null)
            {
                return this.get_type_text(part, part.type, part.delegated_type);
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
