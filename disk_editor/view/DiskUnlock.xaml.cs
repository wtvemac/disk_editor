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
    /// Interaction logic for DiskUnlock.xaml
    /// </summary>
    public partial class DiskUnlock : Fluent.RibbonWindow
    {
        public DiskUnlock(DiskWMIEntry diskWMI, AddDiskViewModel add_disk_viewmodel, bool lock_mode = false)
        {
            InitializeComponent();

            this.DataContext = new DiskUnlockViewModel(this, diskWMI, add_disk_viewmodel, lock_mode);
        }
    }
}
