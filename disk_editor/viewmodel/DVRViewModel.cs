using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace disk_editor
{
    class DVRViewModel : INotifyPropertyChanged
    {
        public DVRView dvr_dialog { get; set; }
        public WebTVPartition part { get; set; }

        private WaitMessage wait_window;
        private delegate void dvr_info_callback(DVRRecordings dvr_recordings, string error_message);

        private DVRRecordings dvr_recordings;

        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _loading;
        public bool loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                RaisePropertyChanged("loading");
            }
        }

        public void process_dvr_recordings(DVRFsd dvr)
        {
            this.loading = true;

            if (this.wait_window == null)
            {
                this.wait_window = new WaitMessage("Loading DVRFsd Recordings...", this.dvr_dialog);
            }

            //this.dvr_dialog.nvram_settings_list.ItemsSource = null;

            this.wait_window.Go(() =>
            {
                this.read_dvr_recordings(dvr);
            });
        }

        public void read_dvr_recordings(DVRFsd dvr)
        {
            if (this.dvr_dialog != null)
            {
                DVRRecordings dvr_recordings = null;
                uint nvram_checksum = 0;
                var error_message = "";

                try
                {
                    dvr_recordings = dvr.enumerate_dvr_recodings();
                }
                catch (Exception ex)
                {
                    error_message = ex.Message;
                }
                finally
                {
                    this.finalize_dvr_recodings_read(dvr_recordings, error_message);
                }
            }
        }

        public void finalize_dvr_recodings_read(DVRRecordings dvr_recordings, string error_message)
        {
            if (this.dvr_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new dvr_info_callback(finalize_dvr_recodings_read);

                this.dvr_dialog.Dispatcher.Invoke(cb, dvr_recordings, error_message);
            }
            else
            {
                this.close_wait_window();
                this.loading = false;

                if (error_message != "")
                {
                    System.Windows.MessageBox.Show(error_message);
                }
                else if (dvr_recordings != null)
                {
                    this.dvr_recordings = dvr_recordings;
                }

                this.draw_dvr_recordings();
            }
        }

        public void draw_dvr_recordings()
        {
            if (this.dvr_recordings != null)
            {
            }
        }

        public void close_wait_window()
        {
            if (this.wait_window != null)
            {
                if (this.wait_window.close_window())
                {
                    this.wait_window = null;
                }
            }
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            var dvr = new DVRFsd(this.part.disk.io, this.part.get_object_bounds());

            this.process_dvr_recordings(dvr);
        }

        public DVRViewModel(DVRView dvr_dialog, WebTVPartition part)
        {
            this.dvr_dialog = dvr_dialog;
            this.part = part;

            this.dvr_dialog.Loaded += window_loaded;
        }
    }
}
