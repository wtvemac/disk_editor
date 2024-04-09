using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Windows;

namespace disk_editor
{
    class MountViewModel : INotifyPropertyChanged
    {
        public MountPartition mount_dialog { get; set; }
        public WebTVPartition part { get; set; }
        public StringCollection available_drive_letters { get; set; }
        public WaitMessage wait_window { get; set; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

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

        private RelayCommand _mount_command;
        public ICommand mount_command
        {
            get
            {
                if (_mount_command == null)
                {
                    _mount_command = new RelayCommand(x => on_mount_click(), x => true);
                }

                return _mount_command;
            }
        }

        public void close_window()
        {
            this.mount_dialog.Close();
        }

        public void on_cancel_click()
        {
            this.close_window();
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

        public void mount_done(bool success = true, string message = "")
        {
            if(this.wait_window != null)
            {
                this.close_wait_window();
            }

            this.close_window();
        }

        public void on_mount_click()
        {
            try
            {
                this.wait_window = new WaitMessage("Mounting Device...");
                wait_window.Owner = this.mount_dialog.Owner;
                this.wait_window.Show();

                this.part.mount(this.mount_dialog.mount_letter.SelectedItem.ToString() + ":", (bool)this.mount_dialog.mount_read_only.IsChecked, this.mount_done);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error! " + e.Message);
            }
        }

        public MountViewModel(MountPartition mount_dialog, WebTVPartition part)
        {
            this.mount_dialog = mount_dialog;
            this.part = part;
            this.available_drive_letters = (new AvailableDriveLetters()).get_available_drive_letters();
        }
    }
}
