using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace disk_editor
{
    public enum PartitionState
    {
        HEALTHY = 0,
        OVERLAP_PREVIOUS = 1,
        OVERLAP_NEXT = 2,
        BAD_SIZE = 3,
        BROKEN = 4,
        SIZE_BEYOND_DISK_BOUND = 5,
        START_BEYOND_DISK_BOUND = 6,
    };
    public enum PartitionType
    {
        UNALLOCATED = -1,
        FREE = 0,
        NONE = 0,
        ONE = 1, // Doesn't seem to be used?
        FAT16 = 2,
        RAW = 3, // Boot or approm
        RAW2 = 4, // Boot or approm on the echostar
        DVRFS = 5,
        DELEGATED = 5,
        _DELEGATED = 50, // used to differentiate from DVRFS for UI elements but needs to be translated back.
        UNKNOWN = 255
    };

    public class WebTVPartition : INotifyPropertyChanged
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public PartitionType type { get; set; }
        public PartitionState state { get; set; }
        public ulong sector_start { get; set; }
        public ulong sector_length { get; set; }
        public ulong lost_sector_length { get; set; }
        public ulong found_sector_length { get; set; }
        public string delegate_filename { get; set; }
        public PartitionType delegated_type { get; set; }
        public WebTVDisk disk { get; set; }

        public bool _is_mounted { get; set; }
        public bool is_mounted
        {
            get
            {
                return this._is_mounted;
            }

            set
            {
                if (this._is_mounted != value)
                {
                    this._is_mounted = value;

                    RaisePropertyChanged("is_mounted");
                }
            }
        }

        private ImDiskNamedPipeServer _server { get; set; }
        public ImDiskNamedPipeServer server 
        {
            get
            {
                return this._server;
            }

            set
            {
                if (this._server != value)
                {
                    this._server = value;

                    RaisePropertyChanged("server");
                }
            }
        }

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

        public void mount(string mount_point, bool read_only = false, ImDiskNamedPipeServer.notify_callback mounted_callback = null)
        { 
            if(!this.has_device_attached())
            {
                this.server = new ImDiskNamedPipeServer(this, mount_point, read_only, this.on_umount, mounted_callback);
                this.is_mounted = true;
            }
            else
            {
                throw new ApplicationException("Partition is already mounted!");
            }
        }

        public void on_umount(bool success = true, string message = "")
        {
            this.server = null;
            this.is_mounted = false;
        }

        public void unmount()
        {
            if (this.has_device_attached())
            {
                this.server.unmount();
            }
        }

        public bool has_device_attached()
        {
            return (this.server != null);
        }

        public DiskCageBounds get_object_bounds()
        {
            DiskCageBounds cage_bounds;

            cage_bounds.start_position = (this.sector_start) * this.disk.sector_bytes_length;
            cage_bounds.end_position = (this.sector_start + this.sector_length) * this.disk.sector_bytes_length;

            return cage_bounds;
        }

        public WebTVPartition()
        { 
            id = Guid.NewGuid();
        }
    }
}
