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
using System.Windows.Shapes;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for BuildView.xaml
    /// </summary>
    public partial class DVRView : Fluent.RibbonWindow
    {
        public DVRView(WebTVPartition part)
        {
            InitializeComponent();

            this.DataContext = new DVRViewModel(this, part);
        }
    }
}
