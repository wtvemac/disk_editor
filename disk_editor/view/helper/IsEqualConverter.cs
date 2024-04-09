using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class IsEqualConverter : IMultiValueConverter
    {
        public static readonly IsEqualConverter Instance = new IsEqualConverter();

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return false;
            }

            for (int i = 1; i < values.Length; i++)
            {
                if (!values[i].Equals(values[0]))
                {
                    return false;
                }
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
