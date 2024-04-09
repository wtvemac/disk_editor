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
    /// Interaction logic for NVRAMView.xaml
    /// </summary>
    public partial class NVRAMView : Fluent.RibbonWindow
    {
        public ObjectLocationCollection NVRAMLocations
        {
            get { return GetValue(NVRAMLocationsProperty) as ObjectLocationCollection; }
            set { SetValue(NVRAMLocationsProperty, value); }
        }
        public static readonly DependencyProperty NVRAMLocationsProperty =
            DependencyProperty.Register("NVRAMLocations",
                                        typeof(ObjectLocationCollection),
                                        typeof(NVRAMView));

        public ObjectLocation SelectedObjectLocation
        {
            get { return GetValue(SelectedObjectLocationProperty) as ObjectLocation; }
            set { SetValue(SelectedObjectLocationProperty, value); }
        }
        public static readonly DependencyProperty SelectedObjectLocationProperty =
            DependencyProperty.Register("SelectedObjectLocation",
                                        typeof(ObjectLocation),
                                        typeof(NVRAMView));

        public ObjectLocationType? SelectedNVRAM
        {
            get { return GetValue(SelectedNVRAMProperty) as ObjectLocationType?; }
            set { SetValue(SelectedNVRAMProperty, value); }
        }
        public static readonly DependencyProperty SelectedNVRAMProperty =
            DependencyProperty.Register("SelectedNVRAM",
                                        typeof(ObjectLocationType?),
                                        typeof(NVRAMView));

        public bool? IsWriting
        {
            get { return GetValue(IsWritingProperty) as bool?; }
            set { SetValue(IsWritingProperty, value); }
        }
        public static readonly DependencyProperty IsWritingProperty =
            DependencyProperty.Register("IsWriting",
                                        typeof(bool?),
                                        typeof(NVRAMView));

        public bool? IsImported
        {
            get { return GetValue(IsImportedProperty) as bool?; }
            set { SetValue(IsImportedProperty, value); }
        }
        public static readonly DependencyProperty IsImportedProperty =
            DependencyProperty.Register("IsImported",
                                        typeof(bool?),
                                        typeof(NVRAMView));
        public bool? PendingImport
        {
            get { return GetValue(PendingImportProperty) as bool?; }
            set { SetValue(PendingImportProperty, value); }
        }
        public static readonly DependencyProperty PendingImportProperty =
            DependencyProperty.Register("PendingImport",
                                        typeof(bool?),
                                        typeof(NVRAMView));

        public bool? OnlyInfo
        {
            get { return GetValue(OnlyInfoProperty) as bool?; }
            set { SetValue(OnlyInfoProperty, value); }
        }
        public static readonly DependencyProperty OnlyInfoProperty =
            DependencyProperty.Register("OnlyInfo",
                                        typeof(bool?),
                                        typeof(NVRAMView));

        public bool? HasChanges
        {
            get { return GetValue(HasChangesProperty) as bool?; }
            set { SetValue(HasChangesProperty, value); }
        }
        public static readonly DependencyProperty HasChangesProperty =
            DependencyProperty.Register("HasChanges",
                                        typeof(bool?),
                                        typeof(NVRAMView));

        private void on_nvram_location_change(object sender, SelectionChangedEventArgs e)
        {
            var dc = this.DataContext as NVRAMViewModel;

            if (dc != null)
            {
                dc.set_current_nvram_info();
            }
        }

        public NVRAMView(WebTVDisk disk, bool? only_info)
        {
            InitializeComponent();

            this.OnlyInfo = only_info;

            this.DataContext = new NVRAMViewModel(this, disk, only_info);
        }
    }
}
