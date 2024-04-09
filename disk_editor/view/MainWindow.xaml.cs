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
using System.Threading;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        public bool? RunningInWine
        {
            get { return GetValue(RunningInWineProperty) as bool?; }
            set { SetValue(RunningInWineProperty, value); }
        }
        public static readonly DependencyProperty RunningInWineProperty =
            DependencyProperty.Register("RunningInWine",
                                        typeof(bool?),
                                        typeof(MainWindow));

        public bool? FarterAwarded
        {
            get { return GetValue(FarterAwardedProperty) as bool?; }
            set { SetValue(FarterAwardedProperty, value); }
        }
        public static readonly DependencyProperty FarterAwardedProperty =
            DependencyProperty.Register("FarterAwarded",
                                        typeof(bool?),
                                        typeof(MainWindow));

        public ICommand KeyInputCommand
        {
            get { return GetValue(KeyInputCommandProperty) as ICommand; }
            set { SetValue(KeyInputCommandProperty, value); }
        }
        public static readonly DependencyProperty KeyInputCommandProperty =
            DependencyProperty.Register("KeyInputCommand",
                                        typeof(ICommand),
                                        typeof(MainWindow));

        private void input_check(object sender, KeyEventArgs e)
        {
            this.KeyInputCommand.Execute(e);

            e.Handled = true;
        }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainViewModel(this, this.disk_view);
        }
    }
}
