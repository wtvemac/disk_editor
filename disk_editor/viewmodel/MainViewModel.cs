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
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Aga.Controls.Tree;
using System.IO;
using LTR.IO.ImDisk;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Resources.ResXFileRef;

namespace disk_editor
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private delegate void void_callback();
        private delegate void disk_progress_callback(WebTVDisk disk);

        public DiskView disk_view { get; set; }
        public WebTVDiskCollection webtv_disks { get; set; }
        public MainWindow main_window { get; set; }

        private ProgressWindow progress_window;
        private Thread progress_thread;
        public string image_filename;
        public WebTVDiskIO io_source;
        public DiskCageBounds io_bounds;

        private bool entered_secret_code_mode = false;
        private string current_secret_code = "";
        private Dictionary<string, Action> secret_commands;

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

        #region Ribbon button ICommand properties
        private RelayCommand _add_disk_image;
        public ICommand add_disk_image
        {
            get
            {
                if (_add_disk_image == null)
                {
                    _add_disk_image = new RelayCommand(x => on_add_disk_image(), x => true);
                }

                return _add_disk_image;
            }
        }

        private RelayCommand _add_physical_disk;
        public ICommand add_physical_disk
        {
            get
            {
                if (_add_physical_disk == null)
                {
                    _add_physical_disk = new RelayCommand(x => on_add_physical_disk(), x => true);
                }

                return _add_physical_disk;
            }
        }

        private RelayCommand _remove_disk;
        public ICommand remove_disk
        {
            get
            {
                if (_remove_disk == null)
                {
                    _remove_disk = new RelayCommand(x => on_remove_disk(), x => true);
                }

                return _remove_disk;
            }
        }

        private RelayCommand _initialize_disk;
        public ICommand initialize_disk
        {
            get
            {
                if (_initialize_disk == null)
                {
                    _initialize_disk = new RelayCommand(x => on_initialize_disk(), x => true);
                }

                return _initialize_disk;
            }
        }

        private RelayCommand _add_partition;
        public ICommand add_partition
        {
            get
            {
                if (_add_partition == null)
                {
                    _add_partition = new RelayCommand(x => on_add_partition(), x => true);
                }

                return _add_partition;
            }
        }

        private RelayCommand _consolidate_free;
        public ICommand consolidate_free
        {
            get
            {
                if (_consolidate_free == null)
                {
                    _consolidate_free = new RelayCommand(x => on_consolidate_free(), x => true);
                }

                return _consolidate_free;
            }
        }

        private RelayCommand _delete_partition;
        public ICommand delete_partition
        {
            get
            {
                if (_delete_partition == null)
                {
                    _delete_partition = new RelayCommand(x => on_delete_partition(), x => true);
                }

                return _delete_partition;
            }
        }

        private RelayCommand _inspect_webtv_build;
        public ICommand inspect_webtv_build
        {
            get
            {
                if (_inspect_webtv_build == null)
                {
                    _inspect_webtv_build = new RelayCommand(x => on_inspect_webtv_build(), x => true);
                }

                return _inspect_webtv_build;
            }
        }

        private RelayCommand _build_info;
        public ICommand build_info
        {
            get
            {
                if (_build_info == null)
                {
                    _build_info = new RelayCommand(x => on_build_info(), x => true);
                }

                return _build_info;
            }
        }

        private RelayCommand _inspect_dvr_recordings;
        public ICommand inspect_dvr_recordings
        {
            get
            {
                if (_inspect_dvr_recordings == null)
                {
                    _inspect_dvr_recordings = new RelayCommand(x => on_inspect_dvr_recordings(), x => true);
                }

                return _inspect_dvr_recordings;
            }
        }
        private RelayCommand _edit_nvram;
        public ICommand edit_nvram
        {
            get
            {
                if (_edit_nvram == null)
                {
                    _edit_nvram = new RelayCommand(x => on_edit_nvram(), x => true);
                }

                return _edit_nvram;
            }
        }

        private RelayCommand _show_about_box;
        public ICommand show_about_box
        {
            get
            {
                if (_show_about_box == null)
                {
                    _show_about_box = new RelayCommand(x => on_show_about_box(), x => true);
                }

                return _show_about_box;
            }
        }

        private RelayCommand _fart_it_out;
        public ICommand fart_it_out
        {
            get
            {
                if (_fart_it_out == null)
                {
                    _fart_it_out = new RelayCommand(x => do_fart_it_out(), x => true);
                }

                return _fart_it_out;
            }
        }

        private RelayCommand _mount_partition;
        public ICommand mount_partition
        {
            get
            {
                if (_mount_partition == null)
                {
                    _mount_partition = new RelayCommand(x => on_mount_partition(), x => true);
                }

                return _mount_partition;
            }
        }
        
        
        private RelayCommand _unmount_partition;
        public ICommand unmount_partition
        {
            get
            {
                if (_unmount_partition == null)
                {
                    _unmount_partition = new RelayCommand(x => on_unmount_partition(), x => true);
                }

                return _unmount_partition;
            }
        }


        private RelayCommand _export_disk;
        public ICommand export_disk
        {
            get
            {
                if (_export_disk == null)
                {
                    _export_disk = new RelayCommand(x => on_export_disks(), x => true);
                }

                return _export_disk;
            }
        }
        private RelayCommand _export_used_disk;
        public ICommand export_used_disk
        {
            get
            {
                if (_export_used_disk == null)
                {
                    _export_used_disk = new RelayCommand(x => on_export_used_disk(), x => true);
                }

                return _export_used_disk;
            }
        }
        private RelayCommand _import_disk;
        public ICommand import_disk
        {
            get
            {
                if (_import_disk == null)
                {
                    _import_disk = new RelayCommand(x => on_import_disk(), x => true);
                }

                return _import_disk;
            }
        }
        private RelayCommand _export_partition_table;
        public ICommand export_partition_table
        {
            get
            {
                if (_export_partition_table == null)
                {
                    _export_partition_table = new RelayCommand(x => on_export_partition_table(), x => true);
                }

                return _export_partition_table;
            }
        }
        private RelayCommand _import_partition_table;
        public ICommand import_partition_table
        {
            get
            {
                if (_import_partition_table == null)
                {
                    _import_partition_table = new RelayCommand(x => on_import_partition_table(), x => true);
                }

                return _import_partition_table;
            }
        }
        private RelayCommand _export_partition;
        public ICommand export_partition
        {
            get
            {
                if (_export_partition == null)
                {
                    _export_partition = new RelayCommand(x => on_export_partition(), x => true);
                }

                return _export_partition;
            }
        }
        private RelayCommand _import_partition;
        public ICommand import_partition
        {
            get
            {
                if (_import_partition == null)
                {
                    _import_partition = new RelayCommand(x => on_import_partition(), x => true);
                }

                return _import_partition;
            }
        }

        private RelayCommand _explore_partition;
        public ICommand explore_partition
        {
            get
            {
                if (_explore_partition == null)
                {
                    _explore_partition = new RelayCommand(x => on_explore_partition(), x => true);
                }

                return _explore_partition;
            }
        }

        private RelayCommand _copy_partition_data;
        public ICommand copy_partition_data
        {
            get
            {
                if (_copy_partition_data == null)
                {
                    _copy_partition_data = new RelayCommand(x => on_copy_partition_data((string)x), x => true);
                }

                return _copy_partition_data;
            }
        }

        private RelayCommand _xxx;
        public ICommand xxx
        {
            get
            {
                if (_xxx == null)
                {
                    _xxx = new RelayCommand(x => on_xxx(), x => true);
                }

                return _xxx;
            }
        }
        #endregion

        private RelayCommand _select_partition_command;
        public ICommand select_partition_command
        {
            get
            {
                if (_select_partition_command == null)
                {
                    _select_partition_command = new RelayCommand(x => select_partition(x), x => true);
                }

                return _select_partition_command;
            }
        }

        private RelayCommand _select_disk_command;
        public ICommand select_disk_command
        {
            get
            {
                if (_select_disk_command == null)
                {
                    _select_disk_command = new RelayCommand(x => select_disk(x), x => true);
                }

                return _select_disk_command;
            }
        }
        
        private RelayCommand _key_input_action;
        public ICommand key_input_action
        {
            get
            {
                if (_key_input_action == null)
                {
                    _key_input_action = new RelayCommand(x => on_key_input_action(x), x => true);
                }

                return _key_input_action;
            }
        }

        private RelayCommand _dblclk_disk_action_ic;
        public ICommand dblclk_disk_action_ic
        {
            get
            {
                if (_dblclk_disk_action_ic == null)
                {
                    _dblclk_disk_action_ic = new RelayCommand(x => dblclk_disk_action(x), x => true);
                }

                return _dblclk_disk_action_ic;
            }
        }

        void clear_selection()
        {
            this.disk_view.SelectedDisk = null;
            this.disk_view.SelectedPartition = null;

            var tree = this.disk_view.disk_collection_tree;

            if (tree != null)
            {
                tree.SelectedItem = null;
            }
        }

        void select_partition(object selected_object)
        {
            var part = selected_object as WebTVPartition;

            if (part == null)
            {
                var _part_index = selected_object as int?;
                var _part_entry = selected_object as partition_entry?;

                if (_part_entry != null)
                {
                    for (var i = 0; i < this.webtv_disks.Count; i++)
                    {
                        for (var ii = 0; ii < this.webtv_disks[i].partition_table.Count; ii++)
                        {
                            var __part = this.webtv_disks[i].partition_table[ii];

                            if (__part.name == ((partition_entry)_part_entry).name && __part.sector_start == BigEndianConverter.ToUInt32(((partition_entry)_part_entry).sector_start, 0))
                            {
                                part = __part;
                                break;
                            }
                        }
                    }
                }
                else if (_part_index != null)
                {
                    for (var i = 0; i < this.webtv_disks.Count; i++)
                    {
                        for (var ii = 0; ii < this.webtv_disks[i].partition_table.Count; ii++)
                        {
                            var __part = this.webtv_disks[i].partition_table[ii];

                            if ((int)_part_index == ii)
                            {
                                part = __part;
                                break;
                            }
                        }
                    }
                }
            }

            if (part != null)
            {
                this.disk_view.SelectedDisk = null;
                this.disk_view.SelectedPartition = part;

                this.set_tree_selection_by_id(part.id);
            }
        }

        void select_disk(object selected_object)
        {
            var disk = selected_object as WebTVDisk;

            if (disk != null)
            {
                this.disk_view.SelectedDisk = disk;
                this.disk_view.SelectedPartition = null;

                this.set_tree_selection_by_id(disk.id);
            }
        }

        #region Ribbon button callbacks
        public void on_add_disk_image()
        {
            OpenFileDialog file_dialog = new OpenFileDialog();
            file_dialog.Filter = "Raw Disk Image Files (*.img, *.bin, *.ima)|*.img;*.bin;*.ima|All Files (*.*)|*.*";
            file_dialog.Title = "Select WebTV Disk Image";


            if (file_dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var existing_disk = this.webtv_disks.get_disk_by_path(file_dialog.FileName);

                    if (existing_disk != null)
                    {
                        this.select_disk(existing_disk);
                    }
                    else
                    {
                        var added_disk = this.webtv_disks.add_disk_image(file_dialog.FileName);

                        if (added_disk != null)
                        {
                            this.select_disk(added_disk);
                        }
                    }

                    if (this.disk_view.disk_collection_tree.Focusable)
                    {
                        this.disk_view.disk_collection_tree.Focus();
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error adding disk! " + ex.Message);
                }
            }
        }

        public void on_add_physical_disk()
        {
            var add_disk_dialog = new AddDisk(this.webtv_disks);

            add_disk_dialog.Owner = this.main_window;

            add_disk_dialog.ShowDialog();

            if (this.disk_view.disk_collection_tree.Focusable)
            {
                this.disk_view.disk_collection_tree.Focus();
            }
        }

        public void on_remove_disk()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                var current_index = this.webtv_disks.get_index(disk);

                this.webtv_disks.remove_disk(disk);

                if (current_index >= 0 && this.webtv_disks.Count > 0)
                {
                    if (current_index >= this.webtv_disks.Count)
                    {
                        this.select_disk(this.webtv_disks[this.webtv_disks.Count - 1]);
                    }
                    else
                    {
                        this.select_disk(this.webtv_disks[current_index]);
                    }
                }
                else
                {
                    this.clear_selection();

                    if (this.main_window.Focusable)
                    {
                        this.main_window.Focus();
                    }
                }
            }
        }

        public void on_initialize_disk()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                if (disk.size_bytes >= WebTVPartitionManager.MINIMUM_DISK_SIZE)
                {
                    var initilize_disk_dialog = new DiskInitialize(disk);

                    initilize_disk_dialog.Owner = this.main_window;

                    initilize_disk_dialog.ShowDialog();

                    this.reset_disk_partitions(disk, true, (int?)0);
                }
                else
                {
                    System.Windows.MessageBox.Show("The disk is too small to be initilized.  It must be at least " + BytesToString.bytes_to_iec(WebTVPartitionManager.MINIMUM_DISK_SIZE) + ".");
                }
            }
        }

        public void reset_disk_partitions(WebTVDisk disk, bool update_disk_state = false, object selected_partition = null)
        {
            if (update_disk_state)
            {
                disk.detect_collation(disk.io.byte_converter);

                var partition_manager = new WebTVPartitionManager(disk);
                disk.detect_layout(partition_manager);

                disk.update_disk_state();

                this.disk_view.ItemsSource = null;
                this.disk_view.ItemsSource = this.webtv_disks;
            }
            else
            {
                disk.enumerate_partitions();
            }

            this.clear_selection();
            this.refresh_tree();
            if (this.disk_view.disk_collection_tree.Focusable)
            {
                this.disk_view.disk_collection_tree.Focus();
            }
            if (selected_partition != null)
            {
                this.select_partition(selected_partition);
            }
            else
            {
                this.select_disk(disk);
            }
        }

        public void on_add_partition()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                var part = this.disk_view.SelectedPartition;

                var add_partition_dialog = new AddPartition(disk, part);

                add_partition_dialog.Owner = this.main_window;

                add_partition_dialog.ShowDialog();

                if(add_partition_dialog.AddedPartition != null)
                {
                    this.reset_disk_partitions(disk, false, add_partition_dialog.AddedPartition);
                }
            }
        }

        public void on_consolidate_free()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                var partition_manager = new WebTVPartitionManager(disk);

                partition_manager.consolidate_free_partitions();

                this.reset_disk_partitions(disk);
            }
        }

        public void on_delete_partition()
        {
            if (this.disk_view.SelectedPartition != null)
            {
                var part = this.disk_view.SelectedPartition;

                if(part != null)
                {
                    var dialog_result = MessageBoxResult.No;

                    if (part.sector_start == 0)
                    {
                        dialog_result = System.Windows.MessageBox.Show("!!! WARNING !!!! Are you sure you want to delete the '" + part.name + "' partition?  The first partition is used for permanently addressed data and there is no undo button!",
                                                                           "Don't run with scissors",
                                                                           System.Windows.MessageBoxButton.YesNo);
                    }
                    else
                    {
                        dialog_result = System.Windows.MessageBox.Show("Are you sure you want to delete the '" + part.name + "' partition?  There is no undo button!",
                                                                           "Don't run with scissors",
                                                                           System.Windows.MessageBoxButton.YesNo);
                    }

                    if (dialog_result == MessageBoxResult.Yes)
                    {
                        var current_index = part.disk.partition_table.get_index(part);

                        this.do_unmount_partition(part);

                        var partition_manager = new WebTVPartitionManager(part.disk);

                        partition_manager.delete_partition(part);

                        if (current_index >= 0 && part.disk.partition_table.Count > 0)
                        {
                            this.reset_disk_partitions(part.disk, false, (int?)current_index);
                        }
                        else
                        {
                            this.reset_disk_partitions(part.disk);
                        }
                    }
                }
            }
        }

        public void on_xxx()
        {

        }

        public void on_inspect_webtv_build()
        {
            if (this.disk_view.SelectedPartition != null
            && (this.disk_view.SelectedPartition.type == PartitionType.RAW
            || (this.disk_view.SelectedPartition.disk.layout == DiskLayout.WEBSTAR && this.disk_view.SelectedPartition.type == PartitionType.RAW2)))
            {
                var build_dialog = new BuildView(this.disk_view.SelectedPartition, false);

                build_dialog.Owner = this.main_window;

                build_dialog.ShowDialog();
            }
        }

        public void on_edit_nvram()
        {
            var nvram_dialog = new NVRAMView(this.get_selected_disk(), false);

            nvram_dialog.Owner = this.main_window;

            nvram_dialog.ShowDialog();
        }

        public void on_show_about_box()
        {
            var about_box = new AboutBox(this);

            about_box.Owner = this.main_window;

            about_box.ShowDialog();
        }

        public void on_build_info()
        {
            var build_dialog = new BuildView(this.disk_view.SelectedPartition, true);

            build_dialog.Owner = this.main_window;

            build_dialog.ShowDialog();
        }

        public void on_inspect_dvr_recordings()
        {
            var build_dialog = new DVRView(this.disk_view.SelectedPartition);

            build_dialog.Owner = this.main_window;

            build_dialog.ShowDialog();
        }

        public bool can_mount_partitions()
        {
            var imdisk_info = new ImDiskInfo();

            return imdisk_info.IsInstalled();
        }

        public void on_mount_partition()
        {
            if (this.disk_view.SelectedPartition != null
             && (this.disk_view.SelectedPartition.type == PartitionType.FAT16 || this.disk_view.SelectedPartition.delegated_type == PartitionType.FAT16)
             && !this.disk_view.SelectedPartition.has_device_attached())
            {
                if (this.can_mount_partitions())
                {
                    var mount_dialog = new MountPartition(this.disk_view.SelectedPartition);

                    mount_dialog.Owner = this.main_window;

                    mount_dialog.ShowDialog();
                }
                else
                {
                    var dialog_result = System.Windows.MessageBox.Show("ImDisk Virtual Disk Driver must be installed in order to mount a WebTV partition.  Do you want to go to the ImDisk website to download the ImDisk install package?",
                                                                       "Mount Error",
                                                                       System.Windows.MessageBoxButton.YesNo);

                    if (dialog_result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(ImDiskInfo.HUMAN_DOWNLOAD_URL);
                    }
                }
            }
        }

        public void do_unmount_partition(WebTVPartition part)
        {
            if (part != null 
             && part.has_device_attached())
            {
                var wait_window = new WaitMessage("Unmounting Device...");
                wait_window.Owner = this.main_window;
                wait_window.Go(() =>
                {
                    try
                    {
                        part.unmount();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Error unmounting disk! " + ex.Message);
                    }
                }, true);
            }
        }

        public void on_unmount_partition()
        {
            if (this.disk_view.SelectedPartition != null
             && (this.disk_view.SelectedPartition.type == PartitionType.FAT16 || this.disk_view.SelectedPartition.delegated_type == PartitionType.FAT16))
            {
                this.do_unmount_partition(this.disk_view.SelectedPartition);
            }
        }

        private void export_thread(WebTVDiskExporter.block_written progress_callback, ObjectLocation build_location)
        {
            WebTVDiskExporter exporter = null;

            try
            {
                exporter = new WebTVDiskExporter(this.io_source, this.io_bounds, this.image_filename);

                exporter.export_image(this.progress_window.write_progress);

            }
            catch (Exception e)
            {
                if (e.Message != "Thread was being aborted.")
                {
                    System.Windows.MessageBox.Show("Error! " + e.Message);
                }
            }
            finally
            {
                if (exporter != null)
                {
                    exporter.close_exporter();
                    exporter = null;
                }

                this.done_progress();
            }
        }

        public void export_progress_window_loaded(object sender, EventArgs e)
        {
            this.progress_thread = new Thread(() => this.export_thread(this.progress_window.write_progress, null));
            this.progress_thread.Start();
        }

        private void import_thread(WebTVDiskExporter.block_written progress_callback, ObjectLocation build_location)
        {
            WebTVDiskImporter importer = null;

            try
            {
                importer = new WebTVDiskImporter(this.io_source, this.io_bounds, this.image_filename);

                importer.import_image(this.progress_window.write_progress);
            }
            catch (Exception e)
            {
                if (e.Message != "Thread was being aborted.")
                {
                    System.Windows.MessageBox.Show("Error! " + e.Message);
                }
            }
            finally
            {
                if (importer != null)
                {
                    importer.close_importer();
                    importer = null;
                }

                this.done_progress(this.io_source.disk);
            }
        }

        public void import_progress_window_loaded(object sender, EventArgs e)
        {
            this.progress_thread = new Thread(() => this.import_thread(this.progress_window.write_progress, null));
            this.progress_thread.Start();
        }

        public void progress_window_unloaded(Object sender, RoutedEventArgs e)
        {
            if (this.progress_thread != null)
            {
                this.progress_thread.Abort();
            }
        }

        public void done_progress(WebTVDisk disk = null)
        {
            if (this.main_window.Dispatcher.CheckAccess() == false)
            {
                var cb = new disk_progress_callback(this.done_progress);

                this.main_window.Dispatcher.Invoke(cb, disk);
            }
            else
            {
                if (this.progress_window != null)
                {
                    this.progress_window.close_window();
                }

                if (disk != null)
                {
                    this.reset_disk_partitions(disk, true);
                }
            }
        }
        public void on_export(WebTVDiskIO io, DiskCageBounds bounds)
        {
            if(io != null)
            {
                System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
                file_dialog.Filter = "Image Files (*.img, *.bin, *.ima, *.o)|*.img;*.bin;*.ima;*.o|All Files (*.*)|*.*";
                file_dialog.Title = "Export Image";

                if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && file_dialog.FileName != null && file_dialog.FileName != "")
                {
                    this.image_filename = file_dialog.FileName;

                    this.io_source = io;
                    this.io_bounds = bounds;

                    this.progress_window = new ProgressWindow(export_progress_window_loaded, progress_window_unloaded, this.main_window);
                    this.progress_window.Go();
                }
            }
        }

        public void on_import(WebTVDiskIO io, DiskCageBounds bounds)
        {
            if (io != null)
            {
                System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
                file_dialog.Filter = "Image Files (*.img, *.bin, *.ima, *.o)|*.img;*.bin;*.ima;*.o|All Files (*.*)|*.*";
                file_dialog.Title = "Import Image";

                if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && file_dialog.FileName != null && file_dialog.FileName != "")
                {
                    var file_info = new FileInfo(file_dialog.FileName);

                    var bounds_length = bounds.end_position - bounds.start_position;

                    if (file_info.Length > (long)bounds_length)
                    {
                        var dialog_result = System.Windows.MessageBox.Show("The image file goes beyond the import location boundry! Do you want to continue?",
                                                                           "Buonds Error",
                                                                           System.Windows.MessageBoxButton.YesNo);

                        if (dialog_result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    this.image_filename = file_dialog.FileName;

                    this.io_source = io;
                    this.io_bounds = bounds;

                    this.progress_window = new ProgressWindow(import_progress_window_loaded, progress_window_unloaded, this.main_window);
                    this.progress_window.Go();
                }
            }
        }

        public void on_export_used_disk()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                DiskCageBounds used_bounds;

                used_bounds.start_position = 0;
                used_bounds.end_position = 0;
                for (var i = 0; i < disk.partition_table.Count; i++)
                {
                    var part = disk.partition_table[i];

                    var end_position = ((part.sector_start + part.sector_length) * disk.sector_bytes_length);
                    if (end_position > used_bounds.end_position)
                    {
                        used_bounds.end_position = end_position;
                    }
                }

                if(used_bounds.end_position == 0)
                {
                    used_bounds.end_position = (ulong)disk.io.Length;
                }

                this.on_export(disk.io, used_bounds);
            }
        }

        public void on_export_disks()
        {
            var disk = get_selected_disk();

            if(disk != null)
            {
                this.on_export(disk.io, disk.io.get_object_bounds());
            }
        }

        public void on_import_disk()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                this.on_import(disk.io, disk.io.get_object_bounds());
            }
        }

        public void on_export_partition_table()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
                file_dialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                file_dialog.Title = "Export Partition Table";

                if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && file_dialog.FileName != null && file_dialog.FileName != "")
                {

                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new WebTVJSONContractResolver(),
                        Converters = new[] { new WebTVJSONConverter() },
                    };
                    var json = JsonConvert.SerializeObject(disk.partition_table, settings);

                    File.WriteAllText(file_dialog.FileName, json);
                }
            }
        }

        public void on_import_partition_table()
        {
            var disk = get_selected_disk();

            if (disk != null)
            {
                System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
                file_dialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
                file_dialog.Title = "Import Partition Table";

                if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && file_dialog.FileName != null && file_dialog.FileName != "")
                {
                    try
                    {
                        var json = File.ReadAllText(file_dialog.FileName);

                        var _partition_table = JsonConvert.DeserializeObject<WebTVPartitionCollection>(json);

                        var partition_manager = new WebTVPartitionManager(disk);

                        partition_manager.write_partition_table(partition_manager.partition_table_to_data(_partition_table));

                        partition_manager.write_partition_sectors(_partition_table);

                        this.reset_disk_partitions(disk, true);
                    }
                    catch (Exception e)
                    {
                        System.Windows.MessageBox.Show("Error trying to import partition table! " + e.Message);
                    }
                }
            }
        }

        public void on_export_partition()
        {
            WebTVPartition partition = null;
            if (this.disk_view.SelectedPartition != null)
            {
                partition = this.disk_view.SelectedPartition;
            }

            if (partition != null)
            {
                this.on_export(partition.disk.io, partition.get_object_bounds());
            }
        }

        public void on_import_partition()
        {
            WebTVPartition partition = null;
            if (this.disk_view.SelectedPartition != null)
            {
                partition = this.disk_view.SelectedPartition;
            }

            if (partition != null)
            {
                this.on_import(partition.disk.io, partition.get_object_bounds());
            }
        }

        public void on_explore_partition()
        {
            if (this.disk_view.SelectedPartition != null
             && (this.disk_view.SelectedPartition.type == PartitionType.FAT16 || this.disk_view.SelectedPartition.delegated_type == PartitionType.FAT16)
             && this.disk_view.SelectedPartition.has_device_attached())
            {
                try
                {
                    var drive_letter = this.disk_view.SelectedPartition.server.get_drive_letter();

                    if (drive_letter != "")
                    {
                        var explorer_start = new ProcessStartInfo()
                        {
                            UseShellExecute = true,
                            Verb = "open",
                            FileName = drive_letter + @":\",
                            WorkingDirectory = drive_letter + @":\",
                            WindowStyle = ProcessWindowStyle.Normal
                        };

                        Process.Start(explorer_start);
                    }
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Error trying to explore disk! " + e.Message);
                }
            }
        }

        public void on_copy_partition_data(string copy_what)
        {
            var copy_string = "";

            var disk = this.get_selected_disk();
            if(disk != null)
            {
                ulong partition_start_offset = 0;
                ulong partition_end_offset = 0;

                if (this.disk_view.SelectedPartition != null)
                {
                    partition_start_offset = this.disk_view.SelectedPartition.sector_start * this.disk_view.SelectedPartition.disk.sector_bytes_length;
                    partition_end_offset = (this.disk_view.SelectedPartition.sector_start + this.disk_view.SelectedPartition.sector_length) * this.disk_view.SelectedPartition.disk.sector_bytes_length;
                }

                switch (copy_what)
                {
                    case "start-address":
                        copy_string = partition_start_offset.ToString("X");
                        break;

                    case "data-start-address":
                        partition_start_offset += WebTVPartitionManager.PARTITON_DATA_OFFSET;

                        copy_string = partition_start_offset.ToString("X");
                        break;

                    case "end-address":
                        copy_string = partition_end_offset.ToString("X");
                        break;

                    case "table-address":
                        copy_string = WebTVPartitionManager.get_partition_table_offset(disk).ToString("X");
                        break;
                }
            }

            if (copy_string != "")
            {
                try
                {
                    System.Windows.Clipboard.SetData(System.Windows.DataFormats.Text, (Object)("0x" + copy_string));
                }
                catch { }
            }
        }
        #endregion

        public WebTVDisk get_selected_disk()
        {
            if (this.disk_view.SelectedPartition != null || this.disk_view.SelectedDisk != null)
            {
                var disk = this.disk_view.SelectedDisk;
                var part = this.disk_view.SelectedPartition;

                if (disk == null)
                {
                    disk = part.disk;
                }

                return disk;
            }

            return null;
        }

        public bool check_fart_award()
        {
            this.main_window.FarterAwarded = false;
            this.main_window.fart_it_out.Visibility = Visibility.Collapsed;

            var value = Registry.get("FarterAwarded");

            if ((string)value == "True" || (string)value == "true")
            {
                this.main_window.FarterAwarded = true;
                this.main_window.fart_it_out.Visibility = Visibility.Visible;
            }

            return (bool)this.main_window.FarterAwarded;
        }

        public void activate_the_farts()
        {
            Registry.set("FarterAwarded", true);

            this.main_window.FarterAwarded = true;
            this.main_window.fart_it_out.Visibility = Visibility.Visible;
        }

        public void do_fart_it_out()
        {
            System.IO.Stream[] farts =
            {
                disk_editor.Properties.Resources.fart1,
                disk_editor.Properties.Resources.fart2,
                disk_editor.Properties.Resources.fart3,
                disk_editor.Properties.Resources.fart4,
                disk_editor.Properties.Resources.fart5,
                disk_editor.Properties.Resources.fart6,
                disk_editor.Properties.Resources.fart7,
                disk_editor.Properties.Resources.fart8,
                disk_editor.Properties.Resources.fart9,
                disk_editor.Properties.Resources.fart10
            };

            var player = new System.Media.SoundPlayer(farts[new Random().Next(farts.Length)]);
            player.Play();
        }

        public void do_polyzoot()
        {
            var player = new System.Media.SoundPlayer(disk_editor.Properties.Resources.polyzoot);
            player.PlayLooping();
        }

        private void enter_secret_code_mode()
        {
            this.entered_secret_code_mode = true;
            this.current_secret_code = "";
        }
        private void exit_secret_code_mode()
        {
            this.entered_secret_code_mode = false;
            this.current_secret_code = "";
        }
        private void secret_code_check(System.Windows.Input.KeyEventArgs input)
        {
            if (input.Key == Key.LeftAlt || input.Key == Key.RightAlt || input.Key == Key.System || input.Key == Key.Delete)
            {
                this.enter_secret_code_mode();
            }
            else if (this.entered_secret_code_mode)
            {
                var current_number = "";

                if (input.Key == Key.D0)
                {
                    current_number = "0";
                }
                else if (input.Key == Key.D1)
                {
                    current_number = "1";
                }
                else if (input.Key == Key.D2)
                {
                    current_number = "2";
                }
                else if (input.Key == Key.D3)
                {
                    current_number = "3";
                }
                else if (input.Key == Key.D4)
                {
                    current_number = "4";
                }
                else if (input.Key == Key.D5)
                {
                    current_number = "5";
                }
                else if (input.Key == Key.D6)
                {
                    current_number = "6";
                }
                else if (input.Key == Key.D7)
                {
                    current_number = "7";
                }
                else if (input.Key == Key.D8)
                {
                    current_number = "8";
                }
                else if (input.Key == Key.D9)
                {
                    current_number = "9";
                }
                else
                {
                    this.exit_secret_code_mode();
                }

                if (current_number != "")
                {
                    if(this.current_secret_code.Length > 10)
                    {
                        this.current_secret_code = current_number;
                    }
                    else
                    {
                        this.current_secret_code += current_number;
                    }

                    if(this.secret_commands != null && this.secret_commands.ContainsKey(this.current_secret_code))
                    {
                        this.secret_commands[this.current_secret_code].Invoke();

                        this.exit_secret_code_mode();
                    }
                }
            }
        }
        public void setup_secret_codes()
        {
            this.secret_commands = new Dictionary<string, Action>()
            {
                { "6969",       do_polyzoot },
                { "411",        on_show_about_box },
                { "8675309",    do_fart_it_out },
            };
        }

        private void on_key_input_action(object e)
        {
            var input = e as System.Windows.Input.KeyEventArgs;

            if (this.main_window.IsKeyboardFocused || this.main_window.IsKeyboardFocusWithin)
            {
                if (input != null)
                {
                    this.secret_code_check(input);

                    if (input.Key == Key.Delete)
                    {
                        this.delete_action();
                    }
                    else if (input.Key == Key.Enter)
                    {
                        this.dblclk_disk_action(null);
                    }
                    else if (input.Key == Key.Up || input.Key == Key.Left)
                    {
                        this.select_previous(Keyboard.IsKeyDown(Key.LeftCtrl));
                    }
                    else if (input.Key == Key.Down || input.Key == Key.Right || input.Key == Key.Tab)
                    {
                        this.select_next(Keyboard.IsKeyDown(Key.LeftCtrl));
                    }
                }
            }
        }

        private void select_previous(bool move_by_disk = false)
        {
            if (this.disk_view.SelectedPartition != null)
            {
                var disk_index = this.webtv_disks.get_index(this.disk_view.SelectedPartition.disk);
                var partition_index = this.disk_view.SelectedPartition.disk.partition_table.get_index(this.disk_view.SelectedPartition);

                if (move_by_disk || partition_index == 0)
                {
                    this.select_disk(this.disk_view.SelectedPartition.disk);
                }
                else if ((partition_index - 1) >= 0)
                {
                    this.select_partition(this.disk_view.SelectedPartition.disk.partition_table[(partition_index - 1)]);
                }
            }
            else if (this.disk_view.SelectedDisk != null)
            {
                var disk_index = this.webtv_disks.get_index(this.disk_view.SelectedDisk);

                if ((disk_index - 1) >= 0)
                {
                    if (!move_by_disk && this.webtv_disks[(disk_index - 1)].partition_table.Count > 0)
                    {
                        this.select_partition(this.webtv_disks[(disk_index - 1)].partition_table[this.webtv_disks[(disk_index - 1)].partition_table.Count - 1]);
                    }
                    else
                    {
                        this.select_disk(this.webtv_disks[(disk_index - 1)]);
                    }
                }
                else if(!move_by_disk && this.webtv_disks[this.webtv_disks.Count - 1].partition_table.Count > 0)
                {
                    this.select_partition(this.webtv_disks[this.webtv_disks.Count - 1].partition_table[this.webtv_disks[this.webtv_disks.Count - 1].partition_table.Count - 1]);
                }
                else
                {
                    this.select_disk(this.webtv_disks[this.webtv_disks.Count - 1]);
                }
            }
        }

        private void select_next(bool move_by_disk = false)
        {

            if (this.disk_view.SelectedPartition != null)
            {
                var disk_index = this.webtv_disks.get_index(this.disk_view.SelectedPartition.disk);
                var partition_index = this.disk_view.SelectedPartition.disk.partition_table.get_index(this.disk_view.SelectedPartition);

                if (!move_by_disk && (partition_index + 1) < this.disk_view.SelectedPartition.disk.partition_table.Count)
                {
                    this.select_partition(this.disk_view.SelectedPartition.disk.partition_table[(partition_index + 1)]);
                }
                else if ((disk_index + 1) < this.webtv_disks.Count)
                {
                    this.select_disk(this.webtv_disks[(disk_index + 1)]);
                }
                else
                {
                    this.select_disk(this.webtv_disks[0]);
                }
            }
            else if (this.disk_view.SelectedDisk != null)
            {
                var disk_index = this.webtv_disks.get_index(this.disk_view.SelectedDisk);

                if (!move_by_disk && this.disk_view.SelectedDisk.partition_table.Count > 0)
                {
                    this.select_partition(this.disk_view.SelectedDisk.partition_table[0]);
                }
                else if ((disk_index + 1) < this.webtv_disks.Count)
                {
                    this.select_disk(this.webtv_disks[(disk_index + 1)]);
                }
                else
                {
                    this.select_disk(this.webtv_disks[0]);
                }
            }
        }

        private void delete_action()
        {
            if (this.disk_view.SelectedPartition != null)
            {
                if (this.disk_view.SelectedPartition.type == PartitionType.FAT16
                 || this.disk_view.SelectedPartition.delegated_type == PartitionType.FAT16)
                {
                    if (this.disk_view.SelectedPartition.is_mounted)
                    {
                        on_unmount_partition();
                    }
                    else
                    {
                        on_delete_partition();
                    }
                }
                else if (this.disk_view.SelectedPartition.type == PartitionType.RAW
                     || this.disk_view.SelectedPartition.type == PartitionType.RAW2
                     || this.disk_view.SelectedPartition.type == PartitionType.DVRFS
                     || this.disk_view.SelectedPartition.type == PartitionType.DELEGATED)
                {
                    on_delete_partition();
                }
            }
            else if (this.disk_view.SelectedDisk != null)
            {
                on_remove_disk();
            }
        }

        private void dblclk_disk_action_mc(object sender, MouseButtonEventArgs args)
        {
            dblclk_disk_action(sender);
        }
        private void dblclk_disk_action(object selected_object)
        {
            if (this.disk_view.SelectedPartition != null)
            {
                if (this.disk_view.SelectedPartition.type == PartitionType.FAT16
                || this.disk_view.SelectedPartition.delegated_type == PartitionType.FAT16)
                {
                    if (this.disk_view.SelectedPartition.is_mounted)
                    {
                        on_explore_partition();
                    }
                    else
                    {
                        on_mount_partition();
                    }
                }
                else if (this.disk_view.SelectedPartition.type == PartitionType.RAW
                     || this.disk_view.SelectedPartition.type == PartitionType.RAW2)
                {
                    on_inspect_webtv_build();
                }
                else if (this.disk_view.SelectedPartition.delegated_type == PartitionType.DVRFS)
                {
                    on_inspect_dvr_recordings();
                }
                else if (this.disk_view.SelectedPartition.type == PartitionType.FREE
                    || this.disk_view.SelectedPartition.type == PartitionType.UNALLOCATED)
                {
                    on_add_partition();
                }
            }
            else if (this.disk_view.SelectedDisk != null)
            {
                if(this.disk_view.SelectedDisk.state == DiskState.NO_PARTITION_TABLE)
                {
                    on_initialize_disk();
                }
                else
                {
                    on_add_partition();
                }
            }
        }

        private void tree_selection_change(object sender, SelectionChangedEventArgs e)
        {
            var item = get_selected_tree_item();

            if (item != null)
            { 
                if(item.object_type == DiskTreeListViewDatum.ItemObjectType.DISK)
                {
                    var disk = this.webtv_disks.get_disk_by_id(item.id);

                    this.select_disk(disk);

                    this.scroll_to_visual_item_by_id(disk.id);
                }
                else if(item.object_type == DiskTreeListViewDatum.ItemObjectType.PARTITION)
                {
                    var part = this.webtv_disks.get_partition_by_id(item.id);

                    if (part != null)
                    {
                        this.select_partition(part);

                        this.scroll_to_visual_item_by_id(part.id);
                    }
                }
            }
        }

        private DiskTreeListViewDatum get_selected_tree_item()
        {
            var tree = this.disk_view.disk_collection_tree;

            if (tree != null)
            {
                var list_items = tree.Items;

                for (int i = 0; i < list_items.Count; i++)
                {
                    var node = list_items[i] as Aga.Controls.Tree.TreeNode;

                    if (node != null)
                    {
                        var item = node.Tag as DiskTreeListViewDatum;
                        var _item = tree.ItemContainerGenerator.ContainerFromItem(node) as TreeListItem;

                        if (_item != null && _item.IsSelected)
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        public void set_tree_selection_by_id(Guid id)
        {
            var tree = this.disk_view.disk_collection_tree;

            if (tree != null)
            {
                tree.UpdateLayout();

                var list_items = tree.Items;

                for (int i = 0; i < list_items.Count; i++)
                {
                    var node = list_items[i] as Aga.Controls.Tree.TreeNode;

                    if (node != null)
                    {
                        var item = node.Tag as DiskTreeListViewDatum;

                        if (item.id == id)
                        {
                            tree.Focus();

                            var _item = tree.ItemContainerGenerator.ContainerFromIndex(i) as TreeListItem;
                            if (_item != null)
                            {
                                _item.IsSelected = true;
                                _item.BringIntoView();
                                _item.Focus();
                            }

                            break;
                        }

                    }
                }
            }
        }

        public void refresh_tree()
        {
            if (this.disk_view.disk_collection_tree.Dispatcher.CheckAccess() == false)
            {
                var cb = new void_callback(refresh_tree);

                this.disk_view.Dispatcher.Invoke(cb, null);
            }
            else
            {
                this.disk_view.disk_collection_tree.RefreshList();

                if (this.disk_view.SelectedPartition != null)
                {
                    this.set_tree_selection_by_id(this.disk_view.SelectedPartition.id);
                }
                else if (this.disk_view.SelectedDisk != null)
                {
                    this.set_tree_selection_by_id(this.disk_view.SelectedDisk.id);
                }
            }
        }

        public void scroll_to_visual_item_by_id(Guid id)
        {
            var scroller = this.find_element_by_id("disk_collection_visual_scroller", this.disk_view.disk_collection_visual) as ScrollViewer;
            if (scroller != null)
            {
                var scroll_index = this.find_scroll_index_by_id(scroller, id.ToString());

                if (scroll_index > -1)
                {
                    scroller.ScrollToVerticalOffset(scroll_index);
                }
            }
        }

        public void mounted_disks_changed(Object sender, EventArgs e)
        {
            this.disks_changed(sender, null);
        }

        public void disks_changed(Object sender, NotifyCollectionChangedEventArgs e)
        {
            this.refresh_tree();
        }

        public double find_scroll_index_by_id(ScrollViewer scroller, string id, bool ignore_if_visible = true, DependencyObject parent = null, double current_index = 0)
        {
            bool is_disk_stack_parent = false;

            if (parent == null)
            {
                parent = scroller;
            }

            if (parent != null)
            {
                var _parent = parent as UIElement;
                if (_parent.Uid == "disk_collection_visual_stack")
                {
                    is_disk_stack_parent = true;
                }

                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var el = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                    if (el != null)
                    {
                        if (el.Uid != "")
                        {
                            if (el.Uid == id)
                            {
                                if (ignore_if_visible)
                                {
                                    var childTransform = el.TransformToAncestor(scroller);
                                    var rectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), el.RenderSize));
                                    var result = Rect.Intersect(new Rect(new Point(0, 0), scroller.RenderSize), rectangle);
                                    
                                    if (result != Rect.Empty && result.Size.Height > 25)
                                    {
                                        return -1;
                                    }
                                }

                                return current_index;
                            }
                        }

                        var _current_index = this.find_scroll_index_by_id(scroller, id, ignore_if_visible, el, current_index);
                        if (_current_index > -1)
                        {
                            return _current_index;
                        }

                        if (is_disk_stack_parent)
                        {
                            current_index += el.ActualHeight;
                        }
                    }
                }
            }

            return -1;
        }

        public FrameworkElement find_element_by_id(string id, DependencyObject parent = null)
        {
            if (parent == null)
            {
                parent = this.disk_view;
            }

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var el = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                if (el != null)
                {
                    if (el.Uid != "")
                    {
                        if (el.Uid == id)
                        {
                            return el;
                        }
                    }

                    el = this.find_element_by_id(id, el);

                    if (el != null)
                    {
                        return el;
                    }
                }
            }

            return null;
        }

        private void clenup_application(object sender, EventArgs e)
        {
            if (this.can_mount_partitions())
            {
                ImDiskAPI.DriveListChanged -= this.mounted_disks_changed;
            }

            if (this.webtv_disks != null)
            {
                var wait_window = new WaitMessage("Cleaning Up...");
                wait_window.Owner = this.main_window;
                wait_window.Go(() =>
                {
                    this.webtv_disks.clear_disks();

                    this.webtv_disks = null;
                }, true, true);
            }
        }

        public void check_in_wine()
        {
            this.main_window.RunningInWine = Wine.is_in_wine();
        }

        public MainViewModel(MainWindow main_window, DiskView window_disk_view)
        {
            /*
            * Check:
            *  this.disk_view != null
            *  this.disk_view.disk_collection_tree != null
            *  this.disk_view.disk_collection_visual != null
            */

            this.main_window = main_window;
            this.main_window.KeyInputCommand = this.key_input_action;
            this.disk_view = window_disk_view;

            this.webtv_disks = new WebTVDiskCollection();

            this.disk_view.ItemsSource = this.webtv_disks;
            this.disk_view.SelectPartitionCommand = this.select_partition_command;
            this.disk_view.SelectDiskCommand = this.select_disk_command;
            this.disk_view.DoubleClickCommand = this.dblclk_disk_action_ic;
            this.disk_view.KeyInputCommand = this.key_input_action;
            this.webtv_disks.CollectionChanged += new NotifyCollectionChangedEventHandler(this.disks_changed);
            if (this.can_mount_partitions())
            {
                ImDiskAPI.DriveListChanged += this.mounted_disks_changed;
            }

            this.disk_view.disk_collection_tree.SelectionChanged += this.tree_selection_change;
            this.disk_view.disk_collection_tree.Model = new DiskCollectionTreeModel(this.webtv_disks);
 
            main_window.Closing += clenup_application;
            AppDomain.CurrentDomain.UnhandledException += clenup_application;

            this.setup_secret_codes();
            this.check_fart_award();
            this.check_in_wine();
        }
    }
}
