using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using DiscUtils;
using DiscUtils.Fat;
using System.Xml.Linq;

namespace disk_editor
{

    [StructLayout(LayoutKind.Sequential, Size = WebTVPartitionManager.PARTITION_ENTRY_SIZE), Serializable]
    public unsafe struct partition_entry
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] sector_start;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] sector_length;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] type;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown1;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown2;
    }

    [StructLayout(LayoutKind.Sequential, Size = WebTVPartitionManager.PARTITION_TABLE_SIZE), Serializable]
    public unsafe struct partition_map
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] checksum;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] partition_count;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] magic;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = WebTVPartitionManager.MAX_PARTITION_COUNT)]
        public partition_entry[] partitions;
    }

    [StructLayout(LayoutKind.Sequential, Size = WebTVPartitionManager.PARTITION_SECTOR_SIZE), Serializable]
    public unsafe struct partition_sector
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] checksum;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] magic;

        [MarshalAsAttribute(UnmanagedType.Struct, SizeConst = WebTVPartitionManager.PARTITION_ENTRY_SIZE)]
        public partition_entry partition;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] unknown1;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] delegate_filename;
    }

    public class WebTVPartitionManager
    {
        // + 2MB of the absolute minimum to account for the +0x4000 shift WebTV has.

        public const ulong MINIMUM_DISK_SIZE = 500 * 1024 * 1024;
        public const ulong MINIMUM_BOOT_PARTITION_SIZE = 2 * 1024 * 1024;
        public const ulong MINIMUM_FAT16_PARTITION_SIZE = 2 * 1024 * 1024;
        public const ulong MINIMUM_DVRFS_PARTITION_SIZE = 2 * 1024 * 1024;
        public const ulong MAXIMUM_BOOT_PARTITION_SIZE = 1 * 1024 * 1024 * 1024;
        public const ulong MAXIMUM_FAT16_PARTITION_SIZE = 512 * 1024 * 1024;
        public const ulong MAXIMUM_DVRFS_PARTITION_SIZE = (ulong)1 * 1024 * 1024 * 1024 * 1024;
        public const ulong PARTITON_DATA_OFFSET = 0x4000;
        public const ulong PARTITION_TABLE_MAGIC = 0x74696D6E; // timn
        public const ulong PARTITION_TABLE_LC2_OFFSET = 0x14C1000;
        public const ulong PARTITION_TABLE_WEBSTAR_OFFSET = 0x14C1000;
        public const uint PARTITION_TABLE_UTV_OFFSET = 0x178C1000;
        public const int PARTITION_TABLE_HEADER_SIZE = 0xC;
        public const int PARTITION_TABLE_SIZE = 0x800;
        public const int PARTITION_ENTRY_SIZE = 0x34;
        public const int MAX_PARTITION_COUNT = 39;

        public const uint PARTITION_SECTOR_MAGIC = 0x70617274; // part
        public const int PARTITION_SECTOR_SIZE = 0x200;

        public const ulong LC2_FIRST_PARTITION_SIZE = 0x14C3200;// 0x1E00000;
        public const ulong WEBSTAR_FIRST_PARTITION_SIZE = 0x1E00000;
        public const ulong WEBSTAR_SECOND_PARTITION_SIZE = 0x4600000;
        public const ulong UTV_FIRST_PARTITION_SIZE = 0x178C3000;

        public WebTVDisk disk { get; set; }

        public WebTVPartitionManager(WebTVDisk disk)
        {
            this.disk = disk;
        }

        public static ulong[] possible_partition_table_offsets()
        {
            ulong[] offsets = { WebTVPartitionManager.PARTITION_TABLE_LC2_OFFSET, 
                               WebTVPartitionManager.PARTITION_TABLE_WEBSTAR_OFFSET, 
                               WebTVPartitionManager.PARTITION_TABLE_UTV_OFFSET };

            return offsets;
        }

        public static ulong get_partition_table_offset(WebTVDisk disk)
        {
            if (disk != null)
            {
                if (disk.layout == DiskLayout.UTV)
                {
                    return WebTVPartitionManager.PARTITION_TABLE_UTV_OFFSET;
                }
                else if (disk.layout == DiskLayout.WEBSTAR)
                {
                    return WebTVPartitionManager.PARTITION_TABLE_WEBSTAR_OFFSET;
                }
            }

            return WebTVPartitionManager.PARTITION_TABLE_LC2_OFFSET;
        }

        public bool is_valid_partition_table(ulong partition_table_offset = WebTVPartitionManager.PARTITION_TABLE_LC2_OFFSET)
        {
            try
            {
                if (this.disk.io != null)
                {
                    byte[] partition_table_test_data = new byte[4];
                    this.disk.io.AbsRead(partition_table_test_data, partition_table_offset + 0x8, 0, 4);

                    // Doing this instead of BitConverter to reduce problems with endianness.
                    uint partition_table_test_uint = BigEndianConverter.ToUInt32(partition_table_test_data, 0);

                    // This tests to see if there's a partition table magic.
                    // The WebTV partition table magic is "timn"
                    if (partition_table_test_uint == 0x74696D6E // timn (no swapping)
                    || partition_table_test_uint == 0x69746E6D  // itnm (16-bit swap)
                    || partition_table_test_uint == 0x6D6E7469  // mnti (16+32-bit swap)
                    || partition_table_test_uint == 0x6E6D6974)  // nmit (32-bit swap)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool is_valid_partition_table()
        {
            if (this.disk.io != null)
            {
                return this.is_valid_partition_table(this.get_partition_table_offset());
            }

            return false;
        }

        public DiskLayout detect_disk_layout()
        {
            if (this.disk.io != null)
            {
                if (this.is_valid_partition_table(WebTVPartitionManager.PARTITION_TABLE_UTV_OFFSET))
                {
                    return DiskLayout.UTV;
                }
                else if (this.is_valid_partition_table(WebTVPartitionManager.PARTITION_TABLE_WEBSTAR_OFFSET))
                {
                    var parts = this.enumerate_partitions();

                    var partition_count = BigEndianConverter.ToUInt32(parts.partition_count, 0);

                    if (partition_count > 2)
                    {
                        var partition_name = parts.partitions[1].name;
                        var partition_type = (PartitionType)BigEndianConverter.ToUInt32(parts.partitions[1].type, 0);

                        if (partition_type == PartitionType.RAW2)
                        {
                            return DiskLayout.WEBSTAR;
                        }
                    }
                }

            }

            return DiskLayout.LC2;
        }

        public ulong get_partition_table_offset()
        {
            return WebTVPartitionManager.get_partition_table_offset(this.disk);
        }

        public byte[] read_partition_table()
        {
            var partition_map_data = new byte[WebTVPartitionManager.PARTITION_TABLE_SIZE];

            if (this.disk.io != null)
            {
                this.disk.io.AbsRead(partition_map_data, this.get_partition_table_offset(), 0, WebTVPartitionManager.PARTITION_TABLE_SIZE);
            }

            return partition_map_data;
        }


        public bool has_valid_partition_map()
        {
            var partition_map_data = this.read_partition_table();

            return this.has_valid_partition_map(partition_map_data);
        }

        public bool has_valid_partition_map(byte[] partition_map_data)
        {
            var stored_magic = BigEndianConverter.ToUInt32(partition_map_data, 0x8);

            return (stored_magic == WebTVPartitionManager.PARTITION_TABLE_MAGIC);
        }

        public bool has_valid_checksum()
        {
            var partition_map_data = this.read_partition_table();

            return this.has_valid_checksum(partition_map_data);
        }

        public bool has_valid_checksum(byte[] partition_map_data)
        {
            var stored_checksum = BigEndianConverter.ToUInt32(partition_map_data, 0);
            var partition_count = BigEndianConverter.ToUInt32(partition_map_data, 4);

            var actual_checksum = calculate_checksum(partition_map_data, this.partition_table_length(partition_count));

            return (stored_checksum != actual_checksum);
        }

        public uint calculate_checksum(byte[] partition_map_data, uint checksum_length = 0)
        {
            if (checksum_length == 0)
            {
                checksum_length = (uint)partition_map_data.Length;
            }

            uint checksum = 0;

            for (int i = 4; i < Math.Min(checksum_length, partition_map_data.Length); i++)
            {
                checksum += partition_map_data[i];
            }

            return checksum;
        }

        public byte[] partition_table_to_data()
        {
            return this.partition_table_to_data(this.disk.partition_table);
        }

        public partition_entry to_partition_entry(WebTVPartition part)
        {
            var _partition_entry = new partition_entry()
            {
                name = part.name,
                sector_start = BigEndianConverter.GetBytes((uint)part.sector_start),
                sector_length = BigEndianConverter.GetBytes((uint)part.sector_length),
                type = BigEndianConverter.GetBytes((uint)part.type)
            };

            return _partition_entry;
        }

        public byte[] partition_table_to_data(WebTVPartitionCollection parts)
        {
            var partition_count = (uint)parts.Count;

            var partition_map_data = new byte[WebTVPartitionManager.PARTITION_TABLE_SIZE];

            var blank_partition_map = this.enumerate_partitions(partition_map_data);
            blank_partition_map.partition_count = BigEndianConverter.GetBytes(partition_count);
            blank_partition_map.magic = BigEndianConverter.GetBytes((uint)WebTVPartitionManager.PARTITION_TABLE_MAGIC);

            for (int i = 0, j = 0; i < Math.Min(partition_count, WebTVPartitionManager.MAX_PARTITION_COUNT); i++)
            {
                if (parts[i].type != PartitionType.UNALLOCATED)
                {
                    blank_partition_map.partitions[j].name = parts[i].name;
                    blank_partition_map.partitions[j].sector_start = BigEndianConverter.GetBytes((uint)parts[i].sector_start);
                    blank_partition_map.partitions[j].sector_length = BigEndianConverter.GetBytes((uint)parts[i].sector_length);
                    blank_partition_map.partitions[j].type = BigEndianConverter.GetBytes((uint)parts[i].type);

                    j++;
                }
            }

            var new_partition_map_data = this.package_partitions(blank_partition_map);

            return update_table_checksum(new_partition_map_data, this.partition_table_length(blank_partition_map));
        }

        public byte[] partition_table_to_data(List<partition_entry> partition_table)
        {
            var partition_count = (uint)partition_table.Count;

            var partition_map_data = new byte[WebTVPartitionManager.PARTITION_TABLE_SIZE];

            var blank_partition_map = this.enumerate_partitions(partition_map_data);
            blank_partition_map.partition_count = BigEndianConverter.GetBytes(partition_count);
            blank_partition_map.magic = BigEndianConverter.GetBytes((uint)WebTVPartitionManager.PARTITION_TABLE_MAGIC);

            for (int i = 0; i < Math.Min(partition_count, WebTVPartitionManager.MAX_PARTITION_COUNT); i++)
            {
                blank_partition_map.partitions[i].name = partition_table[i].name;
                blank_partition_map.partitions[i].sector_start = partition_table[i].sector_start;
                blank_partition_map.partitions[i].sector_length = partition_table[i].sector_length;
                blank_partition_map.partitions[i].type = partition_table[i].type;
            }

            var new_partition_map_data = this.package_partitions(blank_partition_map);

            return update_table_checksum(new_partition_map_data, this.partition_table_length(blank_partition_map));
        }

        public uint partition_table_length(partition_map _partition_map)
        {
            var partition_count = BigEndianConverter.ToUInt32(_partition_map.partition_count, 0);

            return this.partition_table_length(partition_count);
        }

        public uint partition_table_length(uint partition_count)
        {
            return WebTVPartitionManager.PARTITION_TABLE_HEADER_SIZE + (partition_count * WebTVPartitionManager.PARTITION_ENTRY_SIZE);
        }

        public byte[] update_table_checksum(byte[] partition_map_data, uint checksum_length = 0)
        {
            var checksum = calculate_checksum(partition_map_data, checksum_length);

            System.Buffer.BlockCopy(BigEndianConverter.GetBytes((uint)checksum), 0, partition_map_data, 0, 4);

            return partition_map_data;
        }

        public partition_map enumerate_partitions()
        {
            var partition_map_data = this.read_partition_table();

            return this.enumerate_partitions(partition_map_data);
        }

        public byte[] package_partitions(partition_map _partition_map)
        {
            int partition_table_size = Marshal.SizeOf(_partition_map);

            var partition_map_data = new byte[partition_table_size];

            var partition_map_pointer = Marshal.AllocHGlobal(partition_table_size);
            Marshal.StructureToPtr(_partition_map, partition_map_pointer, true);
            Marshal.Copy(partition_map_pointer, partition_map_data, 0, partition_table_size);
            Marshal.FreeHGlobal(partition_map_pointer);

            return partition_map_data;
        }

        public partition_map enumerate_partitions(byte[] partition_map_data)
        {
            GCHandle partition_map_handle = new GCHandle();

            try
            {
                partition_map_handle = GCHandle.Alloc(partition_map_data, GCHandleType.Pinned);
                var partition_map_pointer = partition_map_handle.AddrOfPinnedObject();

                return (partition_map)Marshal.PtrToStructure(partition_map_pointer, typeof(partition_map));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse WebTV partition table. " + e.Message);
            }
            finally
            {
                if (partition_map_handle.IsAllocated)
                {
                    partition_map_handle.Free();
                }
            }

        }

        public List<partition_entry> get_partition_list(partition_map partition_table)
        {
            var partition_list = new List<partition_entry>();

            var partition_count = BigEndianConverter.ToUInt32(partition_table.partition_count, 0);

            for (int i = 0; i < Math.Min(partition_count, partition_table.partitions.Length); i++)
            {
                if (partition_table.partitions[i].name == "")
                {
                    break;
                }

                partition_list.Add(partition_table.partitions[i]);
            }

            return partition_list;
        }

        public List<partition_entry> get_sorted_partition_map(List<partition_entry> partition_table)
        {
            partition_table.Sort(delegate(partition_entry a, partition_entry b)
            {
                var a_val = BigEndianConverter.ToUInt32(a.sector_start, 0);
                var b_val = BigEndianConverter.ToUInt32(b.sector_start, 0);

                if (a_val > b_val)
                {
                    return 1;
                }
                else if (a_val < b_val)
                {
                    return -1;
                }

                return 0;
            });

            return partition_table;
        }

        public List<partition_entry> get_sorted_partition_map(partition_map partition_table)
        {
            var sortable_parts = get_partition_list(partition_table);

            return this.get_sorted_partition_map(sortable_parts);
        }

        public void write_partition_table(byte[] partition_map_data)
        {
            this.disk.io.AbsWrite(partition_map_data, this.get_partition_table_offset(), 0, (uint)partition_map_data.Length);
        }

        public void delete_partition(WebTVPartition part)
        {
            part.unmount();

            part.name = "free";
            part.type = PartitionType.FREE;

            this.write_partition_table(this.partition_table_to_data());
        }

        public ulong smallest_available_create_size(WebTVPartitionCollection parts)
        {
            var free_regions = get_free_regions(parts);

            ulong min_free_space = UInt64.MaxValue;

            for (var i = 0; i < free_regions.Count; i++)
            {
                min_free_space = Math.Min(min_free_space, this.sum_sector_length(free_regions[i]) * this.disk.sector_bytes_length);
            }

            return min_free_space;
        }

        public ulong smallest_available_create_size()
        {
            return this.smallest_available_create_size(this.disk.partition_table);
        }

        public ulong largest_available_create_size(WebTVPartitionCollection parts)
        {
            var free_regions = get_free_regions(parts);

            ulong max_free_space = 0;

            for (var i = 0; i < free_regions.Count; i++)
            {
                max_free_space = Math.Max(max_free_space, this.sum_sector_length(free_regions[i]) * this.disk.sector_bytes_length);
            }

            return max_free_space;
        }

        public ulong largest_available_create_size()
        {
            return this.largest_available_create_size(this.disk.partition_table);
        }

        public ulong bytes_to_sectors(ulong size_in_bytes)
        {
            var alignment_error = (size_in_bytes % (ulong)this.disk.sector_bytes_length);
            var size_in_sectors = (ulong)0;

            if (alignment_error > 0)
            {
                size_in_sectors = (size_in_bytes + (this.disk.sector_bytes_length - (uint)alignment_error));
            }
            else
            {
                size_in_sectors = size_in_bytes;
            }

            size_in_sectors = (size_in_sectors / this.disk.sector_bytes_length);

            return size_in_sectors;
        }

        public ulong sum_sector_length(List<partition_entry> partition_list, int start_index = 0)
        {
            ulong sector_length = 0;

            start_index = Math.Min(start_index, (partition_list.Count - 1));

            for (int i = start_index; i < partition_list.Count; i++)
            {
                sector_length += BigEndianConverter.ToUInt32(partition_list[i].sector_length, 0);
            }

            return sector_length;
        }

        public List<List<partition_entry>> get_free_regions(WebTVPartitionCollection parts)
        {
            var partition_table = new List<partition_entry>();

            for (int i = 0; i < parts.Count; i++)
            {
                var partition_type = parts[i].type;

                if (parts[i].state == PartitionState.HEALTHY)
                {
                    partition_table.Add(this.to_partition_entry(parts[i]));
                }
            }

            return this.get_free_regions(partition_table);
        }

        public List<List<partition_entry>> get_free_regions(List<partition_entry> partition_table)
        {
            var free_regions = new List<List<partition_entry>>();

            partition_table = this.get_sorted_partition_map(partition_table);

            ulong disk_end_sector = (this.disk.size_bytes / this.disk.sector_bytes_length);
            
            ulong expected_next_sector_start = 0;

            for (int i = 0, j = -1, last_found_i = 0; i < partition_table.Count; i++)
            {
                var partition_type = (PartitionType)BigEndianConverter.ToUInt32(partition_table[i].type, 0);
                ulong sector_start = BigEndianConverter.ToUInt32(partition_table[i].sector_start, 0);
                ulong sector_length = BigEndianConverter.ToUInt32(partition_table[i].sector_length, 0);

                Action<partition_entry> add_free_region = delegate(partition_entry part)
                {
                    if (last_found_i != (i - 1) || j == -1)
                    {
                        j++;

                        free_regions.Add(new List<partition_entry>());
                    }

                    last_found_i = i;

                    free_regions[j].Add(part);
                };

                if(sector_start < disk_end_sector)
                {
                    if (sector_start > expected_next_sector_start)
                    {
                        var unallocated_part = new partition_entry()
                        {
                            name = "-",
                            sector_start = BigEndianConverter.GetBytes((uint)expected_next_sector_start),
                            sector_length = BigEndianConverter.GetBytes((uint)(sector_start - expected_next_sector_start)),
                        };

                        add_free_region.Invoke(unallocated_part);
                    }

                    if (partition_type == PartitionType.FREE || partition_type == PartitionType.UNALLOCATED)
                    {
                        add_free_region.Invoke(partition_table[i]);
                    }
                }

                expected_next_sector_start = sector_start + sector_length;
            }

            return free_regions;
        }

        public int get_partition_list_index(List<partition_entry> partition_table, WebTVPartition part)
        {
            if (part != null)
            {
                for (int i = 0; i < partition_table.Count; i++)
                {
                    if (BigEndianConverter.ToUInt32(partition_table[i].sector_start, 0) == part.sector_start)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public List<partition_entry> get_available_free_region(List<partition_entry> partition_table, WebTVPartition center_part, ulong size_in_sectors)
        {
            var free_regions = this.get_free_regions(partition_table);

            var selected_free_region_index = -1;
            var available_free_region_indexes = new List<int>();

            var list_index = -1;

            for (int i = 0; i < free_regions.Count; i++)
            {
                var sector_length = this.sum_sector_length(free_regions[i]);

                if (sector_length >= size_in_sectors)
                {
                    available_free_region_indexes.Add(i);

                    list_index = get_partition_list_index(free_regions[i], center_part);

                    if (list_index >= 0)
                    {
                        selected_free_region_index = i;
                        break;
                    }
                }
            }

            if (selected_free_region_index == -1 && available_free_region_indexes.Count > 0)
            {
                selected_free_region_index = available_free_region_indexes[0];
            }

            if (selected_free_region_index > -1)
            {
                if(list_index > 0)
                {
                    var indexed_sector_length = this.sum_sector_length(free_regions[selected_free_region_index], list_index);

                    if (indexed_sector_length >= size_in_sectors)
                    {
                        free_regions[selected_free_region_index].RemoveRange(0, list_index);
                    }
                }

                return free_regions[selected_free_region_index];
            }
            else
            {
                return null;
            }
        }

        public ulong get_maximum_size(PartitionType type)
        {
            if (type == PartitionType.FAT16)
            {
                return WebTVPartitionManager.MAXIMUM_FAT16_PARTITION_SIZE;
            }
            else if (type == PartitionType.DVRFS || type == PartitionType.DELEGATED || type == PartitionType._DELEGATED)
            {
                return WebTVPartitionManager.MAXIMUM_DVRFS_PARTITION_SIZE;
            }
            else
            {
                return WebTVPartitionManager.MAXIMUM_BOOT_PARTITION_SIZE;
            }
        }

        public ulong get_minimum_size(PartitionType type)
        {
            if (type == PartitionType.FAT16)
            {
                return WebTVPartitionManager.MINIMUM_FAT16_PARTITION_SIZE;
            }
            else if (type == PartitionType.DVRFS || type == PartitionType.DELEGATED || type == PartitionType._DELEGATED)
            {
                return WebTVPartitionManager.MINIMUM_DVRFS_PARTITION_SIZE;
            }
            else
            {
                return WebTVPartitionManager.MINIMUM_BOOT_PARTITION_SIZE;
            }
        }

        public byte[] package_partition_sector(partition_sector _partition_sector)
        {
            int partition_sector_size = Marshal.SizeOf(_partition_sector);

            var partition_sector_data = new byte[partition_sector_size];

            var partition_map_pointer = Marshal.AllocHGlobal(partition_sector_size);
            Marshal.StructureToPtr(_partition_sector, partition_map_pointer, true);
            Marshal.Copy(partition_map_pointer, partition_sector_data, 0, partition_sector_size);
            Marshal.FreeHGlobal(partition_map_pointer);

            return partition_sector_data;
        }

        public byte[] read_partition_sector(WebTVPartition part)
        {
            var partition_sector_data = new byte[WebTVPartitionManager.PARTITION_SECTOR_SIZE];

            if (this.disk.io != null)
            {
                
                this.disk.io.AbsRead(partition_sector_data, (part.sector_start * this.disk.sector_bytes_length), 0, WebTVPartitionManager.PARTITION_SECTOR_SIZE);
            }

            return partition_sector_data;
        }

        public string get_delegate_file(WebTVPartition part)
        {
            var partition_sector_data = this.read_partition_sector(part);

            GCHandle partition_sector_handle = new GCHandle();

            try
            {
                partition_sector_handle = GCHandle.Alloc(partition_sector_data, GCHandleType.Pinned);
                var partition_sector_pointer = partition_sector_handle.AddrOfPinnedObject();

                var _partition_sector = (partition_sector)Marshal.PtrToStructure(partition_sector_pointer, typeof(partition_sector));

                var delegate_filename = Encoding.Default.GetString(Encoding.Convert(Encoding.Unicode, Encoding.Default, _partition_sector.delegate_filename));

                if (delegate_filename.IndexOf("\0") > 0)
                {
                    delegate_filename = delegate_filename.Remove(delegate_filename.IndexOf("\0"));
                }

                return delegate_filename;
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse WebTV partition sector. " + e.Message);
            }
            finally
            {
                if (partition_sector_handle.IsAllocated)
                {
                    partition_sector_handle.Free();
                }
            }
        }

        public PartitionType get_delegated_type(WebTVPartition part)
        {
            return this.get_delegated_type(this.get_delegate_file(part));
        }

        public PartitionType get_delegated_type(string delegated_file)
        {
            if (delegated_file == "DVRFsd.dll")
            {
                return PartitionType.DVRFS;
            }
            else if (delegated_file == "fatfs.dll")
            {
                return PartitionType.FAT16;
            }
            else
            {
                return PartitionType.UNKNOWN;
            }
        }

        public void initialize_partition(partition_entry partition, string requested_delegate = "")
        {
            var partition_type = (PartitionType)BigEndianConverter.ToUInt32(partition.type, 0);
            ulong sector_start = BigEndianConverter.ToUInt32(partition.sector_start, 0);
            ulong partition_offset = sector_start * this.disk.sector_bytes_length;
            ulong sector_length = BigEndianConverter.ToUInt32(partition.sector_length, 0);
            ulong partition_length = sector_length * this.disk.sector_bytes_length;

            if(requested_delegate == null)
            {
                requested_delegate = "";
            }

            if (requested_delegate.Length > 255)
            {
                requested_delegate = requested_delegate.Substring(0, 255);
            }

            var delegate_filename = new byte[512];
            var _delegate_filename = Encoding.Convert(Encoding.Default, Encoding.Unicode, Encoding.Default.GetBytes(requested_delegate));
            _delegate_filename.CopyTo(delegate_filename, 0);
            //Console.WriteLine("requested_delegate: " + requested_delegate);
            //Console.WriteLine(BitConverter.ToString(delegate_filename).Replace("-", string.Empty));

            var _partition_sector = new partition_sector()
            {
                checksum = BigEndianConverter.GetBytes((uint)0),
                magic = BigEndianConverter.GetBytes((uint)WebTVPartitionManager.PARTITION_SECTOR_MAGIC),
                partition = partition,
                unknown1 = new byte[32],
                delegate_filename = delegate_filename
            };

            var partition_sector_data = update_table_checksum(this.package_partition_sector(_partition_sector));

            this.disk.io.AbsWrite(partition_sector_data, partition_offset, 0, (uint)partition_sector_data.Length);

            if (partition_type == PartitionType.FAT16 || requested_delegate == "fatfs.dll")
            {
                var fat_type = FatType.Fat16;
                
                var cio = new WebTVDiskCagedIO(this.disk.io, partition_offset + WebTVPartitionManager.PARTITON_DATA_OFFSET, partition_offset + partition_length);

                var disk_geometry = new Geometry((long)this.disk.size_bytes, 1, 1, (int)this.disk.sector_bytes_length);

                FatFileSystem.FormatPartition(cio, partition.name, disk_geometry, (int)0, (int)this.bytes_to_sectors(partition_length - 0x4000), 0, fat_type);
            }
        }

        public void write_partition_sectors(WebTVPartitionCollection parts)
        {
            var partition_count = (uint)parts.Count;
            ulong disk_end_sector = (this.disk.size_bytes / this.disk.sector_bytes_length);

            for (int i = 0; i < Math.Min(partition_count, WebTVPartitionManager.MAX_PARTITION_COUNT); i++)
            {
                if(parts[i].sector_start < disk_end_sector)
                {
                    var partition = new partition_entry()
                    {
                        name = parts[i].name,
                        sector_length = BigEndianConverter.GetBytes((uint)parts[i].sector_length),
                        sector_start = BigEndianConverter.GetBytes((uint)parts[i].sector_start),
                        type = BigEndianConverter.GetBytes((uint)parts[i].type),
                        unknown1 = BigEndianConverter.GetBytes((uint)0),
                        unknown2 = BigEndianConverter.GetBytes((uint)0)
                    };

                    this.initialize_partition(partition, parts[i].delegate_filename);
                }
            }
        }

        public partition_entry add_partition(WebTVPartition center_part, string name, ulong size_in_bytes, PartitionType type, string requested_delegate = "")
        {
            partition_entry new_partition = new partition_entry();
            new_partition.name = "";

            var max_size = this.largest_available_create_size();

            size_in_bytes = Math.Min(size_in_bytes, max_size);

            var size_in_sectors = this.bytes_to_sectors(size_in_bytes);

            var partition_table = this.get_partition_list(this.enumerate_partitions());

            var free_region = this.get_available_free_region(partition_table, center_part, size_in_sectors);

            if (free_region != null)
            {
                var new_partition_table = new List<partition_entry>();

                ulong selected_free_region_size = 0;

                for (int i = 0; i < partition_table.Count; i++)
                {
                    var partition_type = (PartitionType)BigEndianConverter.ToUInt32(partition_table[i].type, 0);

                    if (partition_type == PartitionType.FREE || partition_type == PartitionType.UNALLOCATED)
                    {
                        ulong free_start = BigEndianConverter.ToUInt32(free_region[0].sector_start, 0);
                        ulong sector_start = BigEndianConverter.ToUInt32(partition_table[i].sector_start, 0);
                        ulong sector_length = BigEndianConverter.ToUInt32(partition_table[i].sector_length, 0);

                        if (sector_start == free_start || selected_free_region_size > 0)
                        {
                            selected_free_region_size += sector_length;

                            if (selected_free_region_size >= size_in_sectors)
                            {
                                new_partition = new partition_entry()
                                {
                                    name = name,
                                    sector_length = BigEndianConverter.GetBytes((uint)size_in_sectors),
                                    sector_start = BigEndianConverter.GetBytes((uint)free_start),
                                    type = BigEndianConverter.GetBytes((uint)type),
                                    unknown1 = BigEndianConverter.GetBytes((uint)0),
                                    unknown2 = BigEndianConverter.GetBytes((uint)0)
                                };
                                new_partition_table.Add(new_partition);

                                var leftover_sectors = selected_free_region_size - size_in_sectors;

                                if (leftover_sectors > 0)
                                {
                                    var leftover_partition = new partition_entry()
                                    {
                                        name = "free",
                                        sector_length = BigEndianConverter.GetBytes((uint)leftover_sectors),
                                        sector_start = BigEndianConverter.GetBytes((uint)(free_start + size_in_sectors)),
                                        type = BigEndianConverter.GetBytes((uint)PartitionType.FREE),
                                        unknown1 = BigEndianConverter.GetBytes((uint)0),
                                        unknown2 = BigEndianConverter.GetBytes((uint)0)
                                    };
                                    new_partition_table.Add(leftover_partition);
                                }

                                selected_free_region_size = 0;
                            }

                            continue;
                        }
                    }

                    new_partition_table.Add(partition_table[i]);
                }

                if (new_partition.name != "")
                {
                    this.write_partition_table(this.partition_table_to_data(new_partition_table));

                    this.initialize_partition(new_partition, requested_delegate);
                }
            }

            return new_partition;
        }

        public void consolidate_free_partitions()
        {
            var current_partition_table = get_partition_list(this.enumerate_partitions());
            var new_partition_table = new List<partition_entry>();

            uint expected_next_sector_start = 0;
            uint free_region_start = 0;
            uint free_region_size = 0;

            Action<uint, uint> update_free_region = delegate(uint sector_start, uint sector_length)
            {
                if (free_region_start == 0)
                {
                    free_region_start = sector_start;
                }

                free_region_size += sector_length;
            };

            Action<string> add_free_region = delegate(string name)
            {
                if (free_region_start > 0 && free_region_size > 0)
                {
                    var free_region = new partition_entry()
                    {
                        name = name,
                        type = BigEndianConverter.GetBytes((uint)PartitionType.FREE),
                        sector_start = BigEndianConverter.GetBytes((uint)free_region_start),
                        sector_length = BigEndianConverter.GetBytes((uint)free_region_size),
                    };

                    new_partition_table.Add(free_region);

                    free_region_start = 0;
                    free_region_size = 0;
                }
            };

            for (int i = 0; i < current_partition_table.Count; i++)
            {
                var partition_type = (PartitionType)BigEndianConverter.ToUInt32(current_partition_table[i].type, 0);
                uint sector_start = BigEndianConverter.ToUInt32(current_partition_table[i].sector_start, 0);
                uint sector_length = BigEndianConverter.ToUInt32(current_partition_table[i].sector_length, 0);

                if (sector_start > expected_next_sector_start)
                {
                    update_free_region.Invoke(expected_next_sector_start, (sector_start - expected_next_sector_start));
                }

                if (partition_type == PartitionType.FREE || partition_type == PartitionType.UNALLOCATED)
                {
                    update_free_region.Invoke(sector_start, sector_length);
                }
                else
                {
                    // Add any free region that has been accumulating before this non-free partition.
                    add_free_region.Invoke("free");

                    new_partition_table.Add(current_partition_table[i]);
                }

                expected_next_sector_start = sector_start + sector_length;
            }

            // If the last partition was free, make sure we add it.
            add_free_region.Invoke("free");

            this.write_partition_table(this.partition_table_to_data(new_partition_table));
        }

        public void initialize_disk(DiskLayout layout = DiskLayout.LC2)
        {
            var disk_size = this.disk.size_bytes;

            ulong first_partition_size = 0;
            ulong second_partition_size = 0;
            this.disk.set_disk_layout(layout);
            if (layout == DiskLayout.UTV)
            { 
                first_partition_size = WebTVPartitionManager.UTV_FIRST_PARTITION_SIZE;
            }
            else if (layout == DiskLayout.WEBSTAR)
            {
                first_partition_size = WebTVPartitionManager.WEBSTAR_FIRST_PARTITION_SIZE;
                second_partition_size = WebTVPartitionManager.WEBSTAR_SECOND_PARTITION_SIZE;
            }
            else if (layout == DiskLayout.LC2)
            {
                first_partition_size = WebTVPartitionManager.LC2_FIRST_PARTITION_SIZE;
            }
            else
            { 
                first_partition_size = WebTVPartitionManager.LC2_FIRST_PARTITION_SIZE;
            }

            this.disk.set_disk_collation(this.disk.io.byte_converter.disklayout_to_bytetransform(layout));
            
            if (first_partition_size > 0 && disk_size > (first_partition_size + second_partition_size))
            {
                var disk_sector_count = this.bytes_to_sectors(disk_size);
                var first_partition_sector_count = this.bytes_to_sectors(first_partition_size);
                var second_partition_sector_count = this.bytes_to_sectors(second_partition_size);
                var free_partition_sector_count = disk_sector_count - first_partition_sector_count - second_partition_sector_count;


                var new_partition_table = new List<partition_entry>();

                var first_partition = new partition_entry()
                {
                    name = "boot",
                    type = BigEndianConverter.GetBytes((uint)PartitionType.RAW),
                    sector_start = BigEndianConverter.GetBytes((uint)0),
                    sector_length = BigEndianConverter.GetBytes((uint)first_partition_sector_count),
                };
                new_partition_table.Add(first_partition);

                if (second_partition_sector_count > 0)
                {
                    var second_partition = new partition_entry()
                    {
                        name = "boot2",
                        type = BigEndianConverter.GetBytes((uint)PartitionType.RAW2),
                        sector_start = BigEndianConverter.GetBytes((uint)first_partition_sector_count),
                        sector_length = BigEndianConverter.GetBytes((uint)second_partition_sector_count),
                    };
                    new_partition_table.Add(second_partition);
                }

                if(free_partition_sector_count > 0)
                {
                    var free_partition = new partition_entry()
                    {
                        name = "free",
                        type = BigEndianConverter.GetBytes((uint)PartitionType.FREE),
                        sector_start = BigEndianConverter.GetBytes((uint)(first_partition_sector_count + second_partition_sector_count)),
                        sector_length = BigEndianConverter.GetBytes((uint)free_partition_sector_count),
                    };
                    new_partition_table.Add(free_partition);
                }

                var blank_table = new byte[WebTVPartitionManager.PARTITION_TABLE_SIZE];
                if (layout == DiskLayout.LC2)
                {
                    this.disk.io.AbsWrite(blank_table, WebTVPartitionManager.PARTITION_TABLE_WEBSTAR_OFFSET, 0, (uint)blank_table.Length);
                    this.disk.io.AbsWrite(blank_table, WebTVPartitionManager.PARTITION_TABLE_UTV_OFFSET, 0, (uint)blank_table.Length);
                }
                else if (layout == DiskLayout.WEBSTAR)
                {
                    this.disk.io.AbsWrite(blank_table, WebTVPartitionManager.PARTITION_TABLE_LC2_OFFSET, 0, (uint)blank_table.Length);
                    this.disk.io.AbsWrite(blank_table, WebTVPartitionManager.PARTITION_TABLE_UTV_OFFSET, 0, (uint)blank_table.Length);
                }
                else if (layout == DiskLayout.UTV)
                {
                    this.disk.io.AbsWrite(blank_table, WebTVPartitionManager.PARTITION_TABLE_LC2_OFFSET, 0, (uint)blank_table.Length);
                    this.disk.io.AbsWrite(blank_table, WebTVPartitionManager.PARTITION_TABLE_WEBSTAR_OFFSET, 0, (uint)blank_table.Length);
                }

                this.write_partition_table(this.partition_table_to_data(new_partition_table));
            }
        }
    }
}
