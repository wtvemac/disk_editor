using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace disk_editor
{
    public enum DiskType
    {
        PHYSICAL = 0,
        IMAGE = 1
    };

    public enum DiskState
    {
        HEALTHY = 0,
        NO_PARTITION_TABLE = 1,
        INVALID_PARTITION_MAP_CHECKSUM = 2,
        NO_PARTITIONS = 3,
        BROKEN = 4,
        ODD_TRANSFORM = 5
    };

    public enum DiskLayout
    {
        PLAIN = 0,
        LC2 = 1,
        WEBSTAR = 2,
        UTV = 4
    };

    public class WebTVDisk : INotifyPropertyChanged
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public DiskType type { get; set; }
        public DiskState state { get; set; }
        public DiskByteTransform byte_transform { get; set; }
        public DiskLayout layout { get; set; }
        public ulong size_bytes { get; set; }
        public uint sector_bytes_length { get; set; }
        public WebTVDiskIO io { get; set; }
        public WebTVPartitionCollection partition_table { get; set; }

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

        public WebTVDisk()
        {
            id = Guid.NewGuid();

            partition_table = new WebTVPartitionCollection();
        }

        ~WebTVDisk()
        {
            this.stop_io();
        }

        public DiskCageBounds get_object_bounds()
        {
            DiskCageBounds cage_bounds;

            cage_bounds.start_position = 0;
            cage_bounds.end_position = (ulong)this.io.Length;

            return cage_bounds;
        }

        public void enumerate_partitions(WebTVPartitionManager manager)
        {
            if (this.io != null)
            {
                this.partition_table.add_enumerated_partitions(manager.enumerate_partitions(), this);
            }
        }

        public void enumerate_partitions()
        {
            this.enumerate_partitions(new WebTVPartitionManager(this));
        }

        public void detect_collation(WebTVDiskCollationConverter byte_converter)
        {
            var partition_table_offsets = WebTVPartitionManager.possible_partition_table_offsets();

            foreach (var partition_table_offset in partition_table_offsets)
            {
                var partition_manager = new WebTVPartitionManager(this);

                if (partition_manager.is_valid_partition_table(partition_table_offset))
                {
                    var collation = byte_converter.detect_partition_table_byte_transform(this.io, partition_table_offset);

                    this.set_disk_collation(collation);

                    break;
                }
            }
        }

        public void detect_layout(WebTVPartitionManager partition_manager)
        {
            this.layout = partition_manager.detect_disk_layout();
        }

        public void start_io(WebTVDiskIO io)
        {
            if (this.io != null)
            {
                this.stop_io();
            }

            this.io = io;

        }

        public void stop_io()
        {
            if (this.io != null)
            {
                this.io.Close();
                this.io.Dispose();

                this.io = null;
            }
        }

        public void clear_partitions()
        { 
            if(this.partition_table != null)
            {
                foreach (var part in this.partition_table)
                {
                    part.unmount();
                }

                this.partition_table.Clear();
            }
        }

        public void set_disk_collation(DiskByteTransform byte_transform)
        {
            this.byte_transform = byte_transform;

            if (this.io != null && this.io.byte_converter != null)
            {
                this.io.byte_converter.byte_transform = byte_transform;
            }
        }

        public void set_disk_layout(DiskLayout layout)
        {
            this.layout = layout;
        }

        public void update_disk_state()
        {
            this.state = DiskState.HEALTHY;

            var partition_manager = new WebTVPartitionManager(this);

            var partition_map_data = partition_manager.read_partition_table();

            if (partition_manager.has_valid_partition_map(partition_map_data))
            {
                if (partition_manager.has_valid_checksum(partition_map_data))
                {
                    this.state = DiskState.INVALID_PARTITION_MAP_CHECKSUM;
                }

                this.enumerate_partitions();

                if(this.state == DiskState.HEALTHY)
                {
                    if (this.partition_table.Count == 0)
                    {
                        this.state = DiskState.NO_PARTITIONS;
                    }
                    else if (this.byte_transform == DiskByteTransform.NOSWAP || this.byte_transform == DiskByteTransform.BIT32SWAP)
                    {
                        this.state = DiskState.ODD_TRANSFORM;
                    }
                    else if (this.byte_transform != DiskByteTransform.BIT1632SWAP && this.layout == DiskLayout.UTV)
                    {
                        this.state = DiskState.ODD_TRANSFORM;
                    }
                    else if (this.byte_transform != DiskByteTransform.BIT16SWAP && this.layout == DiskLayout.WEBSTAR)
                    {
                        this.state = DiskState.ODD_TRANSFORM;
                    }
                    else if (this.byte_transform != DiskByteTransform.BIT16SWAP && this.layout == DiskLayout.LC2)
                    {
                        this.state = DiskState.ODD_TRANSFORM;
                    }
                }
            }
            else
            {
                this.state = DiskState.NO_PARTITION_TABLE;
            }
        }
    }
}
