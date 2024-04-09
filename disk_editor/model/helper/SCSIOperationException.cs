using System;
using System.Runtime.Serialization;

namespace disk_editor
{

    [Serializable]
    public class SCSIOperationException : Exception
    {
        public enum SCSIStatus
        {
            GOOD = 0,
            CHECK_CONDITION = 2,
            CONDITION_MET = 4,
            BUSY = 8,
            INTERMEDIATE = 16,
            INTERMEDIATE_CONDITION_MET = 20,
            RESERVATION_CONFLICT = 24,
            COMMAND_TERMINATED = 34,
            TASK_SET_FULL = 40,
            ACA_ACTIVE = 48,
            TASK_ABORTED = 64,
        };

        public byte scsi_status;
        public byte sense_key;
        public byte asc;
        public byte ascq;

        public SCSIOperationException()
        {
            this.scsi_status = 0x00;
            this.sense_key = 0x00;
            this.asc = 0x00;
            this.ascq = 0x00;
        }

        public SCSIOperationException(string message)
            : base(message)
        {
            this.scsi_status = 0x00;
            this.sense_key = 0x00;
            this.asc = 0x00;
            this.ascq = 0x00;
        }

        public SCSIOperationException(string message, Exception inner)
            : base(message, inner)
        {
            this.scsi_status = 0x00;
            this.sense_key = 0x00;
            this.asc = 0x00;
            this.ascq = 0x00;
        }

        public SCSIOperationException(byte scsi_status, byte sense_key, byte asc, byte ascq)
            : base("SCSI Error status=0x" + scsi_status.ToString("X").PadLeft(2, '0') + ", key=0x" + sense_key.ToString("X").PadLeft(2, '0') + ", asc/ascq=" + asc.ToString("X").PadLeft(2, '0') + "h/" + ascq.ToString("X").PadLeft(2, '0') + "h")
        {
            this.scsi_status = scsi_status;
            this.sense_key = sense_key;
            this.asc = asc;
            this.ascq = ascq;
        }

        public SCSIOperationException(byte scsi_status)
            : base("SCSI Error status=0x" + scsi_status.ToString("X").PadLeft(2, '0'))
        {
            this.scsi_status = scsi_status;
            this.sense_key = 0x00;
            this.asc = 0x00;
            this.ascq = 0x00;

        }
    }
}