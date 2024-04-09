using System;
using System.Windows;
using System.Windows.Data;
using LTR.IO.ImDisk;

namespace disk_editor
{
    class PartitionMountPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var server = value as ImDiskNamedPipeServer;

            if (server != null)
            {
                var drive_letter = server.get_drive_letter();

                if (drive_letter != "")
                {
                    return drive_letter + ":";
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
