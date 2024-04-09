using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace disk_editor
{
    public class AddPartitionViewModel : INotifyPropertyChanged
    {
        private delegate void close_call(partition_entry? created_partition);
        public const uint SIZE_INCREMENT = 1024 * 1024;
        public WebTVPartition part { get; set; }
        public WebTVDisk disk { get; set; }
        public AddPartition add_partition_dialog { get; set; }
        public WaitMessage wait_window;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public string check
        {
            get
            {
                return "true";
            }
        }

        // Raise PropertyChanged event
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        private RelayCommand _add_partition_command;
        public ICommand add_partition_command
        {
            get
            {
                if (_add_partition_command == null)
                {
                    _add_partition_command = new RelayCommand(x => on_add_partition(), x => true);
                }

                return _add_partition_command;
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

        public void close_window(partition_entry? created_partition = null)
        {
            if (this.add_partition_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new close_call(close_window);

                this.add_partition_dialog.Dispatcher.Invoke(cb, created_partition);
            }
            else
            {
                this.close_wait_window();
                this.add_partition_dialog.AddedPartition = created_partition;
                this.add_partition_dialog.Close();
            }
        }

        public void on_add_partition()
        {
            var partition_manager = new WebTVPartitionManager(this.disk);

            var max_size = partition_manager.largest_available_create_size();
            var requested_size = this.add_partition_dialog.new_partition_size.Value * AddPartitionViewModel.SIZE_INCREMENT;
            var requested_name = this.add_partition_dialog.new_partition_name.Text;
            var requested_delegate = "";
            var requested_type_select = this.add_partition_dialog.new_partition_type.SelectedItem as GenericListItem;
            var requested_type = PartitionType.RAW;

            if (requested_type_select != null)
            {
                requested_type = (PartitionType)requested_type_select.Value;
            }

            if (requested_size > this.get_max_size(requested_type))
            {
                MessageBox.Show("Requested partition size is too large!");
            }
            else if (requested_size < this.get_min_size(requested_type))
            {
                MessageBox.Show("Requested partition size too small!");
            }
            else if (requested_name == "")
            {
                MessageBox.Show("You must enter a partition name!");
            }
            else
            {
                if (requested_type == PartitionType._DELEGATED)
                {
                    requested_type = PartitionType.DELEGATED;
                    requested_delegate = this.add_partition_dialog.new_delegate_filename.Text;
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    if (requested_type == PartitionType.FAT16)
                    {
                        requested_type = PartitionType.DELEGATED;
                        requested_delegate = "fatfs.dll";
                    }
                    else if (requested_type == PartitionType.DVRFS)
                    {
                        requested_type = PartitionType.DELEGATED;
                        requested_delegate = "DVRFsd.dll";
                    }
                }

                this.wait_window = new WaitMessage("Creating Partition...");
                this.wait_window.Owner = this.add_partition_dialog.Owner;
                this.wait_window.Go(() =>
                {
                    partition_entry? created_partition = null;

                    try
                    {
                        created_partition = partition_manager.add_partition(this.part, requested_name, (ulong)requested_size, requested_type, requested_delegate);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Error adding partition! " + e.Message);
                    }
                    finally
                    { 
                        this.close_window(created_partition);
                    }
                });
            }
        }

        public void on_cancel_click()
        {
            this.close_window();
        }

        public ulong reduce_to_increment(ulong size_bytes)
        {
            return (size_bytes / AddPartitionViewModel.SIZE_INCREMENT);
        }

        public ulong get_max_size(PartitionType type)
        {
            var partition_manager = new WebTVPartitionManager(this.disk);

            var max_available = partition_manager.largest_available_create_size();
            var max_allowed = partition_manager.get_maximum_size(type);

            return Math.Min(max_available, max_allowed);
        }

        public ulong get_min_size(PartitionType type, bool alert_user = false)
        {
            var partition_manager = new WebTVPartitionManager(this.disk);

            var max_available = partition_manager.largest_available_create_size();
            var min_allowed = partition_manager.get_minimum_size(type);

            if (alert_user && min_allowed > max_available)
            {
                MessageBox.Show("There's no available area to create this type of partition!");
            }

            return min_allowed;
        }

        public void set_partition_size_values(bool initial_values = false)
        {
            RaisePropertyChanged("");

            ulong min_size = 0;
            ulong max_size = 0;

            var requested_type_select = this.add_partition_dialog.new_partition_type.SelectedItem as GenericListItem;
            var requested_type = PartitionType.RAW;
            if (requested_type_select != null)
            {
                requested_type = (PartitionType)requested_type_select.Value;
            }

            this.add_partition_dialog.SelectedPartitionType = requested_type;

            min_size = this.get_min_size(requested_type, true);
            max_size = this.get_max_size(requested_type);

            var min_increment = this.reduce_to_increment(min_size);
            var max_increment = this.reduce_to_increment(max_size);
            var cur_increment = (ulong)this.add_partition_dialog.new_partition_size.Value;

            this.add_partition_dialog.new_partition_size.Minimum = min_increment;
            this.add_partition_dialog.new_partition_size.Maximum = max_increment;

            this.add_partition_dialog.new_partition_min_size.Content = BytesToString.bytes_to_iec(min_size);
            this.add_partition_dialog.new_partition_max_size.Content = BytesToString.bytes_to_iec(max_size);
            this.add_partition_dialog.new_partition_size_range.Content = BytesToString.bytes_to_iec(min_size) + " to " + BytesToString.bytes_to_iec(max_size);

            var requested_size = (ulong)this.add_partition_dialog.new_partition_size.Value;
            if (this.part != null
             && (this.part.type == PartitionType.FREE || this.part.type == PartitionType.UNALLOCATED)
             && initial_values)
            {
                requested_size = this.reduce_to_increment(this.part.sector_length * this.disk.sector_bytes_length);
            }
            this.add_partition_dialog.new_partition_size.Value = Math.Min(Math.Max(min_increment, requested_size), max_increment);
        }

        public GenericListItem new_partition_type(PartitionType type, string description)
        {
            var partition_type = new GenericListItem()
            {
                Value = type,
                Text = description
            };

            return partition_type;
        }

        public void set_partition_types()
        {
            this.add_partition_dialog.new_partition_type.Items.Add(new_partition_type(PartitionType.RAW, "BOOT/APPROM/Raw"));
            this.add_partition_dialog.new_partition_type.Items.Add(new_partition_type(PartitionType.FAT16, "FAT16"));
            this.add_partition_dialog.new_partition_type.Items.Add(new_partition_type(PartitionType.DVRFS, "DVRFS"));
            if(this.disk.layout == DiskLayout.UTV)
            {
                // "Delegated" is a name I gave it so it makes sense to me.
                // I have no idea what they called it at Microsoft.
                this.add_partition_dialog.new_partition_type.Items.Add(new_partition_type(PartitionType._DELEGATED, "Delegated (UTV)"));
            }
        }

        public AddPartitionViewModel(AddPartition add_partition_dialog, WebTVDisk disk, WebTVPartition part)
        {
            this.add_partition_dialog = add_partition_dialog;
            this.disk = disk;
            this.part = part;

            if (this.disk == null)
            {
                this.close_window();
            }
            else
            {
                set_partition_types();
                set_partition_size_values(true);
            }
        }
    }
}
