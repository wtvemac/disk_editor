using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace disk_editor
{
    class DiskIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var images_path = "pack://application:,,,/disk_editor;component/view/static/images";

            var disk = value as WebTVDisk;

            if (disk != null && disk.state == DiskState.HEALTHY)
            {
                switch (disk.type)
                {
                    case DiskType.IMAGE:
                        return new BitmapImage(new Uri(images_path + "/disk-image.png", UriKind.Absolute));

                    case DiskType.PHYSICAL:
                        return new BitmapImage(new Uri(images_path + "/disk-physical.png", UriKind.Absolute));

                    default:
                        return new BitmapImage(new Uri(images_path + "/images/disk-unknown.png", UriKind.Absolute));
                }
            }
            else
            {
                return new BitmapImage(new Uri(images_path + "/disk-bad.png", UriKind.Absolute));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
