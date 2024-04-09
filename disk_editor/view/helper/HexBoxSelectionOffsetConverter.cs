using System;
using System.Windows;
using System.Windows.Data;
using Be.Windows.Forms;

namespace disk_editor
{
    class HexBoxSelectionOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var hb = parameter as HexBox;

            if (hb != null)
            {
                return (hb.LineInfoOffset + hb.SelectionStart).ToString("X");
            }
            else
            {
                return "00";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
