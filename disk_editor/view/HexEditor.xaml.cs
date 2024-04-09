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
using Be.Windows.Forms;

namespace disk_editor
{
    /// <summary>
    /// Interaction logic for DataEditor.xaml
    /// </summary>
    public partial class HexEditor : Fluent.RibbonWindow
    {
        public bool? ReadOnly
        {
            get { return GetValue(ReadOnlyProperty) as bool?; }
            set { SetValue(ReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly",
                                        typeof(bool?),
                                        typeof(HexEditor));

        public byte[] ChangedBytes
        {
            get { return GetValue(ChangedBytesProperty) as byte[]; }
            set { SetValue(ChangedBytesProperty, value); }
        }
        public static readonly DependencyProperty ChangedBytesProperty =
            DependencyProperty.Register("ChangedBytes",
                                        typeof(byte[]),
                                        typeof(HexEditor));

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

        private RelayCommand _export_command;
        public ICommand export_command
        {
            get
            {
                if (_export_command == null)
                {
                    _export_command = new RelayCommand(x => on_export_click(), x => true);
                }

                return _export_command;
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

        public void on_export_click()
        {
            System.Windows.Forms.SaveFileDialog file_dialog = new System.Windows.Forms.SaveFileDialog();
            file_dialog.Filter = "Data Files (*.bin, *.dat, *.img, *.o)|*.bin;*.dat;*.img;*.o|All Files (*.*)|*.*";
            file_dialog.Title = "Export Data";

            if (file_dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK
            && file_dialog.FileName != null && file_dialog.FileName != "")
            {
                var _bytes = new byte[this.HexBox.ByteProvider.Length];
                for (var i = 0; i < this.HexBox.ByteProvider.Length; i++)
                {
                    _bytes[i] = this.HexBox.ByteProvider.ReadByte(i);
                }

                File.WriteAllBytes(file_dialog.FileName, _bytes);
            }
        }

        public void on_cancel_click()
        {
            close_window();
        }

        public void on_save_click()
        {
            if (this.HexBox.ByteProvider.HasChanges())
            {
                this.ChangedBytes = new byte[this.HexBox.ByteProvider.Length];
                for(var i = 0; i < this.HexBox.ByteProvider.Length; i++)
                {
                    this.ChangedBytes[i] = this.HexBox.ByteProvider.ReadByte(i);
                }
            }

            close_window();
        }

        public void close_window()
        {
            this.Close();
        }

        public HexEditor(byte[] bytes, bool read_only = true, long start_offset = 0x00)
        {
            InitializeComponent();

            this.ChangedBytes = null;
            this.HexBox.LineInfoOffset = start_offset;
            this.HexBox.ReadOnly = read_only;
            this.ReadOnly = read_only;
            this.HexBox.ByteProvider = new DynamicByteProvider(bytes);

            this.DataContext = this;
        }
    }
}
