using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace disk_editor
{
    class WMIDiskIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var images_path = "pack://application:,,,/disk_editor;component/view/static/images";

            var disk = value as DiskWMIEntry;

            if (disk != null)
            {
                if (disk.is_webtv_disk)
                {
                    return new BitmapImage(new Uri(images_path + "/disk-webtv.png", UriKind.Absolute));
                }
                else
                {
                    return new BitmapImage(new Uri(images_path + "/disk-physical.png", UriKind.Absolute));
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
