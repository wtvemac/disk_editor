using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace disk_editor
{
    class DiskInitializeViewModel
    {
        private delegate void void_call();

        DiskInitialize initialize_disk_dialog { get; set; }
        WebTVDisk disk;
        WaitMessage wait_window;

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

        private RelayCommand _initilize_disk_command;
        public ICommand initilize_disk_command
        {
            get
            {
                if (_initilize_disk_command == null)
                {
                    _initilize_disk_command = new RelayCommand(x => on_initilize_disk_click(), x => true);
                }

                return _initilize_disk_command;
            }
        }

        public void close_window()
        {
            if (this.initialize_disk_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new void_call(this.close_window);

                this.initialize_disk_dialog.Dispatcher.Invoke(cb);
            }
            else
            {
                if(this.wait_window != null)
                {
                    this.wait_window.Close();
                }

                this.initialize_disk_dialog.Close();
            }
        }

        public void on_cancel_click()
        {
            close_window();
        }

        public void on_initilize_disk_click()
        {
            if (this.disk != null)
            {
                var partition_manager = new WebTVPartitionManager(disk);

                var requested_layout_select = this.initialize_disk_dialog.target_layout.SelectedItem as GenericListItem;
                var requested_layout = DiskLayout.LC2;
                if (requested_layout_select != null)
                {
                    requested_layout = (DiskLayout)requested_layout_select.Value;
                }

                this.wait_window = new WaitMessage("Initilizing Disk...");
                this.wait_window.Owner = this.initialize_disk_dialog.Owner;
                this.wait_window.Go(() =>
                {
                    try
                    {
                        partition_manager.initialize_disk(requested_layout);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error initilizing disk! " + e.Message);
                    }
                    finally
                    {
                        this.close_window();
                    }
                });

            }
        }

        public GenericListItem new_disk_layout(DiskLayout layout, string description)
        {
            var disk_layout = new GenericListItem()
            {
                Value = layout,
                Text = description
            };

            return disk_layout;
        }

        public void add_disk_layouts()
        {
            this.initialize_disk_dialog.target_layout.Items.Add(new_disk_layout(DiskLayout.LC2, "LC2/Derby Compatible"));
            this.initialize_disk_dialog.target_layout.Items.Add(new_disk_layout(DiskLayout.WEBSTAR, "Echostar"));
            this.initialize_disk_dialog.target_layout.Items.Add(new_disk_layout(DiskLayout.UTV, "UltimateTV"));
            // Removing to reduce user confusion. Really shouldn't need to use this option.
            //this.initialize_disk_dialog.target_layout.Items.Add(new_disk_layout(DiskLayout.PLAIN, "Plain"));
        }

        public DiskInitializeViewModel(DiskInitialize initialize_disk_dialog, WebTVDisk disk)
        {
            this.initialize_disk_dialog = initialize_disk_dialog;
            this.disk = disk;

            this.add_disk_layouts();
        }
    }
}
