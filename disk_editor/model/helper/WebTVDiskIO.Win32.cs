using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace disk_editor
{
    /// <summary>
    /// P/Invoke wrappers around Win32 functions and constants.
    /// </summary>
    public partial class WebTVDiskIO
    {
        #region Enums used in unmanaged functions
        #endregion

        #region Constants used in unmanaged functions
        public const uint FILE_ANY_ACCESS = 0x00000001;
        public const uint FILE_READ_ACCESS = 0x00000001;
        public const uint FILE_WRITE_ACCESS = 0x00000002;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint FILE_SHARE_DELETE = 0x00000004;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_DEVICE_CONTROLLER = 0x00000004;
        public const uint IOCTL_SCSI_BASE = FILE_DEVICE_CONTROLLER;
        public const uint METHOD_BUFFERED = 0;

        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);

        public const uint FILE_FLAG_NO_BUFFERING = 0x20000000;
        public const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;
        public const uint FILE_READ_ATTRIBUTES = (0x0080);
        public const uint FILE_WRITE_ATTRIBUTES = 0x0100;
        public const uint ERROR_INSUFFICIENT_BUFFER = 122;

        public const int FSCTL_LOCK_VOLUME = 0x00090018;
        public const int FSCTL_UNLOCK_VOLUME = 0x0009001c;
        public const int FSCTL_DISMOUNT_VOLUME = 0x00090020;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        public const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;
        public const int IOCTL_STORAGE_QUERY_PROPERTY = 0x002D1400;

        public const uint FILE_DEVICE_MASS_STORAGE = 0x0000002d;
        public const uint IOCTL_STORAGE_BASE = FILE_DEVICE_MASS_STORAGE;

        public const uint ATA_FLAGS_DRDY_REQUIRED = 0x01;
        public const uint ATA_FLAGS_DATA_IN = 0x02;
        public const uint ATA_FLAGS_DATA_OUT = 0x04;

        public const byte ATA_IDENTIFY_DEVICE = 0xEC;
        public const ushort ATA_IDENTIFY_DATA_SECURITY_STATUS = 0x80;

        public const byte ATA_SECURITY_PASSWORD_SIZE = 0x20;

        public const byte ATA_SECURITY_SET_PASSWORD = 0xF1;
        public const byte ATA_SECURITY_UNLOCK = 0xF2;
        public const byte ATA_SECURITY_ERASE_PREPARE = 0xF3;
        public const byte ATA_SECURITY_ERASE_UNIT = 0xF4;
        public const byte ATA_SECURITY_FREEZE = 0xF5;
        public const byte ATA_SECURITY_DISABLE_PASSWORD = 0xF6;

        public const int ATA_BLOCK_SIZE_WORDS = 0x100;
        public const int ATA_BLOCK_SIZE_BYTES = ATA_BLOCK_SIZE_WORDS * 2;

        public const int ATA_RESERVED_SIZE_WORDS = ATA_BLOCK_SIZE_WORDS - 1 - (ATA_SECURITY_PASSWORD_SIZE / 2);

        // https://wiki.osdev.org/ATA_PIO_Mode#Error_Register
        public const int ATA_REGISTER_ERROR = 0;
        /*
           Bit Abbreviation    Function
           0	AMNF            Address mark not found.
           1	TKZNF           Track zero not found.
           2	ABRT            Aborted command.
           3	MCR             Media change request.
           4	IDNF            ID not found.
           5	MC              Media changed.
           6	UNC             Uncorrectable data error.
           7	BBK             Bad Block detected.
        */
        public const int ATA_REGISTER_FEATURES = 0;
        public const int ATA_REGISTER_SECTOR_COUNT = 1;
        public const int ATA_REGISTER_SECTOR_NUMBER = 2;
        public const int ATA_REGISTER_CYLINDER_LOW = 3;
        public const int ATA_REGISTER_CYLINDER_HIGH = 4;
        public const int ATA_REGISTER_DEVICE_HEAD = 5;
        public const int ATA_REGISTER_STATUS = 6;
        public const int ATA_REGISTER_COMMAND = 6;
        public const int ATA_REGISTER_RESERVED = 7;

        public const byte SCSI_IOCTL_DATA_OUT = 0x00;
        public const byte SCSI_IOCTL_DATA_IN = 0x01;
        public const byte SCSI_IOCTL_DATA_UNSPECIFIED = 0x02;

        public const byte SCSI_SENSE_KEY = 0x02;
        public const byte SCSI_ADDITIONAL_SENSE_CODE = 0x0C;
        public const byte SCSI_ADDITIONAL_SENSE_CODE_QUALIFIER = 0x0D;

        public const byte SCSI_SENSE_SIZE_WORDS = 0x0C;
        public const int SCSI_SENSE_SIZE_BYTES = SCSI_SENSE_SIZE_WORDS * 2;

        public const int SCSI_BLOCK_SIZE_WORDS = 0x100;
        public const int SCSI_BLOCK_SIZE_BYTES = SCSI_BLOCK_SIZE_WORDS * 2;
        
        public const int SCSI_RESERVED_SIZE_WORDS = SCSI_BLOCK_SIZE_WORDS - 1 - (ATA_SECURITY_PASSWORD_SIZE / 2);

        public const byte SCSI_CBD_SIZE = 0x10;

        public const byte SCSI_OPERATION_CODE = 0x00;
        public const byte SCSI_INQUIRY = 0x12;
        public const byte SCSI_SECURITY_OUT = 0xB5;
        public const byte SCSI_SECURITY_IN = 0xA2;
        public const byte ATA_PASS_THROUGH_12 = 0xA1;
        public const byte ATA_PASS_THROUGH_16 = 0x85;
        public const byte ATA_PASS_THROUGH_32 = 0x7F;

        public const byte SCSI_SECURITY_PROTOCOL = 0x01;
        public const byte SCSI_ATA_SECURITY_PROTOCOL = 0xEF;

        public const byte SCSI_SECURITY_PROTOCOL_SPECIFIC = 0x03;
        public const byte SCSI_SECURITY_SPECIFIC_SET = 0x01;
        public const byte SCSI_SECURITY_SPECIFIC_UNLOCK = 0x02;
        public const byte SCSI_SECURITY_SPECIFIC_ERASE_PREPARE = 0x03;
        public const byte SCSI_SECURITY_SPECIFIC_ERASE = 0x04;
        public const byte SCSI_SECURITY_SPECIFIC_FREEZE = 0x05;
        public const byte SCSI_SECURITY_SPECIFIC_DISABLE = 0x06;

        public const byte SCSI_TRANSFER_LENGTH = 0x09;
        public const byte SCSI_ATA_SECURITY_TRANSFER_LENGTH = 0x24;

        public const byte SCSI_EVPD_BIT = 0x01;
        public const byte SCSI_EVPD_STANDARD = 0x00;
        public const byte SCSI_EVPD_PAGECODE = 0x01;

        public const byte SCSI_PAGE_CODE = 0x02;

        public const byte ATA_VPD_PAGE = 0x89;

        public const byte SCSI_ATA_PROTOCOL = 0x01;
        public const byte PROTOCOL_HARDWARE_RESET = 0x00 << 1;
        public const byte PROTOCOL_SOFTWARE_RESET = 0x01 << 1;
        public const byte PROTOCOL_NON_DATA = 0x03 << 1;
        public const byte PROTOCOL_PIO_DATA_IN = 0x04 << 1;
        public const byte PROTOCOL_PIO_DATA_OUT = 0x05 << 1;
        public const byte PROTOCOL_DMA = 0x06 << 1;
        public const byte PROTOCOL_EXECUTE_DIAGNOSTIC = 0x08 << 1;
        public const byte PROTOCOL_NON_DATA_RESET = 0x09 << 1;
        public const byte PROTOCOL_UDMA_DATA_IN = 0x0A << 1;
        public const byte PROTOCOL_UDMA_DATA_OUT = 0x0B << 1;
        public const byte PROTOCOL_NCQ = 0x0C << 1;
        public const byte PROTOCOL_RETURN_RESPONSE = 0x0F << 1;

        public const byte SCSI_ATA_FLAGS = 0x02;
        public const byte ATA_DO_CK_COND = (0x01 << 5);
        public const byte ATA_T_TYPE_512BYTE_BLOCKS = (0x00 << 4);
        public const byte ATA_T_TYPE_SECTOR_BLOKS = (0x01 << 4);
        public const byte ATA_T_DIR_OUT = (0x00 << 3);
        public const byte ATA_T_DIR_IN = (0x01 << 3);
        public const byte ATA_IS_BYTE_BLOCK = (0x01 << 2);
        public const byte ATA_T_LENGTH_NONE = (0x00);
        public const byte ATA_T_LENGTH_FEATURES = (0x01);
        public const byte ATA_T_LENGTH_COUNT = (0x02);
        public const byte ATA_T_LENGTH_TPSIU = (0x03);

        public const byte SCSI_ATA12_FEATURES = 0x03;

        public const byte SCSI_ATA12_COUNT = 0x04;

        public const byte SCSI_ATA12_LBA_00_07 = 0x05;
        public const byte SCSI_ATA12_LBA_08_15 = 0x06;
        public const byte SCSI_ATA12_LBA_23_16 = 0x07;

        public const byte SCSI_ATA12_DEVICE = 0x08;

        public const byte SCSI_ATA12_COMMAND = 0x09;

        public const byte SCSI_ATA12_CONTROL = 0x0B;
        #endregion


        #region Structs used in unmanaged functions
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct ATA_PASS_THROUGH_EX
        {
            public ushort length;
            public ushort ataFlags;
            public byte pathId;
            public byte targetId;
            public byte lun;
            public byte reservedAsUchar;
            public uint dataTransferLength;
            public uint timeoutValue;
            public uint reservedAsUlong;
            public IntPtr dataBufferOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] previousTaskFile;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] currentTaskFile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SCSI_PASS_THROUGH
        {
            public ushort length;
            public byte scsiStatus;
            public byte pathId;
            public byte targetId;
            public byte lun;
            public byte cdbLength;
            public byte senseInfoLength;
            public byte dataIn;
            public uint dataTransferLength;
            public uint timeoutValue;
            public IntPtr dataBufferOffset;
            public uint senseInfoOffset;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SCSI_CBD_SIZE)]
            public byte[] cdb;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct GENERIC_ATA_COMMAND
        {
            public ATA_PASS_THROUGH_EX header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ATA_BLOCK_SIZE_WORDS)]
            public ushort[] result_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SECURITY_UNLOCK_ATA_QUERY
        {
            public ATA_PASS_THROUGH_EX header;
            public ushort identifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ATA_SECURITY_PASSWORD_SIZE)]
            public byte[] password;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ATA_RESERVED_SIZE_WORDS)]
            public ushort[] reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct GENERIC_SCSI_ATA_COMMAND
        {
            public SCSI_PASS_THROUGH header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SCSI_SENSE_SIZE_BYTES)]
            public byte[] sense_data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SCSI_BLOCK_SIZE_WORDS)]
            public ushort[] result_data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct SECURITY_UNLOCK_SCSI_ATA_COMMAND
        {
            public SCSI_PASS_THROUGH header;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SCSI_SENSE_SIZE_BYTES)]
            public byte[] sense_data;
            public ushort identifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ATA_SECURITY_PASSWORD_SIZE)]
            public byte[] password;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = SCSI_RESERVED_SIZE_WORDS)]
            public ushort[] reserved;
        }
        #endregion

        #region Unamanged function declarations
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(SafeFileHandle hHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static unsafe extern SafeFileHandle CreateFile(
            string FileName,
            uint DesiredAccess,
            uint ShareMode,
            IntPtr SecurityAttributes,
            uint CreationDisposition,
            uint FlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            ref GENERIC_ATA_COMMAND lpInBuffer,
            uint nInBufferSize,
            ref GENERIC_ATA_COMMAND lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            ref SECURITY_UNLOCK_ATA_QUERY lpInBuffer,
            uint nInBufferSize,
            ref SECURITY_UNLOCK_ATA_QUERY lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            ref GENERIC_SCSI_ATA_COMMAND lpInBuffer,
            uint nInBufferSize,
            ref GENERIC_SCSI_ATA_COMMAND lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            ref SECURITY_UNLOCK_SCSI_ATA_COMMAND lpInBuffer,
            uint nInBufferSize,
            ref SECURITY_UNLOCK_SCSI_ATA_COMMAND lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern bool FlushFileBuffers(
            SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool ReadFile(
            SafeFileHandle hFile,
            byte* pBuffer,
            uint NumberOfBytesToRead,
            uint* pNumberOfBytesRead,
            IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetFilePointerEx(
            SafeFileHandle hFile,
            ulong liDistanceToMove,
            out ulong lpNewFilePointer,
            uint dwMoveMethod);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool WriteFile(
            SafeFileHandle hFile,
            byte* pBuffer,
            uint NumberOfBytesToWrite,
            uint* pNumberOfBytesWritten,
            IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern unsafe bool GetFileSizeEx(
            SafeFileHandle handle,
            out ulong size);
        #endregion
    }
}
