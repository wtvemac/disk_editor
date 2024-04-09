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
    public partial class BuildView : Fluent.RibbonWindow
    {
        public string NewBuildFilename
        {
            get { return GetValue(NewBuildFilenameProperty) as string; }
            set { SetValue(NewBuildFilenameProperty, value); }
        }
        public static readonly DependencyProperty NewBuildFilenameProperty =
            DependencyProperty.Register("NewBuildFilename",
                                        typeof(string),
                                        typeof(BuildView));

        public ObjectLocationCollection BuildLocations
        {
            get { return GetValue(BuildLocationsProperty) as ObjectLocationCollection; }
            set { SetValue(BuildLocationsProperty, value); }
        }
        public static readonly DependencyProperty BuildLocationsProperty =
            DependencyProperty.Register("BuildLocations",
                                        typeof(ObjectLocationCollection),
                                        typeof(BuildView));

        public ObjectLocation SelectedObjectLocation
        {
            get { return GetValue(SelectedObjectLocationProperty) as ObjectLocation; }
            set { SetValue(SelectedObjectLocationProperty, value); }
        }
        public static readonly DependencyProperty SelectedObjectLocationProperty =
            DependencyProperty.Register("SelectedObjectLocation",
                                        typeof(ObjectLocation),
                                        typeof(BuildView));

        public ObjectLocationType? SelectedBrowser
        {
            get { return GetValue(SelectedBrowserProperty) as ObjectLocationType?; }
            set { SetValue(SelectedBrowserProperty, value); }
        }
        public static readonly DependencyProperty SelectedBrowserProperty =
            DependencyProperty.Register("SelectedBrowser",
                                        typeof(ObjectLocationType?),
                                        typeof(BuildView));

        public long? CurrentLength
        {
            get { return GetValue(CurrentLengthProperty) as long?; }
            set { SetValue(CurrentLengthProperty, value); }
        }
        public static readonly DependencyProperty CurrentLengthProperty =
            DependencyProperty.Register("CurrentLength",
                                        typeof(long?),
                                        typeof(BuildView));
        public bool? IsWriting
        {
            get { return GetValue(IsWritingProperty) as bool?; }
            set { SetValue(IsWritingProperty, value); }
        }
        public static readonly DependencyProperty IsWritingProperty =
            DependencyProperty.Register("IsWriting",
                                        typeof(bool?),
                                        typeof(BuildView));

        public bool? OnlyInfo
        {
            get { return GetValue(OnlyInfoProperty) as bool?; }
            set { SetValue(OnlyInfoProperty, value); }
        }
        public static readonly DependencyProperty OnlyInfoProperty =
            DependencyProperty.Register("OnlyInfo",
                                        typeof(bool?),
                                        typeof(BuildView));

        private void on_build_location_change(object sender, SelectionChangedEventArgs e)
        {
            var dc = this.DataContext as BuildViewModel;

            if(dc != null)
            {
                dc.set_current_build_info();
            }
        }

        public BuildView(WebTVPartition part, bool? only_info)
        {
            InitializeComponent();

            this.OnlyInfo = only_info;

            this.DataContext = new BuildViewModel(this, part, only_info);
        }
    }
}
