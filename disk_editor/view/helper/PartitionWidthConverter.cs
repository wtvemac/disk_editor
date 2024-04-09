using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace disk_editor
{
    class PartitionWidthConverter : IMultiValueConverter
    {
        private double default_width = 50.0;

        public Double adjust_container_width(Double window_width)
        {
            return window_width - 150 - 3 - 2; //  Disk cell width + left padding + partition box border
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length >= 3)
            {
                var container_width = adjust_container_width((double)values[0]);
                if (container_width > 0)
                {
                    var parent_element = values[1] as ItemsControl;
                    
                    if (parent_element != null)
                    {
                        var part = values[2] as WebTVPartition;

                        if (part != null)
                        {
                            var parts = parent_element.ItemsSource as WebTVPartitionCollection;

                            if (parts != null && parts.Count > 0)
                            {
                                ulong[] source_values = new ulong[parts.Count];
                                int selected_index = 0;
                                bool last_index = false;
                                for (int i = 0; i < parts.Count; i++)
                                {
                                    source_values[i] = parts[i].sector_length;

                                    if (part.id == parts[i].id)
                                    {
                                        selected_index = i;

                                        if(i == (parts.Count - 1))
                                        {
                                            last_index = true;
                                        }
                                    }
                                }

                                Double[] result_values = WidthScaling.log_scale_multi_ratio(source_values);
                                Double results_sum = result_values.Sum();
                                Double partition_width = 0;

                                if (last_index)
                                {
                                    Double previous_size = 0;
                                    for (int i = 0; i < (result_values.Length - 1); i++)
                                    {
                                        Double scaled_size = result_values[i] / results_sum;
                                        previous_size += Math.Round(container_width * scaled_size);
                                    }

                                    Double _scaled_size = result_values[selected_index] / results_sum;

                                    partition_width = container_width - previous_size - 12; // - 12 for outside and right border
                                }
                                else
                                {
                                    Double scaled_size = result_values[selected_index] / results_sum;
                                    partition_width = Math.Round(container_width * scaled_size) - 8; // - 8 for outside border
                                }

                                if (partition_width > 0)
                                {
                                    return new GridLength(partition_width, GridUnitType.Pixel);
                                }
                            }
                        }
                    }
                }
            }

            return new GridLength(default_width);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
