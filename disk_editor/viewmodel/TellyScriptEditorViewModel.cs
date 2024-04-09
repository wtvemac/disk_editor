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

namespace disk_editor
{
    internal class TellyScriptEditorViewModel : INotifyPropertyChanged
    {
        public TellyScriptEditor editor_dialog { get; set; }
        public TellyScriptType tellyscript_type { get; set; }
        public TellyScript tellyscript { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private RelayCommand _export_tokenized_command;
        public ICommand export_tokenized_command
        {
            get
            {
                if (_export_tokenized_command == null)
                {
                    _export_tokenized_command = new RelayCommand(x => on_export_tokenized_click(), x => true);
                }

                return _export_tokenized_command;
            }
        }

        private RelayCommand _import_tokenized_command;
        public ICommand import_tokenized_command
        {
            get
            {
                if (_import_tokenized_command == null)
                {
                    _import_tokenized_command = new RelayCommand(x => on_import_tokenized_click(), x => true);
                }

                return _import_tokenized_command;
            }
        }

        private RelayCommand _export_script_command;
        public ICommand export_script_command
        {
            get
            {
                if (_export_script_command == null)
                {
                    _export_script_command = new RelayCommand(x => on_export_script_click(), x => true);
                }

                return _export_script_command;
            }
        }

        private RelayCommand _import_script_command;
        public ICommand import_script_command
        {
            get
            {
                if (_import_script_command == null)
                {
                    _import_script_command = new RelayCommand(x => on_import_script_click(), x => true);
                }

                return _import_script_command;
            }
        }

        private RelayCommand _copy_all_command;
        public ICommand copy_all_command
        {
            get
            {
                if (_copy_all_command == null)
                {
                    _copy_all_command = new RelayCommand(x => on_copy_all_click(), x => true);
                }

                return _copy_all_command;
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

        public void on_export_tokenized_click()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "Data Files (*.tok, *.dat, *.bin, *.img)|*.tok;*.dat;*.bin;*.img|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                try
                {
                    if (this.editor_dialog.tellyscript_source.Text != this.tellyscript.raw_data)
                    {
                        this.tellyscript.process(this.editor_dialog.tellyscript_source.Text, TellyScriptState.RAW);
                    }

                    File.WriteAllBytes(file_dialog.FileName, this.tellyscript.tokenized_data);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error exporting! " + ex.Message);
                }
            }
        }

        public void on_import_tokenized_click()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "Data Files (*.tok, *.dat, *.bin, *.img)|*.tok;*.dat;*.bin;*.img|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                try
                {
                    var tokenzed_data = File.ReadAllBytes(file_dialog.FileName);

                    this.tellyscript.process(tokenzed_data, TellyScriptState.UNKNOWN);
                    this.sync_editor();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error importing! " + ex.Message);
                }
            }
        }

        public void on_export_script_click()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "TellyScript Files (*.scr, *.ts, *.tsf, *.txt)|*.scr;*.ts;*.tsf;*.txt|All Files (*.*)|*.*";
            file_dialog.Title = "Export Script";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                File.WriteAllText(file_dialog.FileName, this.tellyscript.raw_data);
            }
        }

        public void on_import_script_click()
        {
            System.Windows.Forms.OpenFileDialog file_dialog = new System.Windows.Forms.OpenFileDialog();
            file_dialog.Filter = "TellyScript Files (*.scr, *.ts, *.tsf, *.txt)|*.scr;*.ts;*.tsf;*.txt|All Files (*.*)|*.*";
            file_dialog.Title = "Import Script";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                try
                {
                    var raw_data = File.ReadAllText(file_dialog.FileName);

                    this.tellyscript.process(raw_data, TellyScriptState.RAW);
                    this.sync_editor();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error importing! " + ex.Message);
                }
            }
        }

        public void on_copy_all_click()
        {
            Clipboard.SetText(this.editor_dialog.tellyscript_source.Text);
        }

        public void on_cancel_click()
        {
            this.close_window();
        }

        public void on_save_click()
        {
            this.tellyscript.process(this.editor_dialog.tellyscript_source.Text, TellyScriptState.RAW);
            this.editor_dialog.ChangedBytes = this.tellyscript.packed_data;

            this.sync_editor();

            this.close_window();
        }

        public void sync_editor()
        {
            if (this.tellyscript.tellyscript_type == TellyScriptType.DIALSCRIPT)
            {
                this.editor_dialog.Title = "DialScript";
            }
            else
            {
                this.editor_dialog.Title = "TellyScript";
            }

            DateTime script_mod_time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            script_mod_time = script_mod_time.AddSeconds(this.tellyscript.packed_header.script_mod).ToLocalTime();

            this.editor_dialog.tellyscript_tagline.Content =
                this.tellyscript.packed_header.magic + " " +
                "v" + this.tellyscript.packed_header.version_major.ToString() + "." + this.tellyscript.packed_header.version_minor.ToString() +
                // using (int) to allow negitive values since that what's used with the wtv-script-id: network header.
                " with an id of " + (int)this.tellyscript.packed_header.script_id + "" +
                ", modified @ " + script_mod_time.ToString() + " and a " +
                "0x" + this.tellyscript.packed_header.decompressed_checksum.ToString("X") +
                ((this.tellyscript.packed_header.decompressed_checksum != this.tellyscript.packed_header.actual_decompressed_checksum) ? " (incorrect)" : "") +
                " checksum";

            this.editor_dialog.tellyscript_source.Text = this.tellyscript.raw_data;
        }

        public void close_window()
        {
            this.editor_dialog.Close();
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            this.sync_editor();
        }

        public TellyScriptEditorViewModel(TellyScriptEditor editor_dialog, byte[] tellyscript_data, TellyScriptType tellyscript_type = TellyScriptType.ORIGINAL)
        {
            this.editor_dialog = editor_dialog;

            this.tellyscript_type = tellyscript_type;
            if(tellyscript_data == null || tellyscript_data.Length < 0x24)
            {
                this.tellyscript = new TellyScript("", TellyScriptState.RAW, tellyscript_type);
            }
            else
            {
                this.tellyscript = new TellyScript(tellyscript_data, TellyScriptState.PACKED, tellyscript_type);
            }

            this.editor_dialog.Loaded += window_loaded;

            this.editor_dialog.ChangedBytes = null;
        }

    }
}
