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
using System.Collections.ObjectModel;
using System.Windows.Data;
using Newtonsoft.Json;

namespace disk_editor
{
    internal class NVRAMViewModel : INotifyPropertyChanged
    {
        private delegate void void_call();
        private delegate void error_message_call(string error_message);

        public NVRAMView nvram_dialog { get; set; }
        public WebTVDisk disk { get; set; }
        public ObjectLocationSelected nvram_selector;

        private WaitMessage wait_window;
        private delegate void nvram_info_callback(NVSettings nvram_settings, uint calculated_checksum, string error_message);

        private NVSettings nvram_settings;
        public uint calculated_checksum;

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
        

        private RelayCommand _select_this_nvram;
        public ICommand select_this_nvram
        {
            get
            {
                if (_select_this_nvram == null)
                {
                    _select_this_nvram = new RelayCommand(x => on_select_this_nvram_click(), x => true);
                }

                return _select_this_nvram;
            }
        }


        private RelayCommand _import_box_command;
        public ICommand import_box_command
        {
            get
            {
                if (_import_box_command == null)
                {
                    _import_box_command = new RelayCommand(x => on_import_box_click(), x => true);
                }

                return _import_box_command;
            }
        }
        private RelayCommand _export_box_command;
        public ICommand export_box_command
        {
            get
            {
                if (_export_box_command == null)
                {
                    _export_box_command = new RelayCommand(x => on_export_box_click(), x => true);
                }

                return _export_box_command;
            }
        }

        private RelayCommand _import_simnv_command;
        public ICommand import_simnv_command
        {
            get
            {
                if (_import_simnv_command == null)
                {
                    _import_simnv_command = new RelayCommand(x => on_import_simnv_click(), x => true);
                }

                return _import_simnv_command;
            }
        }
        private RelayCommand _export_simnv_command;
        public ICommand export_simnv_command
        {
            get
            {
                if (_export_simnv_command == null)
                {
                    _export_simnv_command = new RelayCommand(x => on_export_simnv_click(), x => true);
                }

                return _export_simnv_command;
            }
        }

        private RelayCommand _import_json_command;
        public ICommand import_json_command
        {
            get
            {
                if (_import_json_command == null)
                {
                    _import_json_command = new RelayCommand(x => on_import_json_click(), x => true);
                }

                return _import_json_command;
            }
        }
        private RelayCommand _export_json_command;
        public ICommand export_json_command
        {
            get
            {
                if (_export_json_command == null)
                {
                    _export_json_command = new RelayCommand(x => on_export_json_click(), x => true);
                }

                return _export_json_command;
            }
        }

        private RelayCommand _copy_command;
        public ICommand copy_command
        {
            get
            {
                if (_copy_command == null)
                {
                    _copy_command = new RelayCommand(x => on_copy_click((string)x), x => true);
                }

                return _copy_command;
            }
        }

        private RelayCommand _edit_command;
        public ICommand edit_command
        {
            get
            {
                if (_edit_command == null)
                {
                    _edit_command = new RelayCommand(x => on_edit_click((NVSetting)x), x => true);
                }

                return _edit_command;
            }
        
        }

        private RelayCommand _delete_command;
        public ICommand delete_command
        {
            get
            {
                if (_delete_command == null)
                {
                    _delete_command = new RelayCommand(x => on_delete_click((NVSetting)x), x => true);
                }

                return _delete_command;
            }

        }

        private RelayCommand _export_command;
        public ICommand export_command
        {
            get
            {
                if (_export_command == null)
                {
                    _export_command = new RelayCommand(x => on_export_click((NVSetting)x), x => true);
                }

                return _export_command;
            }
        }

        private RelayCommand _import_command;
        public ICommand import_command
        {
            get
            {
                if (_import_command == null)
                {
                    _import_command = new RelayCommand(x => on_import_click((NVSetting)x), x => true);
                }

                return _import_command;
            }
        }
        
        private RelayCommand _add_command;
        public ICommand add_command
        {
            get
            {
                if (_add_command == null)
                {
                    _add_command = new RelayCommand(x => on_add_click(), x => true);
                }

                return _add_command;
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

        private RelayCommand _revert_command;
        public ICommand revert_command
        {
            get
            {
                if (_revert_command == null)
                {
                    _revert_command = new RelayCommand(x => on_revert_click(), x => true);
                }

                return _revert_command;
            }
        }

        private RelayCommand _write_nvram_command;
        public ICommand write_nvram_command
        {
            get
            {
                if (_write_nvram_command == null)
                {
                    _write_nvram_command = new RelayCommand(x => on_write_nvram_click(), x => true);
                }

                return _write_nvram_command;
            }
        }
        
        public void reload_settings(NVSetting selected_setting = null)
        {
            this.nvram_dialog.HasChanges = false;

            if (this.nvram_settings != null)
            {
                for (var i = 0; i < this.nvram_settings.nv_setting.Count; i++)
                {
                    if (this.nvram_settings.nv_setting[i].edited
                    || this.nvram_settings.nv_setting[i].removed
                    || this.nvram_settings.nv_setting[i].added)
                    {
                        this.nvram_dialog.HasChanges = true;
                        break;
                    }
                }

                this.nvram_dialog.nvram_settings_list.ItemsSource = null;
                this.nvram_dialog.nvram_settings_list.ItemsSource = this.nvram_settings.nv_setting;

                CollectionViewSource.GetDefaultView(this.nvram_dialog.nvram_settings_list.ItemsSource).Filter = SettingsFilter;

                if (selected_setting != null)
                {
                    this.nvram_dialog.nvram_settings_list.SelectedItem = selected_setting;
                    this.nvram_dialog.nvram_settings_list.ScrollIntoView(this.nvram_dialog.nvram_settings_list.SelectedItem);
                }
            }
        }
        private bool SettingsFilter(object item)
        {
            var nvram_setting = (NVSetting)item;

            return !(nvram_setting == null || nvram_setting.removed);
        }

        public void change_setting(Guid setting_id, byte[] new_value)
        {
            for (var i = 0; i < this.nvram_settings.nv_setting.Count; i++)
            {
                if(this.nvram_settings.nv_setting[i].id == setting_id)
                {
                    this.nvram_settings.nv_setting[i].edited = true;
                    this.nvram_settings.nv_setting[i].value = new NVSettingValue()
                    {
                        stored_value = this.nvram_settings.nv_setting[i].value.stored_value,
                        edited_value = new_value,
                        default_value = this.nvram_settings.nv_setting[i].value.default_value
                    };

                    this.reload_settings();
                    break;
                }
            }
        }

        public void add_setting(NVSetting nvram_setting)
        {
            nvram_setting.added = true;

            this.nvram_settings.nv_setting.Add(nvram_setting);
            this.reload_settings(nvram_setting);
        }

        public void close_window()
        {
            this.nvram_dialog.Close();
        }
        
        public void on_select_this_nvram_click()
        {
            if (this.nvram_selector != null)
            {
                this.nvram_dialog.IsWriting = true;
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                this.nvram_selector.set_selected(this.nvram_dialog.SelectedObjectLocation.type);
                this.nvram_dialog.IsWriting = false;

                this.update_nvram_selection(this.nvram_dialog.SelectedObjectLocation.type);
            }
        }

        public void on_import_box_click()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "Data Files (*.boxnv, *.bin, *.dat, *.img, *.o)|*.boxnv;*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var nvram = new NVRAM(file_dialog.FileName);

                this.nvram_dialog.PendingImport = true;

                process_current_nvram_info(nvram);
            }
        }
        public void on_export_box_click()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "Data Files (*.boxnv, *.bin, *.dat, *.img, *.o)|*.boxnv;*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var nvram = new NVRAM(this.disk.io, this.nvram_dialog.SelectedObjectLocation);

                try
                {
                    File.WriteAllBytes(file_dialog.FileName, nvram.build_nvram(this.nvram_settings));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error building NVRAM. " + ex.Message);
                }
            }
        }


        public void on_import_simnv_click()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "Data Files (*.simnv, *.bin, *.dat, *.img, *.o)|*.simnv;*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var nvram = new NVRAM(file_dialog.FileName);

                this.nvram_dialog.PendingImport = true;

                process_current_nvram_info(nvram, false);
            }
        }
        public void on_export_simnv_click()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "Data Files (*.simnv, *.bin, *.dat, *.img, *.o)|*.simnv;*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var nvram = new NVRAM(this.disk.io, this.nvram_dialog.SelectedObjectLocation);

                try
                {
                    File.WriteAllBytes(file_dialog.FileName, nvram.build_nvram(this.nvram_settings, false));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error building NVRAM. " + ex.Message);
                }
            }
        }


        public void on_import_json_click()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            file_dialog.Title = "Import NVRAM Settings";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                try
                {
                    var json = File.ReadAllText(file_dialog.FileName);

                    this.nvram_settings = JsonConvert.DeserializeObject<NVSettings>(json);

                    this.nvram_dialog.IsImported = true;

                    this.reload_settings();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Error trying to import NVRAM settings! " + e.Message);
                }
            }
        }
        public void on_export_json_click()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            file_dialog.Title = "Export NVRAM Settings";

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
                var json = JsonConvert.SerializeObject(this.nvram_settings, settings);

                File.WriteAllText(file_dialog.FileName, json);
            }
        }

        public void on_copy_click(string nvram_converted_value)
        {
            Clipboard.SetText(nvram_converted_value);
        }

        public void on_edit_click(NVSetting nvram_setting, bool force_hex = false)
        {
            if (!force_hex && nvram_setting.data_editor == NVDataEditor.TELLYSCRIPT_EDITOR)
            {
                var tellyscript_type = TellyScriptType.ORIGINAL;
                if (nvram_setting.name == "DLSC")
                {
                    tellyscript_type = TellyScriptType.DIALSCRIPT;
                }
                else
                {
                    tellyscript_type = TellyScriptType.ORIGINAL;
                }


                TellyScriptEditor tlly_editor = null;
                try
                {
                    tlly_editor = new TellyScriptEditor(nvram_setting.value.edited_value, tellyscript_type);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error unpacking script. Switching to a hex editor. " + ex.Message);
                    this.on_edit_click(nvram_setting, true);
                    return;
                }

                tlly_editor.Owner = this.nvram_dialog;
                tlly_editor.ShowDialog();
                if (tlly_editor.ChangedBytes != null && tlly_editor.ChangedBytes.Length > 0)
                {
                    change_setting(nvram_setting.id, tlly_editor.ChangedBytes);
                }
            }
            else if ((force_hex || nvram_setting.data_editor == NVDataEditor.HEX_EDITOR))
            {
                var hex_editor = new HexEditor(nvram_setting.value.edited_value, false);
                hex_editor.Owner = this.nvram_dialog;
                hex_editor.ShowDialog();

                if (hex_editor.ChangedBytes != null && hex_editor.ChangedBytes.Length > 0)
                {
                    change_setting(nvram_setting.id, hex_editor.ChangedBytes);
                }
            }
            else if (nvram_setting.data_editor == NVDataEditor.FILE_EDITOR)
            {
                this.on_import_click(nvram_setting);
            }
            else if (nvram_setting.data_editor == NVDataEditor.RGB_COLOR_PICKER)
            {
                System.Windows.Forms.ColorDialog color_picker = new System.Windows.Forms.ColorDialog();
                color_picker.AnyColor = true;
                color_picker.FullOpen = true;
                color_picker.AllowFullOpen = true;
                color_picker.Color = System.Drawing.Color.FromArgb(BigEndianConverter.ToInt32(nvram_setting.value.edited_value, 0));

                if (color_picker.ShowDialog() == System.Windows.Forms.DialogResult.OK
                && color_picker.Color != null)
                {
                    var rgb_color = color_picker.Color.ToArgb() & 0x00FFFFFF;

                    change_setting(nvram_setting.id, NVRAMCurrentValueConverter.Bytes(nvram_setting, rgb_color.ToString()));
                }
            }
            else
            {
                var value_editor = new NVValueEditor(nvram_setting);
                value_editor.Owner = this.nvram_dialog;
                value_editor.ShowDialog();

                if (value_editor.ChangedBytes != null && value_editor.ChangedBytes.Length > 0)
                {
                    change_setting(nvram_setting.id, value_editor.ChangedBytes);
                }
            }
        }

        public void on_delete_click(NVSetting nvram_setting)
        {
            for (var i = 0; i < this.nvram_settings.nv_setting.Count; i++)
            {
                if (this.nvram_settings.nv_setting[i].id == nvram_setting.id)
                {
                    this.nvram_settings.nv_setting[i].removed = true;

                    this.reload_settings();
                    break;
                }
            }
        }

        public void on_export_click(NVSetting nvram_setting)
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "Data Files (*.bin, *.dat, *.img, *.o)|*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                File.WriteAllBytes(file_dialog.FileName, nvram_setting.value.edited_value);
            }
        }

        public void on_import_click(NVSetting nvram_setting)
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "Data Files (*.bin, *.dat, *.img, *.o)|*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Import Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var file_data = File.ReadAllBytes(file_dialog.FileName);

                this.change_setting(nvram_setting.id, file_data);
            }
        }

        public void on_revert_click()
        {
            if((bool)this.nvram_dialog.IsImported)
            {
                this.nvram_dialog.IsImported = false;
 
                this.set_current_nvram_info();
            }
            else
            {
                var remove_list = new ObservableCollection<NVSetting>();

                for (var i = 0; i < this.nvram_settings.nv_setting.Count; i++)
                {
                    if (this.nvram_settings.nv_setting[i].added)
                    {
                        remove_list.Add(this.nvram_settings.nv_setting[i]);
                    }
                    else
                    {
                        this.nvram_settings.nv_setting[i].removed = false;
                        this.nvram_settings.nv_setting[i].edited = false;

                        this.nvram_settings.nv_setting[i].value = new NVSettingValue()
                        {
                            stored_value = this.nvram_settings.nv_setting[i].value.stored_value,
                            edited_value = this.nvram_settings.nv_setting[i].value.stored_value,
                            default_value = this.nvram_settings.nv_setting[i].value.default_value
                        };
                    }
                }

                foreach (var nvram_setting in remove_list)
                {
                    this.nvram_settings.nv_setting.Remove(nvram_setting);
                }

                this.reload_settings();
            }
        }

        public void on_add_click()
        {
            var value_editor = new NVValueEditor(null);
            value_editor.Owner = this.nvram_dialog;
            value_editor.ShowDialog();

            if (value_editor.ChangedBytes != null && value_editor.ChangedBytes.Length > 0)
            {
                value_editor.NVRAMSetting.value = new NVSettingValue()
                {
                    stored_value = value_editor.NVRAMSetting.value.stored_value,
                    edited_value = value_editor.ChangedBytes,
                    default_value = value_editor.NVRAMSetting.value.default_value
                };

                add_setting(value_editor.NVRAMSetting.Copy());
            }
        }

        public void on_cancel_click()
        {
            this.close_window();
        }

        public void on_write_nvram_click()
        {
            this.nvram_dialog.SelectedObjectLocation = this.nvram_dialog.nvram_locations.SelectedItem as ObjectLocation;

            if (this.disk != null && this.nvram_dialog.SelectedObjectLocation != null)
            {
                if (!this.loading)
                {
                    var nvram = new NVRAM(this.disk.io, this.nvram_dialog.SelectedObjectLocation);
                    var nvram_settings = this.nvram_settings;

                    process_new_nvram_info(nvram, nvram_settings);
                }
            }
        }

        public void process_new_nvram_info(NVRAM nvram, NVSettings nvram_settings)
        {
            this.loading = true;
            this.nvram_dialog.IsWriting = true;

            if (this.wait_window == null)
            {
                this.wait_window = new WaitMessage("Writing NVRAM Settings...", this.nvram_dialog);
            }

            this.wait_window.Go(() =>
            {
                this.write_nvram_info(nvram, nvram_settings);
            });
        }

        public void write_nvram_info(NVRAM nvram, NVSettings nvram_settings)
        {
            if (this.nvram_dialog != null)
            {
                var error_message = "";

                try
                {
                    nvram.write_nvram_settings(nvram_settings);
                }
                catch (Exception ex)
                {
                    error_message = "Error building NVRAM. " + ex.Message;
                }

                this.finalize_nvram_write(error_message);
            }
        }

        public void finalize_nvram_write(string error_message)
        {
            if (this.nvram_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new error_message_call(finalize_nvram_write);

                this.nvram_dialog.Dispatcher.Invoke(cb, error_message);
            }
            else
            {
                this.close_wait_window();
                this.loading = false;
                this.nvram_dialog.IsWriting = false;
                this.nvram_dialog.IsImported = false;

                if(error_message != "")
                {
                    System.Windows.MessageBox.Show(error_message);
                }
                else
                {
                    this.set_current_nvram_info();
                }
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

        public void process_current_nvram_info(NVRAM nvram, bool big_endian = true)
        {
            this.loading = true;

            if (this.wait_window == null)
            {
                this.wait_window = new WaitMessage("Loading NVRAM Settings...", this.nvram_dialog);
            }

            this.nvram_dialog.nvram_settings_list.ItemsSource = null;

            this.wait_window.Go(() =>
            {
                this.read_nvram_info(nvram, big_endian);
            });
        }

        public void read_nvram_info(NVRAM nvram, bool big_endian = true)
        {
            if (this.nvram_dialog != null)
            {
                NVSettings nvram_settings = null;
                uint nvram_checksum = 0;
                var error_message = "";

                try
                {
                    nvram_settings = nvram.enumerate_nvram_settings(big_endian);
                    nvram_checksum = nvram.calculate_checksum(big_endian);
                }
                catch (Exception ex)
                {
                    error_message = ex.Message;
                }
                finally
                {
                    this.finalize_nvram_read(nvram_settings, nvram_checksum, error_message);
                }
            }
        }

        public void finalize_nvram_read(NVSettings nvram_settings, uint calculated_checksum, string error_message)
        {
            if (this.nvram_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new nvram_info_callback(finalize_nvram_read);

                this.nvram_dialog.Dispatcher.Invoke(cb, nvram_settings, calculated_checksum, error_message);
            }
            else
            {
                this.close_wait_window();
                this.loading = false;

                if (error_message != "")
                {
                    System.Windows.MessageBox.Show(error_message);
                }
                else if(nvram_settings != null)
                {
                    this.nvram_settings = nvram_settings;
                    this.calculated_checksum = calculated_checksum;
                    this.nvram_dialog.IsImported = this.nvram_dialog.PendingImport;
                }

                this.nvram_dialog.PendingImport = false;

                this.draw_nvram_info();
            }
        }

        public void draw_nvram_info()
        {
            if(this.nvram_settings != null)
            {
                this.nvram_dialog.nvram_checksum.Content = "0x" + this.nvram_settings.checksum.ToString("X");
                if (this.nvram_settings.checksum != this.calculated_checksum)
                {
                    ((Label)this.nvram_dialog.FindName("nvram_checksum")).Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                }
                else
                {
                    ((Label)this.nvram_dialog.FindName("nvram_checksum")).Foreground = (Brush)Application.Current.FindResource("TextBrush");
                }
            }

            this.reload_settings();
        }

        public void set_current_nvram_info()
        {
            this.nvram_dialog.SelectedObjectLocation = this.nvram_dialog.nvram_locations.SelectedItem as ObjectLocation;

            if (this.disk != null && this.nvram_dialog.SelectedObjectLocation != null)
            {
                if (!this.loading)
                {
                    var nvram = new NVRAM(this.disk.io, this.nvram_dialog.SelectedObjectLocation);

                    this.process_current_nvram_info(nvram);
                }

            }
        }

        public void set_nvram_locations()
        {
            this.loading = true;

            this.nvram_dialog.NVRAMLocations = new ObjectLocationCollection();

            if (this.disk != null)
            {
                if (this.nvram_selector == null)
                {
                    this.nvram_selector = new ObjectLocationSelected(ObjectLocationCategory.PRIMARY_NVRAM, this.disk);
                }

                this.nvram_dialog.NVRAMLocations.add_enumerated_primary_nvram(this.disk, this.nvram_selector.get_selected());
            }

            if ((bool)this.nvram_dialog.OnlyInfo)
            {
                this.nvram_dialog.NVRAMLocations.Add(new ObjectLocation()
                {
                    type = ObjectLocationType.FILE_LOCATION,
                    offset = 0,
                    size_bytes = 0,
                });
            }

            this.update_nvram_selection(ObjectLocationType.UNKNOWN);

            this.loading = false;

            this.set_current_nvram_info();
        }

        void update_nvram_selection(ObjectLocationType selected_nvram)
        {
            var selected_index = 0;

            if (this.disk != null && this.nvram_selector != null)
            {
                if (selected_nvram == ObjectLocationType.UNKNOWN)
                {
                    selected_nvram = this.nvram_selector.get_selected();
                }

                for (var i = 0; i < this.nvram_dialog.NVRAMLocations.Count; i++)
                {
                    if (this.nvram_dialog.NVRAMLocations[i].type == selected_nvram)
                    {
                        selected_index = i;
                        this.nvram_dialog.NVRAMLocations[i].selected = true;
                    }
                    else
                    {
                        this.nvram_dialog.NVRAMLocations[i].selected = false;
                    }
                }
            }

            this.nvram_dialog.SelectedNVRAM = selected_nvram;
            this.nvram_dialog.nvram_locations.SelectedIndex = selected_index;
            this.nvram_dialog.SelectedObjectLocation = this.nvram_dialog.NVRAMLocations[selected_index];
            RaisePropertyChanged("");
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            this.set_nvram_locations();
        }

        public NVRAMViewModel(NVRAMView nvram_dialog, WebTVDisk disk, bool? only_info)
        {
            this.nvram_dialog = nvram_dialog;
            this.disk = disk;

            this.nvram_dialog.IsWriting = false;
            this.nvram_dialog.OnlyInfo = only_info;

            this.nvram_dialog.SelectedObjectLocation = null;

            this.nvram_dialog.Loaded += window_loaded;
        }
    }
}
