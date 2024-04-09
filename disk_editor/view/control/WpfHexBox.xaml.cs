using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Be.Windows.Forms
{
    public partial class WpfHexBox : UserControl
    {
        public WpfHexBox()
        {
            InitializeComponent();
        }

        public IByteProvider ByteProvider
        {
            get { return (IByteProvider)GetValue(ByteProviderProperty); }
            set { SetValue(ByteProviderProperty, value); }
        }
        public static readonly DependencyProperty ByteProviderProperty =
            DependencyProperty.Register("ByteProvider", 
                                    typeof(IByteProvider),
                                    typeof(WpfHexBox), 
                                    new UIPropertyMetadata(null, ByteProviderChanged)
            );

        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly",
                                    typeof(bool),
                                    typeof(WpfHexBox),
                                    new UIPropertyMetadata(true, ReadOnlyChanged)
            );

        public long LineInfoOffset
        {
            get { return (long)GetValue(LineInfoOffsetProperty); }
            set { SetValue(LineInfoOffsetProperty, value); }
        }
        public static readonly DependencyProperty LineInfoOffsetProperty =
            DependencyProperty.Register("LineInfoOffset",
                                    typeof(long),
                                    typeof(WpfHexBox),
                                    new UIPropertyMetadata((long)0, LineInfoOffsetChanged)
            );

        private static void ByteProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfHexBox)d).HexBox.ByteProvider = (IByteProvider)e.NewValue;
        }

        private static void ReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfHexBox)d).HexBox.ReadOnly = (bool)e.NewValue;
        }

        private static void LineInfoOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WpfHexBox)d).HexBox.LineInfoOffset = (long)e.NewValue;
        }
    }
}