using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace disk_editor
{
    public enum DiskSecurityStatus
    {
        NONE = 0,
        SUPPORTED = 1,
        ENABLED = 2,
        LOCKED = 4,
        FROZEN = 8,
        SECURITY_HIGH = 16,
        SECURITY_MAXIMUM = 32,
        ATTEMPTS_EXCEEDED = 64,
        HAS_ENHANCED_ERASE = 128
    };

    public partial class WebTVDiskIO : Stream
    {
        uint IOCTL_SCSI_PASS_THROUGH = CTL_CODE(IOCTL_SCSI_BASE, 0x0401, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
        uint IOCTL_SCSI_PASS_THROUGH_DIRECT = CTL_CODE(IOCTL_SCSI_BASE, 0x0405, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
        uint IOCTL_ATA_PASS_THROUGH = CTL_CODE(IOCTL_SCSI_BASE, 0x040B, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);
        uint IOCTL_ATA_PASS_THROUGH_DIRECT = CTL_CODE(IOCTL_SCSI_BASE, 0x040C, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS);

        public WebTVDisk disk { get; set; }
        public FileAccess access { get; set; }
        public DiskByteTransform target_byte_transform { get; set; }
        private SafeFileHandle file_handle { get; set; }

        private WebTVDiskCollationConverter _byte_converter;
        public WebTVDiskCollationConverter byte_converter 
        {
            get 
            {
                return _byte_converter;
            }
            set
            {
                if(value != null)
                {
                    _byte_converter = value;
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                return (this.access == FileAccess.Read || this.access == FileAccess.ReadWrite) ? true : false;
            }
        }
        
        public override bool CanWrite
        {
            get
            {
                return (this.access == FileAccess.Write || this.access == FileAccess.ReadWrite) ? true : false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public ulong _ULength
        {
            get
            {
                return this.disk.size_bytes;
            }
        }
        public override long Length
        {
            get
            {
                return (long)this._ULength;
            }
        }

        public ulong _UPosition
        {
            get
            {
                return this.Seek(0, SeekOrigin.Current);
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }
        public override long Position
        {
            get
            {
                return (long)this._UPosition;
            }
            set
            {
                this._UPosition = (ulong)value;
            }
        }

        public DiskCageBounds get_object_bounds()
        {
            DiskCageBounds cage_bounds;

            cage_bounds.start_position = 0;
            cage_bounds.end_position = (ulong)this.Length;

            return cage_bounds;
        }

        public WebTVDiskIO(WebTVDisk disk, WebTVDiskCollationConverter byte_converter = null, FileAccess access = FileAccess.Read, DiskByteTransform target_byte_transform = DiskByteTransform.NOSWAP)
        {
            this.disk = disk;
            this.access = access;
            this.target_byte_transform = target_byte_transform;
            this.byte_converter = byte_converter;
            this.access = access;

            this.open_disk();
        }

        ~WebTVDiskIO()
        {
            this.Close();

            this.disk = null;
        }

        public bool open_disk()
        {
            if(this.disk != null && this.disk.path != "")
            {
                if (this.file_handle == null)
                {
                    this.file_handle = this.open_disk(this.disk.path, this.access);
                }
            }

            return (this.file_handle != null);
        }

        public SafeFileHandle open_disk(string disk_path, FileAccess access = FileAccess.Read)
        {
            uint DesiredAccess;

            switch (this.access)
            {
                case FileAccess.Read:
                    DesiredAccess = WebTVDiskIO.GENERIC_READ;
                    break;
                case FileAccess.Write:
                    DesiredAccess = WebTVDiskIO.GENERIC_WRITE;
                    break;
                case FileAccess.ReadWrite:
                    DesiredAccess = WebTVDiskIO.GENERIC_READ | WebTVDiskIO.GENERIC_WRITE;
                    break;
                default:
                    DesiredAccess = WebTVDiskIO.GENERIC_READ;
                    break;
            }

            var file_handle = WebTVDiskIO.CreateFile(disk_path,
                                                     DesiredAccess,
                                                     WebTVDiskIO.FILE_SHARE_READ | WebTVDiskIO.FILE_SHARE_WRITE,
                                                     IntPtr.Zero,
                                                     WebTVDiskIO.OPEN_EXISTING,
                                                     WebTVDiskIO.FILE_FLAG_NO_BUFFERING | WebTVDiskIO.FILE_FLAG_WRITE_THROUGH,
                                                     IntPtr.Zero);

            if (file_handle.IsInvalid)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            return file_handle;
        }

        public unsafe DiskSecurityStatus get_security_status()
        {
            ushort[] result_data = null;

            try
            {
                result_data = this.get_security_status_ata();
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == 0x80070001) // error = Incorrect function. Try ATA through SCSI
                {
                    result_data = this.get_security_status_scsi();
                }
            }

            var raw_security_status = 0;
            if(result_data != null)
            {
                raw_security_status = result_data[ATA_IDENTIFY_DATA_SECURITY_STATUS];
            }
            var security_status = DiskSecurityStatus.NONE;

            if ((raw_security_status & 0x0001) != 0) // Security supported
            {
                security_status |= DiskSecurityStatus.SUPPORTED;

                if ((raw_security_status & 0x0002) != 0) // Security enabled
                {
                    security_status |= DiskSecurityStatus.ENABLED;
                }

                if ((raw_security_status & 0x0004) != 0) // Security locked
                {
                    security_status |= DiskSecurityStatus.LOCKED;
                }

                if ((raw_security_status & 0x0008) != 0) // Security frozen
                {
                    security_status |= DiskSecurityStatus.FROZEN;
                }

                if ((raw_security_status & 0x0010) != 0) // Try counts exceeded
                {
                    security_status |= DiskSecurityStatus.ATTEMPTS_EXCEEDED;
                }

                if ((raw_security_status & 0x0020) != 0) // Can enhanced security erase
                {
                    security_status |= DiskSecurityStatus.HAS_ENHANCED_ERASE;
                }

                if ((raw_security_status & 0x0100) != 0) // Security level
                {
                    security_status |= DiskSecurityStatus.SECURITY_MAXIMUM;
                }
                else
                {
                    security_status |= DiskSecurityStatus.SECURITY_HIGH;
                }
            }

            return security_status;
        }

        public unsafe ushort[] get_security_status_scsi()
        {
            GENERIC_SCSI_ATA_COMMAND identify_query = new GENERIC_SCSI_ATA_COMMAND();
            identify_query.header.length = (ushort)Marshal.SizeOf(identify_query.header);
            identify_query.header.cdbLength = SCSI_CBD_SIZE;
            identify_query.header.cdb = new byte[SCSI_CBD_SIZE];
            identify_query.header.cdb[SCSI_OPERATION_CODE] = ATA_PASS_THROUGH_12;
            identify_query.header.cdb[SCSI_ATA_PROTOCOL] = PROTOCOL_PIO_DATA_IN;
            // OFF_LINE=0, CK_COND=1, T_TYPE=0, T_DIR=0, BYTE_BLOCK=1, T_LENGTH=2 (specified in COUNT)
            //  transfer 1 512 byte block to HD (from section 12.2.2.2 of the SAT-5 spec)
            identify_query.header.cdb[SCSI_ATA_FLAGS] =
                ATA_DO_CK_COND |
                ATA_T_TYPE_512BYTE_BLOCKS |
                ATA_T_DIR_OUT |
                ATA_IS_BYTE_BLOCK |
                ATA_T_LENGTH_COUNT;
            identify_query.header.cdb[SCSI_ATA12_COMMAND] = ATA_IDENTIFY_DEVICE;

            identify_query.header.timeoutValue = 20;
            identify_query.header.dataIn = SCSI_IOCTL_DATA_IN;
            identify_query.header.senseInfoOffset = (uint)Marshal.OffsetOf(typeof(GENERIC_SCSI_ATA_COMMAND), "sense_data");
            identify_query.header.senseInfoLength = SCSI_SENSE_SIZE_BYTES;
            identify_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(GENERIC_SCSI_ATA_COMMAND), "result_data");
            identify_query.header.dataTransferLength = SCSI_BLOCK_SIZE_BYTES;

            identify_query.sense_data = new byte[SCSI_SENSE_SIZE_BYTES];
            identify_query.result_data = new ushort[SCSI_BLOCK_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_SCSI_PASS_THROUGH,
                    ref identify_query,
                    (uint)Marshal.SizeOf(identify_query),
                    ref identify_query,
                    (uint)Marshal.SizeOf(identify_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((SCSIOperationException.SCSIStatus)identify_query.header.scsiStatus != SCSIOperationException.SCSIStatus.GOOD)
            {
                throw new SCSIOperationException(
                    identify_query.header.scsiStatus,
                    (byte)(identify_query.sense_data[SCSI_SENSE_KEY] & 0xF),
                    identify_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE],
                    identify_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER]
                );
            }

            return identify_query.result_data;
        }


        public unsafe ushort[] get_security_status_ata()
        {
            GENERIC_ATA_COMMAND identify_query = new GENERIC_ATA_COMMAND();
            identify_query.header.length = (ushort)Marshal.SizeOf(identify_query.header);
            identify_query.header.ataFlags = (ushort)(ATA_FLAGS_DATA_IN | ATA_FLAGS_DRDY_REQUIRED);
            identify_query.header.previousTaskFile = new byte[8];
            identify_query.header.currentTaskFile = new byte[8];
            identify_query.header.currentTaskFile[ATA_REGISTER_COMMAND] = ATA_IDENTIFY_DEVICE;
            identify_query.header.timeoutValue = 20;
            identify_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(GENERIC_ATA_COMMAND), "result_data");
            identify_query.header.dataTransferLength = ATA_BLOCK_SIZE_BYTES;

            identify_query.result_data = new ushort[ATA_BLOCK_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_ATA_PASS_THROUGH,
                    ref identify_query,
                    (uint)Marshal.SizeOf(identify_query),
                    ref identify_query,
                    (uint)Marshal.SizeOf(identify_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((ATAOperationException.ATAError)identify_query.header.currentTaskFile[ATA_REGISTER_ERROR] != ATAOperationException.ATAError.NONE)
            {
                throw new ATAOperationException(identify_query.header.currentTaskFile[ATA_REGISTER_ERROR]);
            }

            return identify_query.result_data;
        }

        public unsafe void security_remove(bool master_unlock, byte[] password)
        {
            try
            {
                this.security_remove_ata(master_unlock, password);
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == 0x80070001) // error = Incorrect function. Try ATA through SCSI
                {
                    this.security_remove_scsi(master_unlock, password);
                }
            }
        
        }
        public unsafe void security_remove_scsi(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_SCSI_ATA_COMMAND remove_query = new SECURITY_UNLOCK_SCSI_ATA_COMMAND();
            remove_query.header.length = (ushort)Marshal.SizeOf(remove_query.header);
            remove_query.header.cdbLength = SCSI_CBD_SIZE;
            remove_query.header.cdb = new byte[SCSI_CBD_SIZE];
            remove_query.header.cdb[SCSI_OPERATION_CODE] = ATA_PASS_THROUGH_12;
            remove_query.header.cdb[SCSI_ATA_PROTOCOL] = PROTOCOL_PIO_DATA_IN;
            // OFF_LINE=0, CK_COND=1, T_TYPE=0, T_DIR=0, BYTE_BLOCK=1, T_LENGTH=2 (specified in COUNT)
            //  transfer 1 512 byte block to HD (from section 12.2.2.2 of the SAT-5 spec)
            remove_query.header.cdb[SCSI_ATA_FLAGS] =
                ATA_DO_CK_COND |
                ATA_T_TYPE_512BYTE_BLOCKS |
                ATA_T_DIR_OUT |
                ATA_IS_BYTE_BLOCK |
                ATA_T_LENGTH_COUNT;
            remove_query.header.cdb[SCSI_ATA12_COMMAND] = ATA_SECURITY_DISABLE_PASSWORD;
            remove_query.header.cdb[SCSI_ATA12_COUNT] = 2;

            remove_query.header.timeoutValue = 10;
            remove_query.header.dataIn = SCSI_IOCTL_DATA_OUT;
            remove_query.header.senseInfoOffset = (uint)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "sense_data");
            remove_query.header.senseInfoLength = SCSI_SENSE_SIZE_BYTES;
            remove_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "identifier");
            remove_query.header.dataTransferLength = SCSI_BLOCK_SIZE_BYTES;

            remove_query.sense_data = new byte[SCSI_SENSE_SIZE_BYTES];
            remove_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0000);
            remove_query.password = password;
            remove_query.reserved = new ushort[SCSI_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_SCSI_PASS_THROUGH,
                    ref remove_query,
                    (uint)Marshal.SizeOf(remove_query),
                    ref remove_query,
                    (uint)Marshal.SizeOf(remove_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((SCSIOperationException.SCSIStatus)remove_query.header.scsiStatus != SCSIOperationException.SCSIStatus.GOOD)
            {
                throw new SCSIOperationException(
                    remove_query.header.scsiStatus,
                    (byte)(remove_query.sense_data[SCSI_SENSE_KEY] & 0xF),
                    remove_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE],
                    remove_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER]
                );
            }
        }

        public unsafe void security_remove_ata(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_ATA_QUERY remove_query = new SECURITY_UNLOCK_ATA_QUERY();
            remove_query.header.length = (ushort)Marshal.SizeOf(remove_query.header);
            remove_query.header.ataFlags = (ushort)(ATA_FLAGS_DATA_OUT | ATA_FLAGS_DRDY_REQUIRED);
            remove_query.header.dataTransferLength = ATA_BLOCK_SIZE_BYTES;
            remove_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_ATA_QUERY), "identifier");
            remove_query.header.previousTaskFile = new byte[8];
            remove_query.header.currentTaskFile = new byte[8];
            remove_query.header.currentTaskFile[ATA_REGISTER_SECTOR_COUNT] = 1;
            remove_query.header.currentTaskFile[ATA_REGISTER_COMMAND] = ATA_SECURITY_DISABLE_PASSWORD;

            remove_query.header.timeoutValue = 10;
            remove_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0000);
            remove_query.password = password;
            remove_query.reserved = new ushort[ATA_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_ATA_PASS_THROUGH,
                    ref remove_query,
                    (uint)Marshal.SizeOf(remove_query),
                    ref remove_query,
                    (uint)Marshal.SizeOf(remove_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((ATAOperationException.ATAError)remove_query.header.currentTaskFile[ATA_REGISTER_ERROR] != ATAOperationException.ATAError.NONE)
            {
                throw new ATAOperationException(remove_query.header.currentTaskFile[ATA_REGISTER_ERROR]);
            }
        }

        public unsafe void security_unlock(bool master_unlock, byte[] password)
        {
            try
            {
                this.security_unlock_ata(master_unlock, password);
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == 0x80070001) // error = Incorrect function. Try ATA through SCSI
                {
                    this.security_unlock_scsi(master_unlock, password);
                }
            }
        }

        public unsafe void security_unlock_scsi(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_SCSI_ATA_COMMAND unlock_query = new SECURITY_UNLOCK_SCSI_ATA_COMMAND();
            unlock_query.header.length = (ushort)Marshal.SizeOf(unlock_query.header);
            unlock_query.header.cdbLength = SCSI_CBD_SIZE;
            unlock_query.header.cdb = new byte[SCSI_CBD_SIZE];
            unlock_query.header.cdb[SCSI_OPERATION_CODE] = ATA_PASS_THROUGH_12;
            unlock_query.header.cdb[SCSI_ATA_PROTOCOL] = PROTOCOL_PIO_DATA_IN;
            // OFF_LINE=0, CK_COND=1, T_TYPE=0, T_DIR=0, BYTE_BLOCK=1, T_LENGTH=2 (specified in COUNT)
            //  transfer 1 512 byte block to HD (from section 12.2.2.2 of the SAT-5 spec)
            unlock_query.header.cdb[SCSI_ATA_FLAGS] =
                ATA_DO_CK_COND |
                ATA_T_TYPE_512BYTE_BLOCKS |
                ATA_T_DIR_IN |
                ATA_IS_BYTE_BLOCK |
                ATA_T_LENGTH_COUNT;
            unlock_query.header.cdb[SCSI_ATA12_COMMAND] = ATA_SECURITY_UNLOCK;
            unlock_query.header.cdb[SCSI_ATA12_COUNT] = 2;

            unlock_query.header.timeoutValue = 10;
            unlock_query.header.dataIn = SCSI_IOCTL_DATA_OUT;
            unlock_query.header.senseInfoOffset = (uint)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "sense_data");
            unlock_query.header.senseInfoLength = SCSI_SENSE_SIZE_BYTES;
            unlock_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "identifier");
            unlock_query.header.dataTransferLength = SCSI_BLOCK_SIZE_BYTES;

            unlock_query.sense_data = new byte[SCSI_SENSE_SIZE_BYTES];
            unlock_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0000);
            unlock_query.password = password;
            unlock_query.reserved = new ushort[SCSI_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_SCSI_PASS_THROUGH,
                    ref unlock_query,
                    (uint)Marshal.SizeOf(unlock_query),
                    ref unlock_query,
                    (uint)Marshal.SizeOf(unlock_query), 
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((SCSIOperationException.SCSIStatus)unlock_query.header.scsiStatus != SCSIOperationException.SCSIStatus.GOOD)
            {
                throw new SCSIOperationException(
                    unlock_query.header.scsiStatus,
                    (byte)(unlock_query.sense_data[SCSI_SENSE_KEY] & 0xF),
                    unlock_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE],
                    unlock_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER]
                );
            }

            /*var data = "";
            for (var i = 0; i < unlock_query.password.Length; i++)
            {
                var hex_string = unlock_query.password[i].ToString("X").PadLeft(4, '0');

                data += hex_string;
            }
            System.Windows.MessageBox.Show("[" + bytes_returned.ToString() + "]unlock_query_OK: " + data);*/
        }

        public unsafe void security_unlock_ata(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_ATA_QUERY unlock_query = new SECURITY_UNLOCK_ATA_QUERY();
            unlock_query.header.length = (ushort)Marshal.SizeOf(unlock_query.header);
            unlock_query.header.ataFlags = (ushort)(ATA_FLAGS_DATA_OUT | ATA_FLAGS_DRDY_REQUIRED);
            unlock_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_ATA_QUERY), "identifier");
            unlock_query.header.dataTransferLength = ATA_BLOCK_SIZE_BYTES;
            unlock_query.header.previousTaskFile = new byte[8];
            unlock_query.header.currentTaskFile = new byte[8];
            unlock_query.header.currentTaskFile[ATA_REGISTER_SECTOR_COUNT] = 1;
            unlock_query.header.currentTaskFile[ATA_REGISTER_COMMAND] = ATA_SECURITY_UNLOCK;

            unlock_query.header.timeoutValue = 10;
            unlock_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0000);
            unlock_query.password = password;
            unlock_query.reserved = new ushort[ATA_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_ATA_PASS_THROUGH,
                    ref unlock_query,
                    (uint)Marshal.SizeOf(unlock_query),
                    ref unlock_query,
                    (uint)Marshal.SizeOf(unlock_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((ATAOperationException.ATAError)unlock_query.header.currentTaskFile[ATA_REGISTER_ERROR] != ATAOperationException.ATAError.NONE)
            {
                throw new ATAOperationException(unlock_query.header.currentTaskFile[ATA_REGISTER_ERROR]);
            }
        }

        public unsafe void security_lock(bool master_unlock, byte[] password)
        {
            try
            {
                this.security_lock_ata(master_unlock, password);
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == 0x80070001) // error = Incorrect function. Try ATA through SCSI
                {
                    this.security_lock_scsi(master_unlock, password);
                }
            }
        }

        public unsafe void security_lock_scsi(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_SCSI_ATA_COMMAND lock_query = new SECURITY_UNLOCK_SCSI_ATA_COMMAND();
            lock_query.header.length = (ushort)Marshal.SizeOf(lock_query.header);
            lock_query.header.cdbLength = SCSI_CBD_SIZE;
            lock_query.header.cdb = new byte[SCSI_CBD_SIZE];
            lock_query.header.cdb[SCSI_OPERATION_CODE] = ATA_PASS_THROUGH_12;
            lock_query.header.cdb[SCSI_ATA_PROTOCOL] = PROTOCOL_PIO_DATA_IN;
            // OFF_LINE=0, CK_COND=1, T_TYPE=0, T_DIR=0, BYTE_BLOCK=1, T_LENGTH=2 (specified in COUNT)
            //  transfer 1 512 byte block to HD (from section 12.2.2.2 of the SAT-5 spec)
            lock_query.header.cdb[SCSI_ATA_FLAGS] =
                ATA_DO_CK_COND |
                ATA_T_TYPE_512BYTE_BLOCKS |
                ATA_T_DIR_IN |
                ATA_IS_BYTE_BLOCK |
                ATA_T_LENGTH_COUNT;
            lock_query.header.cdb[SCSI_ATA12_COMMAND] = ATA_SECURITY_SET_PASSWORD;
            lock_query.header.cdb[SCSI_ATA12_COUNT] = 2;

            lock_query.header.timeoutValue = 10;
            lock_query.header.dataIn = SCSI_IOCTL_DATA_OUT;
            lock_query.header.senseInfoOffset = (uint)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "sense_data");
            lock_query.header.senseInfoLength = SCSI_SENSE_SIZE_BYTES;
            lock_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "identifier");
            lock_query.header.dataTransferLength = SCSI_BLOCK_SIZE_BYTES;

            lock_query.sense_data = new byte[SCSI_SENSE_SIZE_BYTES];
            lock_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0100);
            lock_query.password = password;
            lock_query.reserved = new ushort[SCSI_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_SCSI_PASS_THROUGH,
                    ref lock_query,
                    (uint)Marshal.SizeOf(lock_query),
                    ref lock_query,
                    (uint)Marshal.SizeOf(lock_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((SCSIOperationException.SCSIStatus)lock_query.header.scsiStatus != SCSIOperationException.SCSIStatus.GOOD)
            {
                throw new SCSIOperationException(
                    lock_query.header.scsiStatus,
                    (byte)(lock_query.sense_data[SCSI_SENSE_KEY] & 0xF),
                    lock_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE],
                    lock_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER]
                );
            }
        }

        public unsafe void security_lock_ata(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_ATA_QUERY lock_query = new SECURITY_UNLOCK_ATA_QUERY();
            lock_query.header.length = (ushort)Marshal.SizeOf(lock_query.header);
            lock_query.header.ataFlags = (ushort)(ATA_FLAGS_DATA_OUT | ATA_FLAGS_DRDY_REQUIRED);
            lock_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_ATA_QUERY), "identifier");
            lock_query.header.dataTransferLength = ATA_BLOCK_SIZE_BYTES;
            lock_query.header.previousTaskFile = new byte[8];
            lock_query.header.currentTaskFile = new byte[8];
            lock_query.header.currentTaskFile[ATA_REGISTER_SECTOR_COUNT] = 1;
            lock_query.header.currentTaskFile[ATA_REGISTER_COMMAND] = ATA_SECURITY_SET_PASSWORD;

            lock_query.header.timeoutValue = 10;
            lock_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0100);
            lock_query.password = password;
            lock_query.reserved = new ushort[ATA_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_ATA_PASS_THROUGH,
                    ref lock_query,
                    (uint)Marshal.SizeOf(lock_query),
                    ref lock_query,
                    (uint)Marshal.SizeOf(lock_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((ATAOperationException.ATAError)lock_query.header.currentTaskFile[ATA_REGISTER_ERROR] != ATAOperationException.ATAError.NONE)
            {
                throw new ATAOperationException(lock_query.header.currentTaskFile[ATA_REGISTER_ERROR]);
            }
        }

        public unsafe void security_erase_prepare()
        {
            try
            {
                this.security_erase_prepare_ata();
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == 0x80070001) // error = Incorrect function. Try ATA through SCSI
                {
                    this.security_erase_prepare_scsi();
                }
            }
        }

        public unsafe void security_erase_prepare_scsi()
        {
            GENERIC_SCSI_ATA_COMMAND erase_prepare_query = new GENERIC_SCSI_ATA_COMMAND();
            erase_prepare_query.header.length = (ushort)Marshal.SizeOf(erase_prepare_query.header);
            erase_prepare_query.header.cdbLength = SCSI_CBD_SIZE;
            erase_prepare_query.header.cdb = new byte[SCSI_CBD_SIZE];
            erase_prepare_query.header.cdb[SCSI_OPERATION_CODE] = ATA_PASS_THROUGH_12;
            erase_prepare_query.header.cdb[SCSI_ATA_PROTOCOL] = PROTOCOL_PIO_DATA_IN;
            // OFF_LINE=0, CK_COND=1, T_TYPE=0, T_DIR=0, BYTE_BLOCK=1, T_LENGTH=2 (specified in COUNT)
            //  transfer 1 512 byte block to HD (from section 12.2.2.2 of the SAT-5 spec)
            erase_prepare_query.header.cdb[SCSI_ATA_FLAGS] =
                ATA_DO_CK_COND |
                ATA_T_TYPE_512BYTE_BLOCKS |
                ATA_T_DIR_IN |
                ATA_IS_BYTE_BLOCK |
                ATA_T_LENGTH_COUNT;
            erase_prepare_query.header.cdb[SCSI_ATA12_COMMAND] = ATA_SECURITY_ERASE_PREPARE;

            erase_prepare_query.header.timeoutValue = 10;
            erase_prepare_query.header.dataIn = SCSI_IOCTL_DATA_OUT;
            erase_prepare_query.header.senseInfoOffset = (uint)Marshal.OffsetOf(typeof(GENERIC_SCSI_ATA_COMMAND), "sense_data");
            erase_prepare_query.header.senseInfoLength = SCSI_SENSE_SIZE_BYTES;
            erase_prepare_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(GENERIC_SCSI_ATA_COMMAND), "result_data");
            erase_prepare_query.header.dataTransferLength = SCSI_BLOCK_SIZE_BYTES;

            erase_prepare_query.sense_data = new byte[SCSI_SENSE_SIZE_BYTES];
            erase_prepare_query.result_data = new ushort[SCSI_BLOCK_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_SCSI_PASS_THROUGH,
                    ref erase_prepare_query,
                    (uint)Marshal.SizeOf(erase_prepare_query),
                    ref erase_prepare_query,
                    (uint)Marshal.SizeOf(erase_prepare_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((SCSIOperationException.SCSIStatus)erase_prepare_query.header.scsiStatus != SCSIOperationException.SCSIStatus.GOOD)
            {
                throw new SCSIOperationException(
                    erase_prepare_query.header.scsiStatus,
                    (byte)(erase_prepare_query.sense_data[SCSI_SENSE_KEY] & 0xF),
                    erase_prepare_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE],
                    erase_prepare_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER]
                );
            }
        }

        public unsafe void security_erase_prepare_ata()
        {
            GENERIC_ATA_COMMAND erase_prepare_query = new GENERIC_ATA_COMMAND();
            erase_prepare_query.header.length = (ushort)Marshal.SizeOf(erase_prepare_query.header);
            erase_prepare_query.header.ataFlags = (ushort)(ATA_FLAGS_DATA_OUT | ATA_FLAGS_DRDY_REQUIRED);
            erase_prepare_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(GENERIC_ATA_COMMAND), "result_data");
            erase_prepare_query.header.dataTransferLength = ATA_BLOCK_SIZE_BYTES;
            erase_prepare_query.header.previousTaskFile = new byte[8];
            erase_prepare_query.header.currentTaskFile = new byte[8];
            erase_prepare_query.header.currentTaskFile[ATA_REGISTER_COMMAND] = ATA_SECURITY_ERASE_PREPARE;

            erase_prepare_query.header.timeoutValue = 10;
            erase_prepare_query.result_data = new ushort[ATA_BLOCK_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_ATA_PASS_THROUGH,
                    ref erase_prepare_query,
                    (uint)Marshal.SizeOf(erase_prepare_query),
                    ref erase_prepare_query,
                    (uint)Marshal.SizeOf(erase_prepare_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((ATAOperationException.ATAError)erase_prepare_query.header.currentTaskFile[ATA_REGISTER_ERROR] != ATAOperationException.ATAError.NONE)
            {
                throw new ATAOperationException(erase_prepare_query.header.currentTaskFile[ATA_REGISTER_ERROR]);
            }
        }

        public unsafe void security_erase(bool master_unlock, byte[] password)
        {
            try
            {
                this.security_erase_ata(master_unlock, password);
            }
            catch (COMException ex)
            {
                if ((uint)ex.ErrorCode == 0x80070001) // error = Incorrect function. Try ATA through SCSI
                {
                    this.security_erase_scsi(master_unlock, password);
                }
            }
        }

        public unsafe void security_erase_scsi(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_SCSI_ATA_COMMAND erase_query = new SECURITY_UNLOCK_SCSI_ATA_COMMAND();
            erase_query.header.length = (ushort)Marshal.SizeOf(erase_query.header);
            erase_query.header.cdbLength = SCSI_CBD_SIZE;
            erase_query.header.cdb = new byte[SCSI_CBD_SIZE];
            erase_query.header.cdb[SCSI_OPERATION_CODE] = ATA_PASS_THROUGH_12;
            erase_query.header.cdb[SCSI_ATA_PROTOCOL] = PROTOCOL_PIO_DATA_IN;
            // OFF_LINE=0, CK_COND=1, T_TYPE=0, T_DIR=0, BYTE_BLOCK=1, T_LENGTH=2 (specified in COUNT)
            //  transfer 1 512 byte block to HD (from section 12.2.2.2 of the SAT-5 spec)
            erase_query.header.cdb[SCSI_ATA_FLAGS] =
                ATA_DO_CK_COND |
                ATA_T_TYPE_512BYTE_BLOCKS |
                ATA_T_DIR_IN |
                ATA_IS_BYTE_BLOCK |
                ATA_T_LENGTH_COUNT;
            erase_query.header.cdb[SCSI_ATA12_COMMAND] = ATA_SECURITY_ERASE_UNIT;
            erase_query.header.cdb[SCSI_ATA12_COUNT] = 2;

            //erase_query.header.timeoutValue = 508 * 60;
            erase_query.header.timeoutValue = 540 * 60; // 540 minutes =  9 hours
            erase_query.header.dataIn = SCSI_IOCTL_DATA_OUT;
            erase_query.header.senseInfoOffset = (uint)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "sense_data");
            erase_query.header.senseInfoLength = SCSI_SENSE_SIZE_BYTES;
            erase_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_SCSI_ATA_COMMAND), "identifier");
            erase_query.header.dataTransferLength = SCSI_BLOCK_SIZE_BYTES;

            erase_query.sense_data = new byte[SCSI_SENSE_SIZE_BYTES];
            erase_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0000);
            erase_query.password = password;
            erase_query.reserved = new ushort[SCSI_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_SCSI_PASS_THROUGH,
                    ref erase_query,
                    (uint)Marshal.SizeOf(erase_query),
                    ref erase_query,
                    (uint)Marshal.SizeOf(erase_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if ((SCSIOperationException.SCSIStatus)erase_query.header.scsiStatus != SCSIOperationException.SCSIStatus.GOOD)
            {
                throw new SCSIOperationException(
                    erase_query.header.scsiStatus,
                    (byte)(erase_query.sense_data[SCSI_SENSE_KEY] & 0xF),
                    erase_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE],
                    erase_query.sense_data[SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER]
                );
            }
        }

        public unsafe void security_erase_ata(bool master_unlock, byte[] password)
        {
            SECURITY_UNLOCK_ATA_QUERY erase_query = new SECURITY_UNLOCK_ATA_QUERY();
            erase_query.header.length = (ushort)Marshal.SizeOf(erase_query.header);
            erase_query.header.ataFlags = (ushort)(ATA_FLAGS_DATA_OUT | ATA_FLAGS_DRDY_REQUIRED);
            erase_query.header.dataTransferLength = ATA_BLOCK_SIZE_BYTES;
            erase_query.header.dataBufferOffset = (IntPtr)Marshal.OffsetOf(typeof(SECURITY_UNLOCK_ATA_QUERY), "identifier");
            erase_query.header.previousTaskFile = new byte[8];
            erase_query.header.currentTaskFile = new byte[8];
            erase_query.header.currentTaskFile[ATA_REGISTER_SECTOR_COUNT] = 1;
            erase_query.header.currentTaskFile[ATA_REGISTER_COMMAND] = ATA_SECURITY_ERASE_UNIT;

            //erase_query.header.timeoutValue = 508 * 60;
            erase_query.header.timeoutValue = 540 * 60; // 540 minutes =  9 hours
            erase_query.identifier = (ushort)((master_unlock) ? 0x0101 : 0x0000);
            erase_query.password = password;
            erase_query.reserved = new ushort[ATA_RESERVED_SIZE_WORDS];

            uint bytes_returned;

            if (!WebTVDiskIO.DeviceIoControl(
                    this.file_handle,
                    IOCTL_ATA_PASS_THROUGH,
                    ref erase_query,
                    (uint)Marshal.SizeOf(erase_query),
                    ref erase_query,
                    (uint)Marshal.SizeOf(erase_query),
                    out bytes_returned,
                    IntPtr.Zero)
                )
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            else if((ATAOperationException.ATAError)erase_query.header.currentTaskFile[ATA_REGISTER_ERROR] != ATAOperationException.ATAError.NONE)
            {
                throw new ATAOperationException(erase_query.header.currentTaskFile[ATA_REGISTER_ERROR]);
            }
        }

        public ulong get_sector_alignment_error(ulong position)
        {
            return (position % (ulong)this.disk.sector_bytes_length);
        }

        public bool in_sector_range(ulong position)
        {
            return (this.get_sector_alignment_error(position) == 0);
        }

        public ulong align_position_to_sector(ulong position)
        {
            var alignment_error = this.get_sector_alignment_error(position);

            if (alignment_error > 0)
            {
                return (position - alignment_error);
            }
            else
            {
                return position;
            }
        }

        public ulong align_read_size_to_sector(ulong size)
        {
            var alignment_error = this.get_sector_alignment_error(size);

            if (alignment_error > 0)
            {
                return (size + (this.disk.sector_bytes_length - alignment_error));
            }
            else
            {
                return size;
            }
        }

        private static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        {
            return ((DeviceType << 16) | (Access << 14) | (Function << 2) | Method);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("You can't adjust the size of a fixed-size disk!");
        }

        public override void Close()
        {
            if (this.file_handle != null)
            {
                WebTVDiskIO.CloseHandle(this.file_handle);
                this.file_handle.SetHandleAsInvalid();
                this.file_handle = null;
            }

            base.Close();
        }

        public override void Flush()
        {
        }

        public ulong Seek(ulong disk_offset, SeekOrigin origin)
        {
            if (!this.in_sector_range(disk_offset))
            {
                throw new DataMisalignedException("Seek position must be aligned to a sector position.");
            }

            ulong new_position = 0;

            if (disk_offset <= (this._ULength - 1))
            {
                if (!WebTVDiskIO.SetFilePointerEx(this.file_handle, disk_offset, out new_position, (uint)origin))
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }
            else
            {
                // Allow for this so we don't fail on a corrupt partition table (or other corrupt data). Let the user decide.
                return (this._ULength - 1);
                //throw new EndOfStreamException("You cannot seek beyond the length of the disk!");
            }

            return new_position;
        }
        public override long Seek(long disk_offset, SeekOrigin origin)
        {
            return (long)this.Seek((ulong)disk_offset, origin);
        }

        public unsafe uint Read(byte[] buffer, uint buffer_offset, uint read_size, bool convert_bytes = true)
        {
            if (!this.in_sector_range(read_size))
            {
                throw new DataMisalignedException("Read size must be aligned to sector size.");
            }

            uint bytes_read = 0;

            fixed (byte* buffer_fixed = buffer)
            {
                if (!WebTVDiskIO.ReadFile(this.file_handle, buffer_fixed + buffer_offset, read_size, &bytes_read, IntPtr.Zero))
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }

            if (convert_bytes && byte_converter != null)
            {
                byte_converter.convert_bytes(this.disk.byte_transform, ref buffer, buffer_offset, read_size, this.target_byte_transform);
            }

            return bytes_read;
        }
        public override int Read(byte[] buffer, int buffer_offset, int read_size)
        {
            return (int)this.Read(buffer, (uint)buffer_offset, (uint)read_size);
        }

        public unsafe uint AbsRead(byte[] buffer, ulong disk_offset, int buffer_offset, uint read_size, bool convert_bytes = true)
        {
            var aligned_disk_offset = this.align_position_to_sector(disk_offset);
            this.Seek(aligned_disk_offset, SeekOrigin.Begin);

            var offset_difference = disk_offset - aligned_disk_offset;

            var aligned_read_size = this.align_read_size_to_sector(offset_difference + read_size);
            byte[] _buffer = new byte[aligned_read_size];

            this.Read(_buffer, 0, (uint)aligned_read_size, convert_bytes);

            Buffer.BlockCopy(_buffer, (int)offset_difference, buffer, buffer_offset, (int)read_size);

            return (uint)read_size;
        }

        public unsafe void Write(byte[] buffer, uint buffer_offset, uint write_size, bool convert_bytes = true)
        {
            if (convert_bytes && byte_converter != null)
            {
                byte_converter.convert_bytes(this.disk.byte_transform, ref buffer, buffer_offset, write_size, this.target_byte_transform);
            }

            uint bytes_written = 0;
            fixed (byte* buffer_fixed = buffer)
            {
                if (!WebTVDiskIO.WriteFile(this.file_handle, buffer_fixed + buffer_offset, write_size, &bytes_written, IntPtr.Zero))
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }
        }
        public override void Write(byte[] buffer, int buffer_offset, int write_size)
        {
            this.Write(buffer, (uint)buffer_offset, (uint)write_size);
        }

        public unsafe void AbsWrite(byte[] buffer, ulong disk_offset, int buffer_offset, uint write_size, bool convert_bytes = true)
        {
            var aligned_disk_offset = this.align_position_to_sector(disk_offset);
            this.Seek(aligned_disk_offset, SeekOrigin.Begin);

            var offset_difference = disk_offset - aligned_disk_offset;

            var aligned_write_size = this.align_read_size_to_sector(write_size + offset_difference);
            byte[] _buffer = new byte[aligned_write_size];

            if(aligned_disk_offset != disk_offset || aligned_write_size != write_size)
            {
                this.Read(_buffer, 0, (uint)aligned_write_size, convert_bytes);
            }

            Buffer.BlockCopy(buffer, buffer_offset, _buffer, (int)offset_difference, (int)write_size);

            this.Seek(aligned_disk_offset, SeekOrigin.Begin);

            this.Write(_buffer, (uint)buffer_offset, (uint)aligned_write_size, convert_bytes);
        }
    }
}
