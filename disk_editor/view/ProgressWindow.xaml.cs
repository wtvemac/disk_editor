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
using System.Windows.Threading;
using System.Threading;
using XamlAnimatedGif;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private delegate void progress_owner_call(float percent_value, string message = "");

        private long last_time_ms = 0;
        private long cumulative_time = 0;
        private long cululative_written_bytes = 0;
        private double bytes_per_min = 0.0;

        public void write_progress(long bytes_written, long bytes_written_since, long bytes_total)
        {
            float progress_percentage = ((float)bytes_written / (float)bytes_total) * 100;
            string progress_message = String.Format("{0:00.00}%", progress_percentage);

            long current_time_ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (this.last_time_ms > 0)
            {
                long time_difference_ms = Math.Max(current_time_ms - this.last_time_ms, 1);

                this.cumulative_time += time_difference_ms;
            }

            this.cululative_written_bytes += bytes_written_since;

            this.last_time_ms = current_time_ms;

            if (this.cumulative_time > 250)
            {
                this.bytes_per_min = ((double)this.cululative_written_bytes / (double)this.cumulative_time) * (1000.0 * 60.0);

                this.cumulative_time = 0;
                this.cululative_written_bytes = 0;
            }

            if (this.bytes_per_min > 0)
            {
                progress_message += String.Format(" [{0}/min]", BytesToString.bytes_to_iec((ulong)this.bytes_per_min));
            }

            this.set_progress(progress_percentage, progress_message);
        }

        public void set_progress(float percent_value, string message = "")
        {
            if (this.Dispatcher.CheckAccess() == false)
            {
                var cb = new progress_owner_call(set_progress);

                this.Dispatcher.BeginInvoke(cb, percent_value, message);
            }
            else
            {
                this.progress_bar.Value = percent_value;
                this.progress_message.Text = message;
            }
        }

        public void Go()
        {
            this.Show();
        }

        public ProgressWindow(RoutedEventHandler progress_window_loaded, RoutedEventHandler progress_window_unloaded, Window owning_window = null, int initial_percent_value = 0, string initial_message = "")
        {
            InitializeComponent();

            if (owning_window != null)
            {
                if (owning_window.IsVisible)
                {
                    this.Owner = owning_window;
                }
                else
                {
                    this.Owner = owning_window.Owner;
                }
            }

            this.Loaded += progress_window_loaded;
            this.Unloaded += progress_window_unloaded;

            this.set_progress(initial_percent_value, initial_message);
        }

        public void close_window()
        {
            this.Close();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.close_window();
        }
    }
}