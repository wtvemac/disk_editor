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
using System.Reflection;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Fluent.RibbonWindow
    {
        public MainViewModel main_view;
        public int fart_count = 0;
        public long last_fart_time = 0;

        public string ProductLabel
        {
            get
            {
                var title = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute;


                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    var version = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;

                    return string.Format(
                        "{4} v{0}.{1}.{2}.{3}",
                        version.Major,
                        version.Minor,
                        version.Build,
                        version.Revision,
                        title.Title
                    );
                }
                else
                {
                    var version = Assembly.GetExecutingAssembly().GetName().Version;

                    return string.Format(
                        "{4} v{0}.{1}.{2}.{3}",
                        version.Major,
                        version.Minor,
                        version.Build,
                        version.Revision,
                        title.Title
                    );
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(this.main_view != null)
            {
                if(!((bool)this.main_view.main_window.FarterAwarded))
                {
                    var current_time = DateTimeOffset.Now.ToUnixTimeSeconds();
                    if ((current_time - this.last_fart_time) >= 1)
                    {
                        this.last_fart_time = current_time;
                        this.fart_count++;
                        if (this.fart_count >= 10)
                        {
                            var player = new System.Media.SoundPlayer(disk_editor.Properties.Resources.winner);
                            player.PlaySync();

                            this.main_view.activate_the_farts();
                            return;
                        }
                    }
                }

                this.main_view.do_fart_it_out();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://emac.pm",
                UseShellExecute = true
            });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://unlicense.org/",
                UseShellExecute = true
            });
        }

        public AboutBox(MainViewModel main_view)
        {
            InitializeComponent();

            this.main_view = main_view;

            this.product_header.Text = ProductLabel;
        }
    }
}
