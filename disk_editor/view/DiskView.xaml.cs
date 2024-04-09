using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for DiskView.xaml
    /// </summary>
    public partial class DiskView : UserControl
    {
        public WebTVDiskCollection ItemsSource
        {
            get { return GetValue(ItemsSourceProperty) as WebTVDiskCollection; }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource",
                                        typeof(WebTVDiskCollection),
                                        typeof(DiskView));

        public ICommand DoubleClickCommand
        {
            get { return GetValue(DoubleClickCommandProperty) as ICommand; }
            set { SetValue(DoubleClickCommandProperty, value); }
        }
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.Register("DoubleClickCommand",
                                        typeof(ICommand),
                                        typeof(DiskView));

        public ICommand SelectDiskCommand
        {
            get { return GetValue(SelectDiskCommandProperty) as ICommand; }
            set { SetValue(SelectDiskCommandProperty, value); }
        }
        public static readonly DependencyProperty SelectDiskCommandProperty =
            DependencyProperty.Register("SelectDiskCommand",
                                        typeof(ICommand),
                                        typeof(DiskView));

        public WebTVDisk SelectedDisk
        {
            get { return GetValue(SelectedDiskProperty) as WebTVDisk; }
            set { SetValue(SelectedDiskProperty, value); }
        }
        public static readonly DependencyProperty SelectedDiskProperty =
            DependencyProperty.Register("SelectedDisk",
                                        typeof(WebTVDisk),
                                        typeof(DiskView));

        public ICommand SelectPartitionCommand
        {
            get { return GetValue(SelectPartitionCommandProperty) as ICommand; }
            set { SetValue(SelectPartitionCommandProperty, value); }
        }
        public static readonly DependencyProperty SelectPartitionCommandProperty =
            DependencyProperty.Register("SelectPartitionCommand",
                                        typeof(ICommand),
                                        typeof(DiskView));

        public WebTVPartition SelectedPartition
        {
            get { return GetValue(SelectedPartitionProperty) as WebTVPartition; }
            set { 
                SetValue(SelectedPartitionProperty, value);
                SetValue(HasSelectedPartitionProperty, (value != null));
            }
        }
        public static readonly DependencyProperty SelectedPartitionProperty =
            DependencyProperty.Register("SelectedPartition",
                                        typeof(WebTVPartition),
                                        typeof(DiskView));

        public bool HasSelectedPartition
        {
            get { return (this.SelectedPartition != null); }
            set {  }
        }
        public static readonly DependencyProperty HasSelectedPartitionProperty =
            DependencyProperty.Register("HasSelectedPartition",
                                        typeof(bool),
                                        typeof(DiskView));

        public ICommand KeyInputCommand
        {
            get { return GetValue(KeyInputCommandProperty) as ICommand; }
            set { SetValue(KeyInputCommandProperty, value); }
        }
        public static readonly DependencyProperty KeyInputCommandProperty =
            DependencyProperty.Register("KeyInputCommand",
                                        typeof(ICommand),
                                        typeof(DiskView));

        private void DoubleClickEvent(object sender, MouseButtonEventArgs args)
        {
            this.DoubleClickCommand.Execute(null);
        }

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            var input = e as System.Windows.Input.KeyEventArgs;
            if (input != null)
            {
                if (input.Key == Key.Up || input.Key == Key.Left
                 || input.Key == Key.Down || input.Key == Key.Right || input.Key == Key.Tab)
                {
                    this.KeyInputCommand.Execute(e);
                    e.Handled = true;
                }
            }
        }

        public DiskView()
        {
            InitializeComponent();
        }
    }
}
