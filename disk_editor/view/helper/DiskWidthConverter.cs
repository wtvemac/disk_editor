using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;


namespace disk_editor
{
    class DiskWidthConverter : IMultiValueConverter
    {
        private double default_width = 300.0;

        public double adjust_container_width(double window_width)
        {
            return window_width - 150 - 3 - 2; // Disk cell width + left padding + partition box border
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length >= 3)
            {
                // Bypass this. Don't scale across drives, just scale partitions.
                return new GridLength(adjust_container_width((double)values[0]));
                //return new GridLength(935);

                /*
                var window_width = adjust_container_width((double)values[0]);
                if (window_width > 0)
                {
                    var parent_element = values[1] as ItemsControl;
                    if (parent_element != null)
                    {
                        var disk = values[2] as WebTVDisk;
                        
                        if (disk != null)
                        {
                            var disks = parent_element.ItemsSource as WebTVDiskCollection;

                            if (disks != null && disks.Count > 0)
                            {
                                ulong[] source_values = new ulong[disks.Count];
                                for(int i = 0; i < disks.Count; i++)
                                {
                                    source_values[i] = disks[i].size_bytes;
                                }

                                ulong max_value = source_values.Max();
                                Double geometric_mean = WidthScaling.get_geometric_mean(source_values);
                                Double scaled_size = WidthScaling.log_scale_single_ratio(disk.size_bytes, max_value, geometric_mean);

                                if (scaled_size > 0)
                                {
                                    var partition_width = window_width * scaled_size;

                                    return new GridLength(partition_width, GridUnitType.Pixel);
                                }
                                else
                                {
                                    return new GridLength(window_width, GridUnitType.Pixel);
                                }
                            }
                        }
                    }
                }*/
            }

            return new GridLength(default_width);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
