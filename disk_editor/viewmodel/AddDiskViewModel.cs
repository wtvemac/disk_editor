using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.ComponentModel;

namespace disk_editor
{
    public class AddDiskViewModel : INotifyPropertyChanged
    {
        private delegate void single_callback(DiskWMIEntry physical_disks);
        private delegate void enumerator_callback(DiskWMIEntries physical_disks);

        public AddDisk add_disk_dialog { get; set; }
        public WebTVDiskCollection webtv_disks { get; set; }
        public DiskWMIEntries physical_disks { get; set; }
        private WaitMessage wait_window;

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

        private DiskWMIEntry _selected_disk;
        public DiskWMIEntry selected_disk
        {
            get { return _selected_disk; }
            set
            {
                _selected_disk = value;
                RaisePropertyChanged("selected_disk");
            }
        }

        private RelayCommand _cancel_command;
        public ICommand cancel_command
        {
            get
            {
                if (_cancel_command == null)
                {
                    _cancel_command = new RelayCommand(x => on_cancel_click(), x => true);
                }

                return _cancel_command;
            }
        }

        private RelayCommand _insert_disk_manually;
        public ICommand insert_disk_manually
        {
            get
            {
                if (_insert_disk_manually == null)
                {
                    _insert_disk_manually = new RelayCommand(x => on_insert_disk_manually_click(), x => true);
                }

                return _insert_disk_manually;
            }
        }

        private RelayCommand _add_disk_command;
        public ICommand add_disk_command
        {
            get
            {
                if (_add_disk_command == null)
                {
                    _add_disk_command = new RelayCommand(x => on_add_disk_click(), x => true);
                }

                return _add_disk_command;
            }
        }

        private RelayCommand _lock_disk_command;
        public ICommand lock_disk_command
        {
            get
            {
                if (_lock_disk_command == null)
                {
                    _lock_disk_command = new RelayCommand(x => on_lock_disk_command(), x => true);
                }

                return _lock_disk_command;
            }
        }
        

        private RelayCommand _refresh_command;
        public ICommand refresh_command
        {
            get
            {
                if (_refresh_command == null)
                {
                    _refresh_command = new RelayCommand(x => on_refresh_click(), x => true);
                }

                return _refresh_command;
            }
        }

        public void close_window()
        {
            this.add_disk_dialog.Close();
        }

        public void on_cancel_click()
        {
            close_window();
        }

        public void on_insert_disk_manually_click()
        {
            var input_box = new InputBox("Enter the disk path:", "Disk Path", "/dev/sda");
            input_box.Owner = this.add_disk_dialog;
            var disk_path = input_box.ShowDialog();

            if(disk_path != null && disk_path != "")
            {
                this.loading = true;

                this.wait_window = new WaitMessage("Loading Disk...");
                if (this.add_disk_dialog.IsVisible)
                {
                    this.wait_window.Owner = this.add_disk_dialog;
                }
                else
                {
                    this.wait_window.Owner = this.add_disk_dialog.Owner;
                }

                this.add_disk_dialog.disk_list.ItemsSource = null;

                this.wait_window.Go(() =>
                {
                    try
                    {
                        this.add_physical_disk(disk_path);
                    }
                    catch (Exception ex)
                    {
                        this.set_disk(null);
                        MessageBox.Show("Error adding disk. " + ex.Message);
                    }
                }, false);
            }
        }

        public void on_add_disk_click()
        {
            if (this.selected_disk != null)
            {
                if(this.selected_disk.is_security_locked)
                {
                    this.unlock_disk(this.selected_disk);
                }
                else
                {
                    this.add_disk(this.selected_disk);
                }
            }
        }

        public void on_lock_disk_command()
        {
            if (this.selected_disk != null)
            {
                this.lock_disk(this.selected_disk);
            }
        }

            public void on_refresh_click()
        {
            if (!this.loading)
            {
                get_disks();            
            }
        }

        public void unlock_disk(DiskWMIEntry disk)
        {
            var unlock_disk_dialog = new DiskUnlock(disk, this);
            unlock_disk_dialog.Owner = this.add_disk_dialog;
            unlock_disk_dialog.ShowDialog();
        }

        public void lock_disk(DiskWMIEntry disk)
        {
            var lock_disk_dialog = new DiskUnlock(disk, this, true);
            lock_disk_dialog.Owner = this.add_disk_dialog;
            lock_disk_dialog.ShowDialog();
        }

        public void add_disk(DiskWMIEntry disk)
        {
            try
            {
                var existing_disk = this.webtv_disks.get_disk_by_path(disk.device_id);

                if (existing_disk != null)
                {
                    var owner = this.add_disk_dialog.Owner as MainWindow;

                    if (owner != null)
                    {
                        owner.disk_view.SelectedDisk = existing_disk;
                        owner.disk_view.SelectedPartition = null;
                    }
                }
                else
                {
                    var added_disk = this.webtv_disks.add_physical_disk(disk);

                    if (added_disk != null)
                    {
                        var owner = this.add_disk_dialog.Owner as MainWindow;

                        if (owner != null)
                        {
                            owner.disk_view.SelectedDisk = added_disk;
                            owner.disk_view.SelectedPartition = null;
                        }
                    }
                }

                this.close_window();
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't add disk! " + e.Message);
            }
        }

        public void get_disks()
        {
            this.loading = true;

            this.wait_window = new WaitMessage("Loading Disks...");
            if (this.add_disk_dialog.IsVisible)
            {
                this.wait_window.Owner = this.add_disk_dialog;
            }
            else
            {
                this.wait_window.Owner = this.add_disk_dialog.Owner;
            }

            this.add_disk_dialog.disk_list.ItemsSource = null;

            this.wait_window.Go(() =>
            {
                this.enumerate_disks();
            }, false);
        }

        public void enumerate_disks()
        {
            if(this.add_disk_dialog != null)
            {
                var disk_enumerator = new WebTVDiskEnumerator();

                this.set_disks(disk_enumerator.get_physical_disks(true));
            }
        }

        public void add_physical_disk(string disk_path)
        {
            if (this.add_disk_dialog != null)
            {
                var disk_enumerator = new WebTVDiskEnumerator();

                this.set_disk(disk_enumerator.get_physical_disk(disk_path, true));
            }
        }

        private void set_disk(DiskWMIEntry physical_disk)
        {
            if (this.add_disk_dialog.disk_list.Dispatcher.CheckAccess() == false)
            {
                var cb = new single_callback(set_disk);

                this.add_disk_dialog.Dispatcher.Invoke(cb, physical_disk);
            }
            else
            {
                if (physical_disk != null)
                {
                    if (this.physical_disks == null)
                    {
                        this.physical_disks = new DiskWMIEntries();
                    }

                    var is_duplicate = false;
                    for (var i = 0; i < this.physical_disks.Count; i++)
                    {
                        if (this.physical_disks[i].device_id == physical_disk.device_id)
                        {
                            is_duplicate = true;
                            break;
                        }
                    }

                    if(!is_duplicate)
                    {
                        this.physical_disks.Add(physical_disk);
                    }

                    this.add_disk_dialog.disk_list.ItemsSource = this.physical_disks;

                    RaisePropertyChanged("");
                }

                if (this.wait_window != null)
                {
                    this.wait_window.close_window();
                }

                this.loading = false;
            }
        }

        private void set_disks(DiskWMIEntries physical_disks)
        {
            if (this.add_disk_dialog.disk_list.Dispatcher.CheckAccess() == false)
            {
                var cb = new enumerator_callback(set_disks);

                this.add_disk_dialog.Dispatcher.Invoke(cb, physical_disks);
            }
            else
            {
                this.physical_disks = physical_disks;

                this.add_disk_dialog.disk_list.ItemsSource = this.physical_disks;
                if(this.add_disk_dialog.disk_list.Focusable)
                {
                    this.add_disk_dialog.disk_list.Focus();
                }
                if(this.physical_disks.Count > 0)
                {
                    this.add_disk_dialog.disk_list.SelectedIndex = 0;
                }

                if (this.wait_window != null)
                {
                    this.wait_window.close_window();
                }

                this.loading = false;
            }
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            if(!((bool)this.add_disk_dialog.RunningInWine))
            {
                this.get_disks();
            }
        }

        public void check_in_wine()
        {
            this.add_disk_dialog.RunningInWine = Wine.is_in_wine();
        }

        public AddDiskViewModel(AddDisk add_disk_dialog, WebTVDiskCollection webtv_disks)
        {
            this.add_disk_dialog = add_disk_dialog;
            this.webtv_disks = webtv_disks;
            this.add_disk_dialog.Loaded += window_loaded;

            this.check_in_wine();
        }
    }
}
