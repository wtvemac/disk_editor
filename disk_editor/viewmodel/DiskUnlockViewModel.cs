using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Net;

namespace disk_editor
{
    public enum DiskUnlockMethod
    {
        WEBTV_USER_PASSWORD = 0,
        WEBTV_MASTER_PASSWORD = 1,
        CUSTOM_PASSWORD = 2,
        WEBTV_SECURITY_ERASE = 3,
        CUSTOM_SECURITY_ERASE = 4,
        WEBTV_USER_PASSWORD_LOCK = 5,
        CUSTOM_PASSWORD_LOCK = 6
    };

    class DiskUnlockViewModel : INotifyPropertyChanged
    {
        public struct ATA_CREDENTIALS
        {
            public bool master_unlock;
            public byte[] password;
        }

        private delegate void void_call();

        DiskUnlock unlock_disk_dialog;
        AddDiskViewModel add_disk_viewmodel;
        bool lock_mode;
        DiskWMIEntry diskWMI;
        WebTVDisk disk;
        string error_message;
        GenericValueSettings ssid_list = null;
        GenericValueSettings disk_random_list = null;
        GenericValueSettings disk_password_list = null;

        private WaitMessage wait_window;

        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool sent_disk_command;

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

        private RelayCommand _erase_disk_command;
        public ICommand erase_disk_command
        {
            get
            {
                if (_erase_disk_command == null)
                {
                    _erase_disk_command = new RelayCommand(x => on_erase_disk_click(), x => true);
                }

                return _erase_disk_command;
            }
        }

        private RelayCommand _unlock_and_disable_disk_command;
        public ICommand unlock_and_disable_disk_command
        {
            get
            {
                if (_unlock_and_disable_disk_command == null)
                {
                    _unlock_and_disable_disk_command = new RelayCommand(x => on_unlock_and_disable_disk_click(), x => true);
                }

                return _unlock_and_disable_disk_command;
            }
        }

        private RelayCommand _unlock_disk_command;
        public ICommand unlock_disk_command
        {
            get
            {
                if (_unlock_disk_command == null)
                {
                    _unlock_disk_command = new RelayCommand(x => on_unlock_disk_click(), x => true);
                }

                return _unlock_disk_command;
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
        public void close_window()
        {
            if (this.unlock_disk_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new void_call(this.close_window);

                this.unlock_disk_dialog.Dispatcher.Invoke(cb);
            }
            else
            {
                this.unlock_disk_dialog.Close();
            }
        }

        public void on_cancel_click()
        {
            this.close_window();
        }

        public ATA_CREDENTIALS get_credentials(DiskUnlockMethod? selected_method = null)
        {
            if(selected_method == null)
            {
                selected_method = (DiskUnlockMethod)((GenericListItem)this.unlock_disk_dialog.unlock_method.SelectedItem).Value;
            }

            ATA_CREDENTIALS credentials;
            credentials.master_unlock = false;

            byte[] password_bytes;
            bool is_custom = false;

            if (selected_method == DiskUnlockMethod.WEBTV_USER_PASSWORD || selected_method == DiskUnlockMethod.WEBTV_USER_PASSWORD_LOCK)
            {
                this.unlock_disk_dialog.ssid.Text = this.fix_hex_string(this.unlock_disk_dialog.ssid.Text, 16);
                this.unlock_disk_dialog.nvram_36.Text = this.fix_hex_string(this.unlock_disk_dialog.nvram_36.Text, 16);

                var ssid_bytes = this.hex_string_to_bytes(this.unlock_disk_dialog.ssid.Text);
                var nvram_36_bytes = this.hex_string_to_bytes(this.unlock_disk_dialog.nvram_36.Text);

                Array.Reverse(ssid_bytes);
                Array.Reverse(nvram_36_bytes);

                uint[] SSID =
                {
                    BitConverter.ToUInt32(ssid_bytes, 4),
                    BitConverter.ToUInt32(ssid_bytes, 0)
                };

                uint[] nvram_36 =
                {
                    BitConverter.ToUInt32(nvram_36_bytes, 4),
                    BitConverter.ToUInt32(nvram_36_bytes, 0)
                };

                password_bytes = this.get_user_ata_password(SSID, nvram_36);
                credentials.master_unlock = false;
            }
            else if (selected_method == DiskUnlockMethod.CUSTOM_PASSWORD || selected_method == DiskUnlockMethod.CUSTOM_PASSWORD_LOCK || selected_method == DiskUnlockMethod.CUSTOM_SECURITY_ERASE)
            {
                this.unlock_disk_dialog.password.Text = this.fix_hex_string(this.unlock_disk_dialog.password.Text);

                password_bytes = this.hex_string_to_bytes(this.unlock_disk_dialog.password.Text);
                credentials.master_unlock = (bool)this.unlock_disk_dialog.master_unlock.IsChecked;
                is_custom = true;
            }
            else if (selected_method == DiskUnlockMethod.WEBTV_MASTER_PASSWORD || selected_method == DiskUnlockMethod.WEBTV_SECURITY_ERASE)
            {
                password_bytes = this.get_master_ata_password();
                credentials.master_unlock = true;
            }
            else
            {
                password_bytes = new byte[1];
                is_custom = true;
            }

            credentials.password = new byte[32];
            for (int i = 0; i < Math.Min(password_bytes.Length, 32); i++)
            {
                credentials.password[i] = password_bytes[i];
            }

            if (!is_custom)
            {
                var converter = new WebTVDiskCollationConverter();
                converter.convert_bytes(ref credentials.password, (ulong)0, (uint)credentials.password.Length, DiskByteTransform.BIT16SWAP);
            }

            return credentials;
        }

        public void unlock_disk(bool disable_security)
        {
            var unlock_credentials = get_credentials();

            this.loading = true;
            this.sent_disk_command = true;
            this.error_message = "";

            this.wait_window = new WaitMessage("Attempting Disk Unlock...");
            this.wait_window.Owner = this.unlock_disk_dialog;
            this.wait_window.Go(() =>
            {
                try
                {
                    this.disk.io.security_unlock(unlock_credentials.master_unlock, unlock_credentials.password);
                    this.diskWMI.security_status = this.disk.io.get_security_status();

                    if (disable_security && ((this.diskWMI.security_status & DiskSecurityStatus.LOCKED) != DiskSecurityStatus.LOCKED))
                    {
                        this.disk.io.security_remove(unlock_credentials.master_unlock, unlock_credentials.password);
                    }
                }
                catch (Exception ex)
                {
                    this.error_message = "Failed! " + ex.Message;
                }
                finally
                {
                    this.done_waiting();
                }
            }, false);
        }

        public void lock_disk()
        {
            var lock_credentials = get_credentials();
            var master_credentials = get_credentials(DiskUnlockMethod.WEBTV_MASTER_PASSWORD);

            this.loading = true;
            this.sent_disk_command = true;
            this.error_message = "";

            this.wait_window = new WaitMessage("Attempting Disk Lock...");
            this.wait_window.Owner = this.unlock_disk_dialog;
            this.wait_window.Go(() =>
            {
                try
                {
                    this.disk.io.security_lock(lock_credentials.master_unlock, lock_credentials.password);
                    this.disk.io.security_lock(master_credentials.master_unlock, master_credentials.password);
                    this.diskWMI.security_status = this.disk.io.get_security_status();
                }
                catch (Exception ex)
                {
                    this.error_message = "Failed! " + ex.Message;
                }
                finally
                {
                    this.done_waiting();
                }
            }, false);
        }

        public void erase_disk()
        {
            var unlock_credentials = get_credentials();

            this.loading = true;
            this.sent_disk_command = true;
            this.error_message = "";

            this.wait_window = new WaitMessage("!! Attempting Disk Erase !!");
            this.wait_window.Owner = this.unlock_disk_dialog;
            this.wait_window.Go(() =>
            {
                try
                {
                    this.disk.io.security_erase_prepare();
                    this.disk.io.security_erase(unlock_credentials.master_unlock, unlock_credentials.password);

                    this.diskWMI.security_status = this.disk.io.get_security_status();
                }
                catch (Exception ex)
                {
                    this.error_message = "Failed! " + ex.Message;
                }
                finally
                {
                    this.done_waiting();
                }
            }, false, false, true);
        }
        public void on_erase_disk_click()
        {
            this.unlock_disk_dialog.error_message.Visibility = Visibility.Collapsed;

            this.erase_disk();
        }

        public void finish_unlock_disk()
        {
            if ((this.diskWMI.security_status & DiskSecurityStatus.LOCKED) != DiskSecurityStatus.LOCKED)
            {
                this.unlock_disk_dialog.error_message.Visibility = Visibility.Collapsed;
                this.save_values();
                this.close_window();
                this.add_disk_viewmodel.add_disk(this.diskWMI);
            }
            else
            {
                if(this.error_message == "")
                {
                    this.error_message = "-- Failed! --";
                }

                this.unlock_disk_dialog.error_message.Visibility = Visibility.Visible;
                this.unlock_disk_dialog.error_message.Text = this.error_message;
            }
        }

        public void finish_lock_disk()
        {
            if (this.error_message != "")
            {
                this.error_message = "-- Failed! --";
                this.unlock_disk_dialog.error_message.Visibility = Visibility.Visible;
                this.unlock_disk_dialog.error_message.Text = this.error_message;
            }
            else
            {
                System.Windows.MessageBox.Show("The password should be enabled. The drive needs to be power cycled for the drive to be fully locked.");
                this.save_values();
                this.close_window();
                this.add_disk_viewmodel.on_refresh_click();
            }
        }

        public void save_values(DiskUnlockMethod? selected_method = null)
        {
            if (selected_method == null)
            {
                selected_method = (DiskUnlockMethod)((GenericListItem)this.unlock_disk_dialog.unlock_method.SelectedItem).Value;
            }

            if (selected_method == DiskUnlockMethod.WEBTV_USER_PASSWORD || selected_method == DiskUnlockMethod.WEBTV_USER_PASSWORD_LOCK)
            {
                this.ssid_list.add_value(this.unlock_disk_dialog.ssid.Text);
                this.disk_random_list.add_value(this.unlock_disk_dialog.nvram_36.Text);

                JSONSettings.set("ssid_list", this.ssid_list.settings);
                JSONSettings.set("disk_random_list", this.disk_random_list.settings);
            }
            else if (selected_method == DiskUnlockMethod.CUSTOM_PASSWORD || selected_method == DiskUnlockMethod.CUSTOM_PASSWORD_LOCK || selected_method == DiskUnlockMethod.CUSTOM_SECURITY_ERASE)
            {
                this.disk_password_list.add_value(this.unlock_disk_dialog.password.Text);

                JSONSettings.set("disk_password_list", this.disk_password_list.settings);
            }
        }

        public void on_unlock_and_disable_disk_click()
        {
            this.unlock_disk_dialog.error_message.Visibility = Visibility.Collapsed;

            this.unlock_disk(true);
        }

        public void on_unlock_disk_click()
        {
            this.unlock_disk_dialog.error_message.Visibility = Visibility.Collapsed;

            this.unlock_disk(false);
        }

        public void on_lock_disk_command()
        {
            this.unlock_disk_dialog.error_message.Visibility = Visibility.Collapsed;

            this.lock_disk();
        }


        /*
         * Compute Psudo-DES Random
         * nvram_36 comes from IIC NVRAM 0x36 XORd with the SSID.  It's stored at 0x8001B3C0 on the LC2 16467 int debug build.
         * 
         * ASM:
         * 
            ROM:8054C320 GetPDESRandom:                           # CODE XREF: GetDiskKey:loc_8054C4C8↓p
            ROM:8054C320                                          # ROM:loc_8054C784↓p
            ROM:8054C320                 move    $t4, $zero
            ROM:8054C324                 lui     $a2, 0x8002
            ROM:8054C328                 addiu   $t2, $a2, -0x4C40
            ROM:8054C32C
            ROM:8054C32C loc_8054C32C:                            # CODE XREF: GetPDESRandom+7C↓j
            ROM:8054C32C                 la      $v0, unk_80703B20
            ROM:8054C334                 sll     $a3, $t4, 2
            ROM:8054C338                 addu    $v0, $a3, $v0
            ROM:8054C33C                 lw      $t0, 4($t2)
            ROM:8054C340                 lw      $v0, 0($v0)
            ROM:8054C344                 xor     $t3, $t0, $v0
            ROM:8054C348                 andi    $a0, $t3, 0xFFFF
            ROM:8054C34C                 mul     $v1, $a0, $a0
            ROM:8054C350                 srl     $a1, $t3, 16
            ROM:8054C354                 mul     $v0, $a1, $a1
            ROM:8054C358                 addiu   $t4, 1
            ROM:8054C35C                 lw      $t1, -0x4C40($a2)
            ROM:8054C360                 mul     $a0, $a1
            ROM:8054C364                 sw      $t0, -0x4C40($a2)
            ROM:8054C368                 nor     $v0, $zero, $v0
            ROM:8054C36C                 addu    $v1, $v0
            ROM:8054C370                 sll     $t3, $v1, 16
            ROM:8054C374                 srl     $v1, 16
            ROM:8054C378                 la      $v0, unk_80703B30
            ROM:8054C380                 addu    $a3, $v0
            ROM:8054C384                 lw      $v0, 0($a3)
            ROM:8054C388                 or      $t3, $v1
            ROM:8054C38C                 xor     $v0, $t3, $v0
            ROM:8054C390                 addu    $v0, $a0
            ROM:8054C394                 xor     $v1, $t1, $v0
            ROM:8054C398                 sltiu   $v0, $t4, 4
            ROM:8054C39C                 bnez    $v0, loc_8054C32C
            ROM:8054C3A0                 sw      $v1, 4($t2)
            ROM:8054C3A4                 jr      $ra
            ROM:8054C3A8                 move    $v0, $v1
            ROM:8054C3A8  # End of function GetPDESRandom
         * 
         */
        public uint GetPDESRandom(uint[] nvram_36)
        {
            uint[] xor_group1 = { 0xBAA96887, 0x1E17D32C, 0x03BCDC3C, 0x0F33D1B2 }; // unk_80703B20
            uint[] xor_group2 = { 0x4B0F3B58, 0xE874F0C3, 0x6955C5A6, 0x55A7CA46 }; // unk_80703B30

            for (int t4 = 0; t4 < 4; t4++)
            {
                uint a = (nvram_36[1] ^ xor_group1[t4]);
                var a1 = (ushort)(a & 0xFFFF);
                var a2 = (ushort)(a >> 16);

                var b = (uint)((a1 * a1) + ~(a2 * a2)); // ~ = nor $v0, $zero, $v0
                uint b_swapped = (b << 16) | (b >> 16);

                uint nvram_0 = nvram_36[0];

                // Swap DWORDs
                nvram_36[0] = nvram_36[1];
                nvram_36[1] = nvram_0 ^ (uint)((b_swapped ^ xor_group2[t4]) + (a1 * a2));
            }

            return nvram_36[1];
        }

        public byte[] get_master_ata_password()
        {
            // "WebTV Networks Inc."
            byte[] ata_password =
            {
                0x57, // W
                0x65, // e
                0x62, // b
                0x54, // T
                0x56, // V
                0x20, //  
                0x4E, // N
                0x65, // e
                0x74, // t
                0x77, // w
                0x6F, // o
                0x72, // r
                0x6B, // k
                0x73, // s
                0x20, //  
                0x49, // I
                0x6E, // n
                0x63, // c
                0x2E  // .
            };

            return ata_password;
        }

        public byte[] get_user_ata_password(uint[] SSID, uint[] nvram_36)
        {
            nvram_36[0] ^= SSID[0];
            nvram_36[1] ^= SSID[1];

            uint[] ata_password_chunks =
            {
                0x00000000,
                0x00000000,
                0x00000000,
                0x00000000,
                0x00000000,
                0x00000000,
                0x00000000,
                0x00000000
            };

            int s3 = 0;

            for (; s3 < 8; s3++)
            {
                var random = GetPDESRandom(nvram_36);

                ata_password_chunks[s3] = ((random << 16) & 0xFFFF0000) | (random >> 16);
            }

            byte[] ata_password =
            {
                (byte)((ata_password_chunks[0] >> 24) & 0xFF),
                (byte)((ata_password_chunks[0] >> 16) & 0xFF),
                (byte)((ata_password_chunks[0] >> 08) & 0xFF),
                (byte)((ata_password_chunks[0])       & 0xFF),
                (byte)((ata_password_chunks[1] >> 24) & 0xFF),
                (byte)((ata_password_chunks[1] >> 16) & 0xFF),
                (byte)((ata_password_chunks[1] >> 08) & 0xFF),
                (byte)((ata_password_chunks[1])       & 0xFF),
                (byte)((ata_password_chunks[2] >> 24) & 0xFF),
                (byte)((ata_password_chunks[2] >> 16) & 0xFF),
                (byte)((ata_password_chunks[2] >> 08) & 0xFF),
                (byte)((ata_password_chunks[2])       & 0xFF),
                (byte)((ata_password_chunks[3] >> 24) & 0xFF),
                (byte)((ata_password_chunks[3] >> 16) & 0xFF),
                (byte)((ata_password_chunks[3] >> 08) & 0xFF),
                (byte)((ata_password_chunks[3])       & 0xFF),
                (byte)((ata_password_chunks[4] >> 24) & 0xFF),
                (byte)((ata_password_chunks[4] >> 16) & 0xFF),
                (byte)((ata_password_chunks[4] >> 08) & 0xFF),
                (byte)((ata_password_chunks[4])       & 0xFF),
                (byte)((ata_password_chunks[5] >> 24) & 0xFF),
                (byte)((ata_password_chunks[5] >> 16) & 0xFF),
                (byte)((ata_password_chunks[5] >> 08) & 0xFF),
                (byte)((ata_password_chunks[5])       & 0xFF),
                (byte)((ata_password_chunks[6] >> 24) & 0xFF),
                (byte)((ata_password_chunks[6] >> 16) & 0xFF),
                (byte)((ata_password_chunks[6] >> 08) & 0xFF),
                (byte)((ata_password_chunks[6])       & 0xFF),
                (byte)((ata_password_chunks[7] >> 24) & 0xFF),
                (byte)((ata_password_chunks[7] >> 16) & 0xFF),
                (byte)((ata_password_chunks[7] >> 08) & 0xFF),
                (byte)((ata_password_chunks[7])       & 0xFF),
            };

            return ata_password;
        }

        public string fix_hex_string(string hex_string, int length = 0)
        {
            string new_hex_string = "";

            new_hex_string = Regex.Replace(hex_string, @"[^a-fA-F0-9]", "");

            if(length > 0)
            {
                if(new_hex_string.Length < length)
                {
                    new_hex_string = new_hex_string.PadRight(length, '0');
                }
                else if(new_hex_string.Length > length)
                {
                    new_hex_string = new_hex_string.Substring(0, length);
                }
            }

            return new_hex_string;
        }

        public byte[] hex_string_to_bytes(string hex_string)
        {
            hex_string = this.fix_hex_string(hex_string, hex_string.Length + (hex_string.Length % 2));

            if (hex_string.Length == 0)
            {
                hex_string = "00";
            }

            byte[] bytes = new byte[hex_string.Length / 2];

            for(int i = 0; i < hex_string.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex_string.Substring(i, 2), 16);
            }

            return bytes;
        }

        public GenericListItem new_disk_unlock_method(DiskUnlockMethod unlock_mthod, string description)
        {
            var disk_unlock_method = new GenericListItem()
            {
                Value = unlock_mthod,
                Text = description
            };

            return disk_unlock_method;
        }

        public void display_security_status()
        {
            this.unlock_disk_dialog.security_enabled.Content = "Yes";
            this.unlock_disk_dialog.security_enabled.Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");

            this.diskWMI.security_status |= DiskSecurityStatus.SUPPORTED;

            if ((this.diskWMI.security_status & DiskSecurityStatus.SUPPORTED) == DiskSecurityStatus.SUPPORTED)
            {
                if ((this.diskWMI.security_status & DiskSecurityStatus.ENABLED) != DiskSecurityStatus.ENABLED)
                {
                    this.unlock_disk_dialog.security_enabled.Content = "No";
                    this.unlock_disk_dialog.security_enabled.Foreground = (Brush)Application.Current.FindResource("TextBrush");
                    this.unlock_disk_dialog.security_enabled.FontWeight = FontWeights.UltraBold;
                }
                else
                {
                    if ((this.diskWMI.security_status & DiskSecurityStatus.LOCKED) != DiskSecurityStatus.LOCKED)
                    {
                        this.unlock_disk_dialog.security_enabled.Content = "No";
                        this.unlock_disk_dialog.security_enabled.Foreground = (Brush)Application.Current.FindResource("TextBrush");
                        this.unlock_disk_dialog.security_enabled.FontWeight = FontWeights.UltraBold;
                    }
                }

                if ((this.diskWMI.security_status & DiskSecurityStatus.FROZEN) == DiskSecurityStatus.FROZEN)
                {
                    this.unlock_disk_dialog.security_frozen.Content = "Yes (CAN'T UNLOCK)";
                    this.unlock_disk_dialog.security_frozen.Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                    this.unlock_disk_dialog.security_frozen.FontWeight = FontWeights.UltraBold;
                }
                else
                {
                    this.unlock_disk_dialog.security_frozen.Content = "No";
                    this.unlock_disk_dialog.security_frozen.Foreground = (Brush)Application.Current.FindResource("TextBrush");
                    this.unlock_disk_dialog.security_frozen.FontWeight = FontWeights.Normal;
                }

                if ((this.diskWMI.security_status & DiskSecurityStatus.ATTEMPTS_EXCEEDED) == DiskSecurityStatus.ATTEMPTS_EXCEEDED)
                {
                    this.unlock_disk_dialog.security_tries_exceeded.Content = "Yes";
                    this.unlock_disk_dialog.security_tries_exceeded.Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                    this.unlock_disk_dialog.security_tries_exceeded.FontWeight = FontWeights.UltraBold;
                }
                else
                {
                    this.unlock_disk_dialog.security_tries_exceeded.Content = "No";
                    this.unlock_disk_dialog.security_tries_exceeded.Foreground = (Brush)Application.Current.FindResource("TextBrush");
                    this.unlock_disk_dialog.security_tries_exceeded.FontWeight = FontWeights.Normal;
                }

                if ((this.diskWMI.security_status & DiskSecurityStatus.HAS_ENHANCED_ERASE) == DiskSecurityStatus.HAS_ENHANCED_ERASE)
                {
                    this.unlock_disk_dialog.security_enhanced_erase.Content = "Yes";
                }
                else
                {
                    this.unlock_disk_dialog.security_enhanced_erase.Content = "No";
                }

                if ((this.diskWMI.security_status & DiskSecurityStatus.SECURITY_MAXIMUM) == DiskSecurityStatus.SECURITY_MAXIMUM)
                {
                    this.unlock_disk_dialog.security_level.Content = "Maximum Security";
                    this.unlock_disk_dialog.security_level.Foreground = (Brush)Application.Current.FindResource("TextBrush");
                    this.unlock_disk_dialog.security_level.FontWeight = FontWeights.UltraBold;
                }
                else if ((this.diskWMI.security_status & DiskSecurityStatus.SECURITY_HIGH) == DiskSecurityStatus.SECURITY_HIGH)
                {
                    this.unlock_disk_dialog.security_level.Content = "High Security";
                    this.unlock_disk_dialog.security_level.Foreground = (Brush)Application.Current.FindResource("TextBrush");
                    this.unlock_disk_dialog.security_level.FontWeight = FontWeights.Normal;
                }
                else
                {
                    this.unlock_disk_dialog.security_level.Content = "No Security?";
                    this.unlock_disk_dialog.security_level.Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                    this.unlock_disk_dialog.security_level.FontWeight = FontWeights.UltraBold;
                }
            }
            else
            {
                this.unlock_disk_dialog.security_enabled.Content = "No";
                this.unlock_disk_dialog.security_enabled.Foreground = (Brush)Application.Current.FindResource("TextBoxBadTextBrush");
                this.unlock_disk_dialog.security_enabled.FontWeight = FontWeights.UltraBold;
            }
        }

        public void add_unlock_methods()
        {
            this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.WEBTV_USER_PASSWORD, "WebTV User Password Unlock"));
            this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.CUSTOM_PASSWORD, "Custom Password Unlock"));
            this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.WEBTV_SECURITY_ERASE, "WebTV Security Erase Unlock"));
            //this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.CUSTOM_SECURITY_ERASE, "Custom Security Erase Unlock"));
            //this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.WEBTV_MASTER_PASSWORD, "WebTV Master Password Unlock"));
        }

        public void add_lock_methods()
        {
            this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.WEBTV_USER_PASSWORD_LOCK, "WebTV User Password Lock"));
            this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.CUSTOM_PASSWORD_LOCK, "Custom Password Lock"));
            //this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.CUSTOM_SECURITY_ERASE, "Custom Security Erase Unlock"));
            //this.unlock_disk_dialog.unlock_method.Items.Add(new_disk_unlock_method(DiskUnlockMethod.WEBTV_MASTER_PASSWORD, "WebTV Master Password Unlock"));
        }

        private void done_waiting()
        {
            if (this.unlock_disk_dialog.Dispatcher.CheckAccess() == false)
            {
                var cb = new void_call(done_waiting);

                this.unlock_disk_dialog.Dispatcher.Invoke(cb);
            }
            else
            {

                if (this.wait_window != null)
                {
                    this.wait_window.close_window();
                }

                this.loading = false;

                this.display_security_status();

                if (this.sent_disk_command)
                {
                    if(this.lock_mode)
                    {
                        finish_lock_disk();
                    }
                    else
                    {
                        finish_unlock_disk();
                    }
                }

                this.sent_disk_command = false;
            }
        }

        public void window_loaded(Object sender, RoutedEventArgs e)
        {
            this.loading = true;
            this.error_message = "";

            this.wait_window = new WaitMessage("Accessing Drive...");
            this.wait_window.Owner = this.unlock_disk_dialog;
            this.wait_window.Go(() =>
            {
                this.disk = this.diskWMI.open_disk(null, FileAccess.ReadWrite);

                this.diskWMI.security_status = this.disk.io.get_security_status();

                this.done_waiting();
            }, false);
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
            //this.unlock_disk_dialog.disk_layout.Items.Add(new_disk_layout(DiskLayout.UTV, "UltimateTV"));
            //this.unlock_disk_dialog.disk_layout.Items.Add(new_disk_layout(DiskLayout.WEBSTAR, "Echostar"));
            //this.unlock_disk_dialog.disk_layout.Items.Add(new_disk_layout(DiskLayout.LC2, "LC2/Derby Compatible"));
            // Removing to reduce user confusion. Really shouldn't need to use this option.
            //this.initialize_disk_dialog.target_layout.Items.Add(new_disk_layout(DiskLayout.PLAIN, "Plain"));
        }

        public void add_settings()
        {
            this.unlock_disk_dialog.ssid.ItemsSource = this.ssid_list.settings;
            this.unlock_disk_dialog.nvram_36.ItemsSource = this.disk_random_list.settings;
            this.unlock_disk_dialog.password.ItemsSource = this.disk_password_list.settings;
        }

        public DiskUnlockViewModel(DiskUnlock unlock_disk_dialog, DiskWMIEntry diskWMI, AddDiskViewModel add_disk_viewmodel, bool lock_mode = false)
        {
            this.unlock_disk_dialog = unlock_disk_dialog;
            this.add_disk_viewmodel = add_disk_viewmodel;
            this.diskWMI = diskWMI;
            this.lock_mode = lock_mode;

            if (this.lock_mode)
            {
                this.unlock_disk_dialog.Title = "Lock Disk: " + diskWMI.title;
                this.unlock_disk_dialog.unlock_method_label.Content = "Lock Method:";
                this.add_lock_methods();
            }
            else
            {
                this.unlock_disk_dialog.Title = "Unlock & Add Disk: " + diskWMI.title;
                this.unlock_disk_dialog.unlock_method_label.Content = "Unlock Method:";
                this.add_unlock_methods();
            }

            this.unlock_disk_dialog.Loaded += window_loaded;

            this.loading = true;

            this.ssid_list = new GenericValueSettings(JSONSettings.get<List<GenericValueSetting>>("ssid_list"));
            this.disk_random_list = new GenericValueSettings(JSONSettings.get<List<GenericValueSetting>>("disk_random_list"));
            this.disk_password_list = new GenericValueSettings(JSONSettings.get<List<GenericValueSetting>>("disk_password_list"));

            this.add_settings();
        }
    }
}
