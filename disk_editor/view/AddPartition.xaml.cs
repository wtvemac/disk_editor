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
using Xceed.Wpf.Toolkit;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for AddPartition.xaml
    /// </summary>
    public partial class AddPartition : Fluent.RibbonWindow
    {
        public partition_entry? AddedPartition
        {
            get { return GetValue(AddedPartitionProperty) as partition_entry?; }
            set { SetValue(AddedPartitionProperty, value); }
        }
        public static readonly DependencyProperty AddedPartitionProperty =
            DependencyProperty.Register("AddedPartition",
                                        typeof(partition_entry?),
                                        typeof(AddPartition));

        public PartitionType? SelectedPartitionType
        {
            get { return GetValue(SelectedPartitionTypeProperty) as PartitionType?; }
            set { SetValue(SelectedPartitionTypeProperty, value); }
        }
        public static readonly DependencyProperty SelectedPartitionTypeProperty =
            DependencyProperty.Register("SelectedPartitionType",
                                        typeof(PartitionType?),
                                        typeof(AddPartition));

        private void on_partition_type_change(object sender, SelectionChangedEventArgs e)
        {
            var dc = this.DataContext as AddPartitionViewModel;

            if(dc != null)
            {
                dc.set_partition_size_values();
            }
        }

        public AddPartition(WebTVDisk disk, WebTVPartition part)
        {
            InitializeComponent();

            this.DataContext = new AddPartitionViewModel(this, disk, part);
        }
    }
}
