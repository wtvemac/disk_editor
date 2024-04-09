using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace disk_editor
{
    class PartitionIconConverter : IValueConverter
    {
        string images_path = "pack://application:,,,/disk_editor;component/view/static/images";

        public BitmapImage get_type_icon(WebTVPartition part, PartitionType type, PartitionType delegated_type)
        {
            switch (type)
            {
                case PartitionType.FREE:
                    return new BitmapImage(new Uri(this.images_path + "/partition-free.png", UriKind.Absolute));

                case PartitionType.ONE:
                    return new BitmapImage(new Uri(this.images_path + "/partition-one.png", UriKind.Absolute));

                case PartitionType.FAT16:
                    return new BitmapImage(new Uri(this.images_path + "/partition-fat.png", UriKind.Absolute));

                case PartitionType.RAW:
                case PartitionType.RAW2:
                    return new BitmapImage(new Uri(this.images_path + "/partition-boot.png", UriKind.Absolute));

                case PartitionType.DVRFS: // also == PartitionType.DELEGATED
                    if (delegated_type != PartitionType.NONE && delegated_type != PartitionType.DELEGATED)
                    {
                        return this.get_type_icon(part, delegated_type, PartitionType.NONE);
                    }
                    else
                    {
                        return new BitmapImage(new Uri(this.images_path + "/partition-dvrfs.png", UriKind.Absolute));
                    }

                case PartitionType.UNALLOCATED:
                    return new BitmapImage(new Uri(this.images_path + "/partition-unallocated.png", UriKind.Absolute));

                default:
                    return new BitmapImage(new Uri(this.images_path + "/partition-unknown.png", UriKind.Absolute));
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var part = value as WebTVPartition;

            if (part != null)
            {
                return this.get_type_icon(part, part.type, part.delegated_type);
            }

            return new BitmapImage(new Uri(this.images_path + "/partition-unknown.png", UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
