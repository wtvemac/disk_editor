using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace disk_editor
{
    public class WebTVPartitionCollection : ObservableCollection<WebTVPartition>
    {
        public string get_loose_id(WebTVPartition part)
        {
            return part.sector_start.ToString() + part.name;
        }

        public void add_enumerated_partitions(partition_map partition_table, WebTVDisk disk)
        {
            int i = 0;

            var mounted_servers = new Dictionary<string, ImDiskNamedPipeServer>();

            for(i = 0; i < this.Count; i++)
            {
                if(this[i].has_device_attached())
                {
                    mounted_servers.Add(this.get_loose_id(this[i]), this[i].server);
                }
            }

            this.Clear();

            var partition_manager = new WebTVPartitionManager(disk);

            var sortable_parts = partition_manager.get_sorted_partition_map(partition_table);

            ulong disk_end_sector = (disk.size_bytes / disk.sector_bytes_length);

            ulong expected_next_sector_start = 0;
            WebTVPartition last_partition = null;
            for (i = 0; i < sortable_parts.Count; i++)
            {
                if (sortable_parts[i].name != "")
                {
                    ulong current_sector_start = BigEndianConverter.ToUInt32(sortable_parts[i].sector_start, 0);
                    ulong current_sector_length = BigEndianConverter.ToUInt32(sortable_parts[i].sector_length, 0);
                    ulong current_end_sector = (current_sector_start + current_sector_length);
                    ulong current_lost_sector_length = 0;
                    ulong current_found_sector_length = current_sector_length;
                    PartitionState current_partition_state = PartitionState.HEALTHY;

                    if (last_partition != null)
                    {
                        if (current_sector_start > expected_next_sector_start)
                        {
                            var unallocated_partition = new WebTVPartition()
                            {
                                name = "-",
                                type = PartitionType.UNALLOCATED,
                                sector_start = expected_next_sector_start,
                                sector_length = (current_sector_start - expected_next_sector_start),
                                disk = disk
                            };

                            this.Add(unallocated_partition);
                        }
                        else if (current_sector_start < expected_next_sector_start)
                        {
                            last_partition.state = PartitionState.OVERLAP_NEXT;
                            current_partition_state = PartitionState.OVERLAP_PREVIOUS;
                        }
                    }

                    if (current_sector_start < disk_end_sector && current_end_sector > disk_end_sector)
                    {
                        current_partition_state = PartitionState.SIZE_BEYOND_DISK_BOUND;
                        current_lost_sector_length = current_end_sector - disk_end_sector;
                        current_found_sector_length = current_sector_length - current_lost_sector_length;
                    }
                    else if (current_sector_start >= disk_end_sector)
                    {
                        current_partition_state = PartitionState.START_BEYOND_DISK_BOUND;
                        current_lost_sector_length = current_sector_length;
                        current_found_sector_length = 0;
                    }


                    if (current_sector_length == 0)
                    {
                        current_partition_state = PartitionState.BAD_SIZE;

                        current_sector_length = 1; 
                    }

                    expected_next_sector_start = current_sector_start + current_sector_length;

                    last_partition = new WebTVPartition()
                    {
                        name = sortable_parts[i].name,
                        type = (PartitionType)BigEndianConverter.ToUInt32(sortable_parts[i].type, 0),
                        state = current_partition_state,
                        sector_start = current_sector_start,
                        sector_length = current_sector_length,
                        found_sector_length = current_found_sector_length,
                        lost_sector_length = current_lost_sector_length,
                        disk = disk
                    };

                    var loose_id = this.get_loose_id(last_partition);

                    if (mounted_servers.ContainsKey(loose_id))
                    {
                        var mounted_server = mounted_servers[loose_id];

                        mounted_server.unmount_callback = last_partition.on_umount;
                        last_partition.server = mounted_server;

                        mounted_servers.Remove(loose_id);
                    }

                    if (last_partition.type == PartitionType.DELEGATED && disk.layout == DiskLayout.UTV)
                    {
                        if (current_partition_state == PartitionState.START_BEYOND_DISK_BOUND)
                        {
                            last_partition.delegate_filename = "unknown";
                            last_partition.delegated_type = PartitionType.UNKNOWN;
                        }
                        else
                        {
                            last_partition.delegate_filename = partition_manager.get_delegate_file(last_partition);
                            last_partition.delegated_type = partition_manager.get_delegated_type(last_partition.delegate_filename);
                        }
                    }
                    else
                    {
                        last_partition.delegated_type = last_partition.type;
                    }

                    this.Add(last_partition);
                }
            }

            foreach (KeyValuePair<string, ImDiskNamedPipeServer> mounted_server in mounted_servers)
            {
                try
                {
                    mounted_server.Value.unmount();
                }
                catch { }
            }
        }

        public int get_index(WebTVPartition part)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].id == part.id)
                {
                    return i;
                }
            }

            return -1;
        }

        public WebTVPartition get_partition_by_id(Guid id)
        {
            foreach (WebTVPartition part in this)
            {
                if (part.id == id)
                {
                    return part;
                }
            }

            return null;
        }
    }
}
