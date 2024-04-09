using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class PartitionStatusFlagsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var part = value as WebTVPartition;

            if (part != null)
            {
                var status_flags_line = "";

                switch (part.state)
                {
                    case PartitionState.BROKEN:
                        status_flags_line += "Broken";
                        break;

                    case PartitionState.HEALTHY:
                        status_flags_line += "Healthy";
                        break;

                    case PartitionState.OVERLAP_PREVIOUS:
                        status_flags_line += "Overlaps previous";
                        break;

                    case PartitionState.OVERLAP_NEXT:
                        status_flags_line += "Overlaps next";
                        break;

                    case PartitionState.BAD_SIZE:
                        status_flags_line += "Bad size";
                        break;

                    case PartitionState.SIZE_BEYOND_DISK_BOUND:
                        status_flags_line += "Size beyond disk";
                        break;

                    case PartitionState.START_BEYOND_DISK_BOUND:
                        status_flags_line += "Start beyond disk";
                        break;

                    default:
                        status_flags_line += "Unknown";
                        break;
                }

                return status_flags_line;
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
