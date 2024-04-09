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

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for TellyScriptEditor.xaml
    /// </summary>
    public partial class TellyScriptEditor : Fluent.RibbonWindow
    {
        public byte[] ChangedBytes
        {
            get { return GetValue(ChangedBytesProperty) as byte[]; }
            set { SetValue(ChangedBytesProperty, value); }
        }
        public static readonly DependencyProperty ChangedBytesProperty =
            DependencyProperty.Register("ChangedBytes",
                                        typeof(byte[]),
                                        typeof(TellyScriptEditor));

        public TellyScriptEditor(byte[] tellyscript_data, TellyScriptType tellyscript_type = TellyScriptType.ORIGINAL)
        {
            InitializeComponent();

            this.DataContext = new TellyScriptEditorViewModel(this, tellyscript_data, tellyscript_type);
        }
    }
}
