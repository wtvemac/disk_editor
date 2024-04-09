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
using System.IO;
using System.ComponentModel;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for ValueEditor.xaml
    /// </summary>
    public partial class NVValueEditor : Fluent.RibbonWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public NVSetting NVRAMSetting
        {
            get { return GetValue(NVRAMSettingProperty) as NVSetting; }
            set { SetValue(NVRAMSettingProperty, value); }
        }
        public static readonly DependencyProperty NVRAMSettingProperty =
            DependencyProperty.Register("NVRAMSetting",
                                        typeof(NVSetting),
                                        typeof(NVValueEditor));

        public List<NVSetting> NVRAMSettings
        {
            get { return GetValue(NVRAMSettingsProperty) as List<NVSetting>; }
            set { SetValue(NVRAMSettingsProperty, value); }
        }
        public static readonly DependencyProperty NVRAMSettingsProperty =
            DependencyProperty.Register("NVRAMsSetting",
                                        typeof(List<NVSetting>),
                                        typeof(NVValueEditor));

        public bool? AddingSetting
        {
            get { return GetValue(AddingSettingProperty) as bool?; }
            set { SetValue(AddingSettingProperty, value); }
        }
        public static readonly DependencyProperty AddingSettingProperty =
            DependencyProperty.Register("AddingSetting",
                                        typeof(bool?),
                                        typeof(NVValueEditor));
        
        public byte[] ChangedBytes
        {
            get { return GetValue(ChangedBytesProperty) as byte[]; }
            set { SetValue(ChangedBytesProperty, value); }
        }
        public static readonly DependencyProperty ChangedBytesProperty =
            DependencyProperty.Register("ChangedBytes",
                                        typeof(byte[]),
                                        typeof(NVValueEditor));


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

        private RelayCommand _save_command;
        public ICommand save_command
        {
            get
            {
                if (_save_command == null)
                {
                    _save_command = new RelayCommand(x => on_save_click(), x => true);
                }

                return _save_command;
            }
        }

        private RelayCommand _import_command;
        public ICommand import_command
        {
            get
            {
                if (_import_command == null)
                {
                    _import_command = new RelayCommand(x => on_import_click(), x => true);
                }

                return _import_command;
            }
        }

        private RelayCommand _hex_edit_command;
        public ICommand hex_edit_command
        {
            get
            {
                if (_hex_edit_command == null)
                {
                    _hex_edit_command = new RelayCommand(x => on_hex_edit_click(), x => true);
                }

                return _hex_edit_command;
            }
        }
        

        private RelayCommand _color_edit_command;
        public ICommand color_edit_command
        {
            get
            {
                if (_color_edit_command == null)
                {
                    _color_edit_command = new RelayCommand(x => on_color_edit_click(), x => true);
                }

                return _color_edit_command;
            }
        }

        private RelayCommand _tellyscript_edit_command;
        public ICommand tellyscript_edit_command
        {
            get
            {
                if (_tellyscript_edit_command == null)
                {
                    _tellyscript_edit_command = new RelayCommand(x => on_tellyscript_edit_click(), x => true);
                }

                return _tellyscript_edit_command;
            }
        }

        private void on_settings_opened(object sender, EventArgs e)
        {
            var nv_setting = this.selected_nvram_setting.SelectedItem as NVSetting;

            this.selected_nvram_setting.SelectedItem = null;

            this.selected_nvram_setting.SelectedItem = nv_setting;
        }

        private void on_nvram_setting_type(object sender, KeyEventArgs e)
        {
            if(this.selected_nvram_setting.Text != "")
            {
                this.on_nvram_setting_change(sender, null);
            }
        }

        private void on_nvram_setting_change(object sender, SelectionChangedEventArgs e)
        {
            var nv_setting = this.selected_nvram_setting.SelectedItem as NVSetting;

            var default_value = new byte[1];

            if (nv_setting == null)
            {
                nv_setting = new NVSetting()
                {
                    name = this.selected_nvram_setting.Text.Trim().ToUpper(),
                    title = "",
                    notes = "",
                    data_type = NVDataType.BINARY_BLOB,
                    data_editor = NVDataEditor.HEX_EDITOR,
                    edited = false,
                    removed = false
                };
            }
            else if(nv_setting.value.default_value != null)
            {
                default_value = nv_setting.value.default_value;
            }

            nv_setting.id = Guid.NewGuid();
            nv_setting.value = new NVSettingValue()
            {
                stored_value = default_value,
                edited_value = default_value,
                default_value = default_value
            };

            this.NVRAMSetting = nv_setting;
            this.prepare_value();
        }

        public void change_setting(byte[] new_value)
        {
            this.NVRAMSetting.value = new NVSettingValue()
            {
                default_value = this.NVRAMSetting.value.default_value,
                stored_value = this.NVRAMSetting.value.stored_value,
                edited_value = new_value
            };

            this.prepare_value();
        }

        public void on_import_click()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "Data Files (*.bin, *.dat, *.img, *.o)|*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Import Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var file_data = File.ReadAllBytes(file_dialog.FileName);

                this.change_setting(file_data);
            }
        }

        public void on_hex_edit_click()
        {
            var hex_editor = new HexEditor(this.NVRAMSetting.value.edited_value, false);
            hex_editor.Owner = this;
            hex_editor.ShowDialog();

            if (hex_editor.ChangedBytes != null && hex_editor.ChangedBytes.Length > 0)
            {
                this.change_setting(hex_editor.ChangedBytes);
            }
        }

        public void on_color_edit_click()
        {
            System.Windows.Forms.ColorDialog color_picker = new System.Windows.Forms.ColorDialog();
            color_picker.AnyColor = true;
            color_picker.FullOpen = true;
            color_picker.AllowFullOpen = true;
            color_picker.Color = System.Drawing.Color.FromArgb(BigEndianConverter.ToInt32(this.NVRAMSetting.value.edited_value, 0));

            if (color_picker.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && color_picker.Color != null)
            {
                var rgb_color = color_picker.Color.ToArgb() & 0x00FFFFFF;

                change_setting(NVRAMCurrentValueConverter.Bytes(this.NVRAMSetting, rgb_color.ToString()));
            }
        }

        public void on_tellyscript_edit_click()
        {
            var tellyscript_type = TellyScriptType.ORIGINAL;
            if (this.NVRAMSetting.name == "DLSC")
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
                tlly_editor = new TellyScriptEditor(this.NVRAMSetting.value.edited_value, tellyscript_type);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error unpacking script. Switching to a hex editor.");
                this.on_hex_edit_click();
                return;
            }

            tlly_editor.Owner = this;
            tlly_editor.ShowDialog();
            if (tlly_editor.ChangedBytes != null && tlly_editor.ChangedBytes.Length > 0)
            {
                change_setting(tlly_editor.ChangedBytes);
            }
        }

        public void on_cancel_click()
        {
            this.close_window();
        }

        public void on_save_click()
        {
            if (this.NVRAMSetting.data_editor == NVDataEditor.HEX_EDITOR
            || this.NVRAMSetting.data_editor == NVDataEditor.FILE_EDITOR
            || this.NVRAMSetting.data_editor == NVDataEditor.TELLYSCRIPT_EDITOR)
            {
                this.ChangedBytes = this.NVRAMSetting.value.edited_value;
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.RGB_COLOR_PICKER)
            {
                if(this.NVRAMSetting.value.edited_value.Length < 4)
                {
                    this.NVRAMSetting.value = new NVSettingValue()
                    {
                        edited_value = new byte[4],
                        stored_value = this.NVRAMSetting.value.stored_value,
                        default_value = this.NVRAMSetting.value.default_value
                    };
                }

                this.ChangedBytes = this.NVRAMSetting.value.edited_value;
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.STRING_EDITOR
                || this.NVRAMSetting.data_editor == NVDataEditor.IP_EDITOR)
            {
                this.ChangedBytes = NVRAMCurrentValueConverter.Bytes(this.NVRAMSetting, this.string_input.Text);
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.INTEGER_EDITOR)
            {
                this.ChangedBytes = NVRAMCurrentValueConverter.Bytes(this.NVRAMSetting, this.number_input.Value.ToString());
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.BOOLEAN_EDITOR)
            {
                var value = new byte[1];

                if ((bool)this.boolean_input.IsChecked)
                {
                    value[0] = 0x01;
                }
                else
                {
                    value[0] = 0x00;
                }

                this.ChangedBytes = value;
            }

            this.close_window();
        }

        public void close_window()
        {
            this.Close();
        }

        public void prepare_value()
        {
            var editable_value = NVRAMCurrentValueConverter.String(this.NVRAMSetting);

            if (this.NVRAMSetting.data_editor == NVDataEditor.STRING_EDITOR
            || this.NVRAMSetting.data_editor == NVDataEditor.IP_EDITOR)
            {
                this.string_input.Text = editable_value;
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.INTEGER_EDITOR)
            {
                this.number_input.Text = editable_value;

                if (this.NVRAMSetting.data_type == NVDataType.UINT16)
                {
                    this.number_input.Minimum = 0;
                    this.number_input.Maximum = 0xFFFF;
                }
                else if (this.NVRAMSetting.data_type == NVDataType.INT16)
                {
                    this.number_input.Minimum = -1 * 0x7FFF;
                    this.number_input.Maximum = 0x7FFF;
                }
                else if (this.NVRAMSetting.data_type == NVDataType.UINT32)
                {
                    this.number_input.Minimum = 0;
                    this.number_input.Maximum = 0xFFFFFFFF;
                }
                else if (this.NVRAMSetting.data_type == NVDataType.INT32)
                {
                    this.number_input.Minimum = -1 * 0x7FFFFFFF;
                    this.number_input.Maximum = 0x7FFFFFFF;
                }
                else if (this.NVRAMSetting.data_type == NVDataType.UINT64)
                {
                    this.number_input.Minimum = 0;
                    this.number_input.Maximum = 0x7FFFFFFFFFFFFFFF;
                }
                else if (this.NVRAMSetting.data_type == NVDataType.INT64)
                {
                    this.number_input.Minimum = -1 * 0x7FFFFFFFFFFFFFFF;
                    this.number_input.Maximum = 0x7FFFFFFFFFFFFFFF;
                }

            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.BOOLEAN_EDITOR)
            {
                if (this.NVRAMSetting.value.edited_value != null && this.NVRAMSetting.value.edited_value.Length > 0)
                {
                    if (this.NVRAMSetting.value.edited_value[0] == 0x01)
                    {
                        this.boolean_input.IsChecked = true;
                    }
                    else
                    {
                        this.boolean_input.IsChecked = false;
                    }
                }
                else
                {
                    this.boolean_input.IsChecked = false;
                }
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.HEX_EDITOR
                 || this.NVRAMSetting.data_editor == NVDataEditor.FILE_EDITOR
                 || this.NVRAMSetting.data_editor == NVDataEditor.TELLYSCRIPT_EDITOR)
            {
                this.data_size.Content = this.NVRAMSetting.value.edited_value.Length;
            }
            else if (this.NVRAMSetting.data_editor == NVDataEditor.RGB_COLOR_PICKER)
            {
                if(this.NVRAMSetting.value.edited_value.Length > 3)
                {
                    this.picked_color.Background = new SolidColorBrush(Color.FromRgb(this.NVRAMSetting.value.edited_value[1], this.NVRAMSetting.value.edited_value[2], this.NVRAMSetting.value.edited_value[3]));
                }
                else
                {
                    this.picked_color.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                }
            }
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            if((bool)this.AddingSetting)
            {
                this.NVRAMSettings = new List<NVSetting>();

                var nvram_setting_keys = NVDefaults.defaults.Keys.ToArray();
                Array.Sort(nvram_setting_keys);
                for (var i = 0; i < nvram_setting_keys.Length; i++)
                {
                    this.NVRAMSettings.Add(NVDefaults.defaults[nvram_setting_keys[i]]);
                }

                RaisePropertyChanged("NVRAMSettings");
            }
            else
            {
                this.prepare_value();
            }
        }

        public NVValueEditor(NVSetting nvram_setting)
        {
            InitializeComponent();

            if (nvram_setting == null)
            {
                this.AddingSetting = true;
                this.Height = 150;
                this.Title = "New NVRAM Setting";
            }
            else
            {
                this.AddingSetting = false;
                this.Height = 110;
                this.Title = "NVRAM Setting Value: " + nvram_setting.name;
            }

            this.NVRAMSetting = nvram_setting;

            this.ChangedBytes = null;

            this.DataContext = this;

            this.Loaded += window_loaded;
        }
    }
}
