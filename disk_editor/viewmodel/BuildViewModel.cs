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

namespace disk_editor
{
    class BuildViewModel : INotifyPropertyChanged
    {
        private delegate void void_call();

        public BuildView build_dialog { get; set; }
        public WebTVPartition part { get; set; }
        public string exported_build_filename;
        public uint calculated_code_checksum;
        public uint calculated_romfs_checksum;
        public ObjectLocationSelected browser_selector;

        private WaitMessage wait_window;
        private delegate void build_info_callback(WebTVBuildInfo build, WebTVBuild build_header, string pane_prefix, uint calculated_code_checksum, ulong file_offset_bytes = 0);

        private ProgressWindow progress_window;
        private Thread progress_thread;

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
        
        private RelayCommand _select_this_browser;
        public ICommand select_this_browser
        {
            get
            {
                if (_select_this_browser == null)
                {
                    _select_this_browser = new RelayCommand(x => on_select_this_browser(), x => true);
                }

                return _select_this_browser;
            }
        }

        private RelayCommand _export_build_image;
        public ICommand export_build_image
        {
            get
            {
                if (_export_build_image == null)
                {
                    _export_build_image = new RelayCommand(x => on_export_build_image(), x => true);
                }

                return _export_build_image;
            }
        }

        private RelayCommand _load_build_image;
        public ICommand load_build_image
        {
            get
            {
                if (_load_build_image == null)
                {
                    _load_build_image = new RelayCommand(x => on_load_build_image(), x => true);
                }

                return _load_build_image;
            }
        }
        
        private RelayCommand _write_image_command;
        public ICommand write_image_command
        {
            get
            {
                if (_write_image_command == null)
                {
                    _write_image_command = new RelayCommand(x => on_write_image_command(), x => true);
                }

                return _write_image_command;
            }
        }

        public void done_progress()
        {
            if (this.build_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new void_call(this.done_progress);

                this.build_dialog.Dispatcher.Invoke(cb);
            }
            else
            {
                this.build_dialog.IsWriting = false;

                if (this.progress_window != null)
                {
                    this.progress_window.close_window();
                }

                this.set_current_build_info();
            }
        }

        public void close_window()
        {
            this.build_dialog.Close();
        }

        public void on_cancel_click()
        {
            close_window();
        }

        public void on_select_this_browser()
        {
            if (this.browser_selector != null)
            {
                this.build_dialog.IsWriting = true;
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                this.browser_selector.set_selected(this.build_dialog.SelectedObjectLocation.type);
                this.build_dialog.IsWriting = false;

                this.update_build_selection(this.build_dialog.SelectedObjectLocation.type);
            }
        }

        public void on_export_build_image()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "ROM Image Files (*.o, *.img, *.bin, *.ima)|*.o;*.img;*.bin;*.ima|All Files (*.*)|*.*";
            file_dialog.Title = "Export WebTV ROM Image";

            if(file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                this.exported_build_filename = file_dialog.FileName;
                this.build_dialog.IsWriting = true;

                this.progress_window = new ProgressWindow(export_progress_window_loaded, progress_window_unloaded, this.build_dialog);
                this.progress_window.Go();
            }
        }

        public void on_load_build_image()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "ROM Image Files (*.o, *.img, *.bin, *.ima)|*.o;*.img;*.bin;*.ima|All Files (*.*)|*.*";
            file_dialog.Title = "Select WebTV ROM Image";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.build_dialog.NewBuildFilename = file_dialog.FileName;

                set_new_build_info();
            }
        }

        public void export_thread(WebTVDiskExporter.block_written progress_callback, ObjectLocation build_location)
        {
            WebTVDiskExporter exporter = null;

            try
            {
                exporter = new WebTVDiskExporter(this.part, build_location, this.exported_build_filename);

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
            }

            this.done_progress();
        }

        public void export_progress_window_loaded(object sender, EventArgs e)
        {
            var build_location = this.build_dialog.SelectedObjectLocation as ObjectLocation;

            this.progress_thread = new Thread(() => this.export_thread(this.progress_window.write_progress, build_location));
            this.progress_thread.Start();
        }

        public void writing_thread(WebTVBuildWriter.block_written progress_callback, ObjectLocation build_location, bool new_use_calculated_code_checksum, bool new_use_calculated_romfs_checksum, string build_filename)
        {
            WebTVBuildWriter build_writer = null;

            try
            {
                build_writer = new WebTVBuildWriter(this.part, build_location, build_filename);

                if (new_use_calculated_code_checksum)
                {
                    build_writer.set_code_checksum(this.calculated_code_checksum);
                }

                if (new_use_calculated_romfs_checksum)
                {
                    build_writer.set_romfs_checksum(this.calculated_romfs_checksum);
                }

                build_writer.write_build(this.progress_window.write_progress);

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
                if (build_writer != null)
                {
                    build_writer.close_writer();
                    build_writer = null;
                }

            }

            this.done_progress();
        }

        public void write_progress_window_loaded(object sender, EventArgs e)
        {
            var new_use_calculated_code_checksum = (bool)this.build_dialog.new_build_use_calculated_code_checksum.IsChecked;
            var new_use_calculated_romfs_checksum = (bool)this.build_dialog.new_build_use_calculated_code_checksum.IsChecked;

            var build_location = this.build_dialog.SelectedObjectLocation as ObjectLocation;
            var build_filename = this.build_dialog.NewBuildFilename;

            this.progress_thread = new Thread(() => this.writing_thread(this.progress_window.write_progress, build_location, new_use_calculated_code_checksum, new_use_calculated_romfs_checksum, build_filename));
            this.progress_thread.Start();
        }

        public void progress_window_unloaded(Object sender, RoutedEventArgs e)
        {
            if (this.progress_thread != null)
            {
                this.progress_thread.Abort();
            }
        }

        public void on_write_image_command()
        {
            if (this.build_dialog.NewBuildFilename != null && this.build_dialog.NewBuildFilename != "")
            {
                if (this.build_dialog.SelectedObjectLocation != null)
                {
                    var build_file_info = new FileInfo(this.build_dialog.NewBuildFilename);

                    if (build_file_info.Length > (uint)this.build_dialog.SelectedObjectLocation.size_bytes)
                    {
                        var dialog_result = System.Windows.MessageBox.Show("The build image file goes beyond the build location boundry! Do you want to continue?",
                                                                           "Buonds Error",
                                                                           System.Windows.MessageBoxButton.YesNo);

                        if (dialog_result != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }

                    this.build_dialog.IsWriting = true;

                    this.progress_window = new ProgressWindow(write_progress_window_loaded, progress_window_unloaded, this.build_dialog);
                    this.progress_window.Go();
                }
                else
                {
                    MessageBox.Show("You must choose a valid build image location!");
                }
            }
            else
            {
                MessageBox.Show("You must choose a build image file!");
            }
        }

        public void reset_build_page(string pane_prefix)
        {
            if (pane_prefix == "new")
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_path")).Text = "";
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_size")).Text = "-";
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_size")).Foreground = (Brush)Application.Current.FindResource("TextBoxTextBrush");
                ((CheckBox)this.build_dialog.FindName(pane_prefix + "_build_use_calculated_code_checksum")).IsChecked = true;
                ((CheckBox)this.build_dialog.FindName(pane_prefix + "_build_use_calculated_romfs_checksum")).IsChecked = true;
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_collation")).Text = "-";
            }

            if (pane_prefix == "info")
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_size")).Text = "-";
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_size")).Foreground = (Brush)Application.Current.FindResource("TextBoxTextBrush");
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_collation")).Text = "-";
            }

            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_number")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_flags")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_checksum")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_checksum")).Foreground = (Brush)Application.Current.FindResource("TextBoxTextBrush");
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_length")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_length")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_jump_offset")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_base")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_base")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_checksum")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_checksum")).Foreground = (Brush)Application.Current.FindResource("TextBoxTextBrush");
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_calculated_romfs_checksum")).Text = "-";
            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_size")).Text = "-";
        }

        public void do_postread(WebTVBuildInfo build, WebTVBuild build_header, string pane_prefix, uint calculated_code_checksum, ulong file_offset_bytes = 0)
        {
            if (this.build_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new build_info_callback(do_postread);

                this.build_dialog.Dispatcher.Invoke(cb, build, build_header, pane_prefix, calculated_code_checksum, file_offset_bytes);
            }
            else
            {
                this.calculated_code_checksum = calculated_code_checksum;

                this.style_build_pane_postread(build, build_header, pane_prefix, file_offset_bytes);

                this.close_wait_window();

                this.loading = false;
            }
        }

        public void close_wait_window()
        {
            if (this.wait_window != null)
            {
                if(this.wait_window.close_window())
                {
                    this.wait_window = null;
                }
            }
        }

        public void process_build_info(WebTVBuildInfo build, string pane_prefix, ulong file_offset_bytes = 0)
        {
            this.loading = true;

            if (this.wait_window == null)
            {
                this.wait_window = new WaitMessage("Loading Build...", this.build_dialog);
            }

            this.wait_window.Go(() =>
            {
                this.read_build_info(build, pane_prefix, file_offset_bytes);
            });
        }

        public void read_build_info(WebTVBuildInfo build, string pane_prefix, ulong file_offset_bytes = 0)
        {
            if (this.build_dialog != null)
            {
                var build_header = build.get_build_info();

                uint calculated_code_checksum = 0;
                if (!build_header.is_classic_build && build_header.build_number > 0 && build_header.dword_code_length > 0 && build_header.jump_offset > 4)
                {
                    calculated_code_checksum = build.calculate_code_checksum();
                }
                else
                {
                    calculated_code_checksum = this.calculated_code_checksum;
                }

                this.do_postread(build, build_header, pane_prefix, calculated_code_checksum, file_offset_bytes);
            }
        }

        public void style_build_pane_preread(WebTVBuildInfo build, string pane_prefix, string filename = "", ulong file_size_bytes = 0, ulong file_offset_bytes = 0, DiskByteTransform collation = DiskByteTransform.NOSWAP)
        {
            reset_build_page(pane_prefix);

            this.calculated_romfs_checksum = 0;
            this.calculated_code_checksum = 0;

            if (pane_prefix == "current")
            {
                this.build_dialog.CurrentLength = 0;
            }

            if (pane_prefix == "new" || (pane_prefix == "info" && filename != ""))
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_path")).Text = filename;

                ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_size")).Text = BytesToString.bytes_to_iec(file_size_bytes);

                var build_location = this.build_dialog.partition_build_locations.SelectedItem as ObjectLocation;

                if (pane_prefix == "new" && build_location != null && file_size_bytes > build_location.size_bytes)
                {
                    ((TextBox)this.build_dialog.FindName(pane_prefix + "_file_size")).Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                }

                var collation_description = "";
                switch (collation)
                {
                    case DiskByteTransform.BIT16SWAP:
                        collation_description = "16-bit swapped";
                        break;

                    case DiskByteTransform.BIT1632SWAP:
                        collation_description = "16+32-bit swapped";
                        break;

                    case DiskByteTransform.BIT32SWAP:
                        collation_description = "32-bit swapped";
                        break;

                    case DiskByteTransform.NOSWAP:
                    default:
                        collation_description = "no swapping";
                        break;
                }

                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_collation")).Text = collation_description;
            }

            this.process_build_info(build, pane_prefix, file_offset_bytes);
        }

        public void style_build_pane_postread(WebTVBuildInfo build, WebTVBuild build_header, string pane_prefix, ulong file_offset_bytes = 0)
        {
            var build_flags = new List<string>();

            if ((build_header.build_flags & 0x04) != 0)
            {
                build_flags.Add("Debug");
            }
            if ((build_header.build_flags & 0x20) != 0)
            {
                build_flags.Add("Satellite?");
            }
            if ((build_header.build_flags & 0x10) != 0)
            {
                build_flags.Add("Windows CE?");
            }
            if ((build_header.build_flags & 0x01) != 0)
            {
                build_flags.Add("Compressed Heap Data");
            }
            else
            {
                build_flags.Add("Raw Heap Data");
            }

            if (build_flags.Count > 0)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_flags")).Text = String.Join(", ", build_flags);
            }
            else
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_flags")).Text = "-";
            }

            if (build_header.is_classic_build || build_header.build_number <= 0 || build_header.dword_code_length <= 0 || build_header.jump_offset <= 4)
            {
                if (pane_prefix == "new")
                {
                    ((CheckBox)this.build_dialog.FindName(pane_prefix + "_build_use_calculated_code_checksum")).IsChecked = false;
                    ((CheckBox)this.build_dialog.FindName(pane_prefix + "_build_use_calculated_romfs_checksum")).IsChecked = false;
                }
            }

            if (build_header.build_number == 0)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_number")).Text = "?";
            }
            else
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_number")).Text = build_header.build_number.ToString();
            }

            if (build_header.is_classic_build)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_checksum")).Text = "Classic build?";
            }
            else
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_checksum")).Text = build_header.code_checksum.ToString("X");

                if (build_header.jump_offset > 4 && this.calculated_code_checksum != build_header.code_checksum)
                {
                    ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_checksum")).Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                }
            }

            if (build_header.jump_offset == 4)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_calculated_code_checksum")).Text = "Compressed build?";
            }
            else
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_calculated_code_checksum")).Text = this.calculated_code_checksum.ToString("X");
            }


            if (build_header.is_classic_build || build_header.dword_length == 0)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_length")).Text = "?";
            }
            else
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_length")).Text = this.get_size_string(build_header.dword_length);

                if(pane_prefix == "current")
                {
                    this.build_dialog.CurrentLength = build_header.dword_length;
                }
            }

            if (build_header.is_classic_build || build_header.dword_code_length == 0)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_length")).Text = "?";
            }
            else
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_length")).Text = this.get_size_string(build_header.dword_code_length);
            }

            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_jump_offset")).Text = this.get_offset_string((build_header.build_base_address + build_header.jump_offset), build_header.build_base_address, file_offset_bytes);

            ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_base")).Text = this.get_offset_string(build_header.build_base_address, build_header.build_base_address, file_offset_bytes);

            if (build_header.romfs_base_address == 0)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_base")).Text = "No ROMFS";

                if (pane_prefix == "new")
                {
                    ((CheckBox)this.build_dialog.FindName(pane_prefix + "_build_use_calculated_romfs_checksum")).IsChecked = false;
                }
            }
            else if (!build_header.is_classic_build)
            {
                this.calculated_romfs_checksum = build.calculate_romfs_checksum();

                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_base")).Text = this.get_offset_string(build_header.romfs_base_address, build_header.build_base_address, file_offset_bytes);

                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_checksum")).Text = build_header.romfs_checksum.ToString("X");
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_calculated_romfs_checksum")).Text = this.calculated_romfs_checksum.ToString("X");
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_size")).Text = this.get_size_string(build_header.dword_romfs_size);

                if (this.calculated_romfs_checksum != build_header.romfs_checksum)
                {
                    ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_romfs_checksum")).Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                }
            }

            if (!build_header.is_classic_build)
            {
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_heap_size")).Text = this.get_size_string(build_header.dword_heap_data_size + build_header.dword_heap_free_size);
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_heap_data_offset")).Text = this.get_offset_string(build_header.heap_data_address, build_header.build_base_address, file_offset_bytes);
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_heap_data_compressed_size")).Text = this.get_size_string(build_header.dword_heap_compressed_data_size);
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_heap_data_size")).Text = this.get_size_string(build_header.dword_heap_data_size);

                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_shrunk_offset")).Text = this.get_offset_string(build_header.code_compressed_address, build_header.build_base_address, file_offset_bytes);
                ((TextBox)this.build_dialog.FindName(pane_prefix + "_build_code_shrunk_size")).Text = this.get_size_string(build_header.dword_code_compressed_size);
            }

            build.Close();
        }

        public string get_offset_string(ulong offset, ulong build_base, ulong stream_offset)
        {
            var offset_string = offset.ToString("X");

            if (offset >= build_base)
            {
                offset_string += " {" + (stream_offset + (offset - build_base)).ToString("X") + "}";
            }

            return offset_string;
        }

        public string get_size_string(uint dword_size)
        { 
            var size = dword_size * WORD.DWORD_SIZE_BYTES;

            return BytesToString.bytes_to_iec(size) + " (" + dword_size.ToString("X") + ")";
        }

        public void set_build_info()
        {
            this.set_current_build_info();
            this.set_new_build_info();
        }

        public void set_new_build_info()
        {
            if (this.build_dialog.NewBuildFilename != null && this.build_dialog.NewBuildFilename != "")
            {
                try
                {
                    string pane_prefix = "new";

                    var build = new WebTVBuildInfo(this.build_dialog.NewBuildFilename);

                    if ((bool)this.build_dialog.OnlyInfo)
                    {
                        pane_prefix = "info";
                    }

                    if (!this.loading)
                    {
                        this.style_build_pane_preread(build, pane_prefix, this.build_dialog.NewBuildFilename, (ulong)build.io.Length, 0, build.byte_converter.byte_transform);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error! " + e.Message);
                }
            }
        }

        public void set_current_build_info()
        {
            this.build_dialog.SelectedObjectLocation = this.build_dialog.partition_build_locations.SelectedItem as ObjectLocation;

            if (this.part != null && this.build_dialog.SelectedObjectLocation != null)
            {
                string pane_prefix = "current";

                var build = new WebTVBuildInfo(this.part, this.build_dialog.SelectedObjectLocation);

                if ((bool)this.build_dialog.OnlyInfo)
                {
                    pane_prefix = "info";
                }

                if (!this.loading)
                {
                    this.style_build_pane_preread(build, pane_prefix, "", this.build_dialog.SelectedObjectLocation.size_bytes, this.build_dialog.SelectedObjectLocation.offset);
                }
            }
        }

        public void set_build_locations()
        {
            this.loading = true;

            this.build_dialog.BuildLocations = new ObjectLocationCollection();

            if (this.part != null)
            {
                if(this.browser_selector == null)
                {
                    this.browser_selector = new ObjectLocationSelected(ObjectLocationCategory.BUILD, this.part.disk);
                }

                this.build_dialog.BuildLocations.add_enumerated_build(this.part, this.browser_selector.get_selected());
            }

            if ((bool)this.build_dialog.OnlyInfo)
            {
                this.build_dialog.BuildLocations.Add(new ObjectLocation()
                {
                    type = ObjectLocationType.FILE_LOCATION,
                    offset = 0,
                    size_bytes = 0,
                });
            }

            this.update_build_selection(ObjectLocationType.UNKNOWN);

            this.loading = false;

            this.set_current_build_info();
        }

        void update_build_selection(ObjectLocationType selected_browser)
        {
            var selected_index = 0;

            if (this.part != null && this.browser_selector != null)
            {
                if (selected_browser == ObjectLocationType.UNKNOWN)
                {
                    selected_browser = this.browser_selector.get_selected();
                }

                for (var i = 0; i < this.build_dialog.BuildLocations.Count; i++)
                {
                    if (this.build_dialog.BuildLocations[i].type == selected_browser)
                    {
                        selected_index = i;
                        this.build_dialog.BuildLocations[i].selected = true;
                    }
                    else
                    {
                        this.build_dialog.BuildLocations[i].selected = false;
                    }
                }
            }

            this.build_dialog.SelectedBrowser = selected_browser;
            this.build_dialog.partition_build_locations.SelectedIndex = selected_index;
            this.build_dialog.SelectedObjectLocation = this.build_dialog.BuildLocations[selected_index];
            RaisePropertyChanged("");
        }

        public void style_partition_info()
        {
            if (this.part != null)
            {
                this.build_dialog.partition_name.Content = this.part.name;
                this.build_dialog.partition_size.Content = (new PartitionSizeConverter()).Convert(this.part, null, null, null).ToString();
            }
        }
        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            style_partition_info();
            set_build_locations();
        }

        public BuildViewModel(BuildView build_dialog, WebTVPartition part, bool? only_info)
        {
            this.build_dialog = build_dialog;
            this.part = part;

            this.build_dialog.IsWriting = false;
            this.build_dialog.OnlyInfo = only_info;

            this.build_dialog.SelectedObjectLocation = null;

            this.build_dialog.Loaded += window_loaded;
        }
    }
}
