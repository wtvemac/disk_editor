using System;
using System.Runtime.Serialization;

namespace disk_editor
{

    [Serializable]
    public class ATAOperationException : Exception
    {
        public enum ATAError
        {
            NONE = 0,
            AMNF = 1,
            TKZNF = 2,
            ABRT = 4,
            MCR = 8,
            IDNF = 16,
            MC = 32,
            UN = 64,
            BBK = 128
        };

        public ATAError ata_error_code;

        public ATAOperationException()
        {
            this.ata_error_code = ATAError.NONE;
        }

        public ATAOperationException(string message)
            : base(message)
        {
            this.ata_error_code = ATAError.NONE;
        }

        public ATAOperationException(string message, Exception inner)
            : base(message, inner)
        {
            this.ata_error_code = ATAError.NONE;
        }

        public ATAOperationException(ATAError ata_error_code)
            : base("ATA Error code=0x" + ata_error_code.ToString("X").PadLeft(2, '0'))
        {
            this.ata_error_code = ata_error_code;
        }

        public ATAOperationException(byte ata_error_code)
            : base("ATA Error code=0x" + ata_error_code.ToString("X").PadLeft(2, '0'))
        {
            this.ata_error_code = (ATAError)ata_error_code;
        }
    }
}