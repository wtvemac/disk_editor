using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using disk_editor;
using System.Windows.Markup;
using Newtonsoft.Json.Linq;

namespace disk_editor
{
    [StructLayout(LayoutKind.Sequential, Size = DVRFsd.DVRFS_HEADER_SIZE), Serializable]
    public unsafe struct dvrfs_header
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x00] a1 + 36 (0x24) forces the checksum of the first 40 bytes to 0
        public byte[] header_checksum_negative;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x04] a1 + 40 (0x28)
        public byte[] version;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x08] a1 + 44 (0x2C)
        public byte[] signature;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x0C] a1 + 48 (0x30)
        public byte[] chunk_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x10] a1 + 52 (0x34)
        public byte[] total_chunks;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x14] a1 + 56 (0x38)
        public byte[] first_chunk_offset;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x18] a1 + 60 (0x3C) size=((total_chunks >> 1) * 4)
        public byte[] first_chain_table_offset;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x1C] a1 + 64 (0x40) size=((total_chunks >> 1) * 4)
        public byte[] second_chain_table_offset;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x20] a1 + 68 (0x44)
        public byte[] fs_info_offset;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x24] a1 + 72 (0x48)
        public byte[] chain_table_checksum;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x28] a1 + 76 (0x4C) Always 0x602C0300?
        public byte[] unknown4;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x2C] a1 + 80 (0x50) Always 0xC83C0300?
        public byte[] unknown5;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x30] a1 + 84  (0x54) "((a1 + 84) + 56) / ((a1 + 84) + 52)"
        public byte[] fsinfo_capacity_offset;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x34] a1 + 88 (0x58) Always 0x01000000?
        public byte[] unknown6;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x38] a1 + 92 (0x5C)
        public byte[] unknown7;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x3C] a1 + 96 (0x60) Always 0x7804FE81?
        public byte[] unknown8;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x40] a1 + 100 (0x64) Always 0x00000000?
        public byte[] unknown9;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x44] a1 + 104 (0x68) Always 0x00000000?
        public byte[] unknown10;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x48] a1 + 108 (0x6C) Always 0x01000000?
        public byte[] unknown11;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x4C] a1 + 112 (0x70)
        public byte[] unknown12;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x50] a1 + 116 (0x74) Always 0x1404FE81?
        public byte[] unknown13;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x54] a1 + 120 (0x78) Always 0x00000000?
        public byte[] unknown14;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x58] a1 + 124 (0x7C) Always 0x00000000?
        public byte[] unknown15;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x5C] a1 + 128 (0x80)
        public byte[] using_avdisk_driver;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x60] a1 + 132 (0x84)
        public byte[] unknown17;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x64] a1 + 136 (0x88)
        public byte[] unknown18;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x68] a1 + 140 (0x8C) Always 0x01000000?
        public byte[] unknown19;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x6C] a1 + 144 (0x90) Always 0x38000000?
        public byte[] unknown20;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x70] a1 + 148 (0x94) Always 0x982C0300?
        public byte[] unknown21;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x74] a1 + 152 (0x98) Always 0xA02C0300?
        public byte[] unknown22;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x78] a1 + 156 (0x9C) Always 0xA02C0300?
        public byte[] unknown23;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x7C] a1 + 160 (0xa0) Always 0x01000000?
        public byte[] unknown24;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x80] a1 + 164 (0xA4) Always 0x01000000?
        public byte[] unknown25;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x84] a1 + 168 (0xA8) Always 0xB003FE81?
        public byte[] unknown26;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x88] a1 + 172 (0xAC) Always 0x01000000?
        public byte[] unknown27;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x8C] a1 + 176 (0xB0) Always 0x01000000?
        public byte[] unknown28;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x90] a1 + 180 (0xB4) Always 0xC82B0300?
        public byte[] unknown29;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x94] a1 + 184 (0xB8) Always 0x00020000?
        public byte[] unknown30;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x98] a1 + 188 (0xBC) Always 0x00100000?
        public byte[] unknown31;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0x9C] a1 + 192 (0xC0) Always 0x02000000?
        public byte[] unknown32;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0xA0] a1 + 196 (0xC4) Always 0x15010000?
        public byte[] unknown33;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0xA4] a1 + 200 (0xC8) Always 0x02000000?
        public byte[] unknown34;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0xA8] a1 + 204 (0xCC) Always 0x00000000?
        public byte[] unknown35;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)] // [0xAC] a1 + 208 (0xD0) Always 0x20100000?
        public byte[] unknown36;
    }

    [StructLayout(LayoutKind.Sequential, Size = DVRFsd.FS_INFO_SIZE), Serializable]
    public unsafe struct fs_info
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] checksum_negative;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] signature;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string file_name;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] chunk_start;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown2;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] padding_chunks;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown4;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 968)]
        public byte[] unknown5;
    }

    class DVRFileStream
    {
        public string file_name { get; set; }
        public ulong file_size { get; set; }
        public int fs_info_index { get; set; }
        public ulong chunk_size { get; set; }
        public ulong current_chunk_index { get; set; }
        public ulong last_chunk_index { get; set; }
        public ulong last_chunk_size { get; set; }
    }

    class DVRRecordings
    {
        public ObservableCollection<DVRRecording> dvr_recordings { get; set; }
    }

    class DVRFsd : WebTVIO
    {
        public const ulong DVRFS_HEADER_SIGNATURE = 0x4A484734; // JHG4 (or 4GHJ stored)
        public const int DVRFS_HEADER_SIZE = 0xB0;
        public const int DVRFS_BLOCK_SIZE = 0x200;
        public const ulong FS_INFO_SIGNATURE = 0x4C61656C; // Leal (or laeL stored)
        public const int FS_INFO_SIZE = 0x400;
        public const int PADDING_CHUNK_SIZE = 0x10000;
        public const int FS_INFO_COUNT = 1414;

        public DVRFSInfo dvrfs_header;

        public ulong block_offset_to_byte_offset(ulong block_offset)
        {
            return WebTVPartitionManager.PARTITON_DATA_OFFSET + (block_offset * DVRFsd.DVRFS_BLOCK_SIZE);
        }

        public uint calculate_checksum(byte[] data, int start = 0x00, int length = 0x00, bool big_endian = false)
        {
            if (length == 0)
            {
                length = data.Length;
            }
            else
            {
                length = Math.Min(length, data.Length);

            }

            uint checksum = 0x00;
            for (int i = 0x00; i < length; i += 0x04)
            {
                uint checksum_block = 0x00;
                if (big_endian)
                {
                    checksum_block = BigEndianConverter.ToUInt32(data, i);
                }
                else
                {
                    checksum_block = BitConverter.ToUInt32(data, i);
                }

                checksum += checksum_block;
            }

            return checksum;
        }

        public byte[] get_dvrfs_header_data()
        {
            var dvrfs_header_data = new byte[DVRFsd.DVRFS_HEADER_SIZE];

            this.Read(dvrfs_header_data, 0, (long)WebTVPartitionManager.PARTITON_DATA_OFFSET, dvrfs_header_data.Length);

            return dvrfs_header_data;
        }

        public dvrfs_header unserialize_dvrfs_header(byte[] dvrfs_header_data)
        {
            GCHandle dvrfs_header_handle = new GCHandle();

            try
            {
                this.Read(dvrfs_header_data, 0, (long)WebTVPartitionManager.PARTITON_DATA_OFFSET, dvrfs_header_data.Length);

                dvrfs_header_handle = GCHandle.Alloc(dvrfs_header_data, GCHandleType.Pinned);
                var dvrfs_header_pointer = dvrfs_header_handle.AddrOfPinnedObject();

                return (dvrfs_header)Marshal.PtrToStructure(dvrfs_header_pointer, typeof(dvrfs_header));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse DVRFsd header. " + e.Message);
            }
            finally
            {
                if (dvrfs_header_handle.IsAllocated)
                {
                    dvrfs_header_handle.Free();
                }
            }
        }

        public DVRFSInfo populate_dvrfs_info()
        {
            var dvrfs_header_data = this.get_dvrfs_header_data();

            var dvrfs_header = this.unserialize_dvrfs_header(dvrfs_header_data);

            var found_header_checksum = this.calculate_checksum(dvrfs_header_data, 0x00, 0x0A * 4);

            var total_chunks = BitConverter.ToUInt32(dvrfs_header.total_chunks, 0);
            var first_chain_table_offset = BitConverter.ToUInt32(dvrfs_header.first_chain_table_offset, 0);
            var second_chain_table_offset = BitConverter.ToUInt32(dvrfs_header.second_chain_table_offset, 0);

            var chain_table_length = (total_chunks >> 1) * 2;

            var _first_chain_table = new byte[chain_table_length * 2];
            this.Read(_first_chain_table, 0, (long)this.block_offset_to_byte_offset(first_chain_table_offset), _first_chain_table.Length);
            var first_chain_table_checksum = this.calculate_checksum(_first_chain_table);
            var first_chain_table = new ushort[chain_table_length];
            for (int i = 0, ii = 0; i < _first_chain_table.Length; i += 2, ii++)
            {
                first_chain_table[ii] = BitConverter.ToUInt16(_first_chain_table, i);
            }

            var _second_chain_table = new byte[chain_table_length * 2];
            this.Read(_second_chain_table, 0, (long)this.block_offset_to_byte_offset(second_chain_table_offset), _second_chain_table.Length);
            var second_chain_table_checksum = this.calculate_checksum(_second_chain_table);
            var second_chain_table = new ushort[chain_table_length];
            for (int i = 0, ii = 0; i < _first_chain_table.Length; i += 2, ii++)
            {
                second_chain_table[ii] = BitConverter.ToUInt16(_second_chain_table, i);
            }

            return new DVRFSInfo()
            {
                header_checksum_negative = BitConverter.ToUInt32(dvrfs_header.header_checksum_negative, 0),
                found_header_checksum = found_header_checksum,
                version = BitConverter.ToUInt32(dvrfs_header.version, 0),
                signature = BitConverter.ToUInt32(dvrfs_header.signature, 0),
                chunk_size = BitConverter.ToUInt32(dvrfs_header.chunk_size, 0),
                total_chunks = total_chunks,
                first_chunk_offset = BitConverter.ToUInt32(dvrfs_header.first_chunk_offset, 0),
                first_chain_table_offset = first_chain_table_offset,
                first_chain_table = first_chain_table,
                first_chain_table_found_checksum = first_chain_table_checksum,
                second_chain_table_offset = second_chain_table_offset,
                second_chain_table = second_chain_table,
                second_chain_table_found_checksum = second_chain_table_checksum,
                fs_info_offset = BitConverter.ToUInt32(dvrfs_header.fs_info_offset, 0),
                chain_table_checksum = BitConverter.ToUInt32(dvrfs_header.chain_table_checksum, 0),
                selected_chain_table = 0,
                fsinfo_capacity_offset = BitConverter.ToUInt32(dvrfs_header.fsinfo_capacity_offset, 0),
                using_avdisk_driver = (BitConverter.ToUInt32(dvrfs_header.using_avdisk_driver, 0) == 1)
            };
        }

        public bool parse_dvrfs_header()
        {
            this.dvrfs_header = this.populate_dvrfs_info();

            if (this.dvrfs_header.found_header_checksum == 0x00 || this.dvrfs_header.signature == DVRFS_HEADER_SIGNATURE)
            {
                if (this.dvrfs_header.version < 2)
                {
                    if (((this.dvrfs_header.chunk_size * DVRFsd.DVRFS_BLOCK_SIZE) & 0x1FFFF) == 0)
                    {
                        if (((DVRFsd.DVRFS_BLOCK_SIZE + 0x1FFFF) / DVRFsd.DVRFS_BLOCK_SIZE) < this.dvrfs_header.chunk_size)
                        {
                            if (this.dvrfs_header.first_chain_table_found_checksum != this.dvrfs_header.chain_table_checksum)
                            {
                                if (this.dvrfs_header.second_chain_table_found_checksum != this.dvrfs_header.chain_table_checksum)
                                {
                                    throw new InvalidDataException("Chain table 1 and 2 corrupt. Checksum check failed.");
                                }
                                else
                                {
                                    this.dvrfs_header.selected_chain_table = 2;

                                    return true; // using chain table 2
                                }
                            }
                            else
                            {
                                this.dvrfs_header.selected_chain_table = 1;

                                return true; // using chain table 1
                            }
                        }
                        else
                        {
                            throw new InvalidDataException("Chunk size too small '" + this.dvrfs_header.chunk_size.ToString("X") + "'.");
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("Bad chunk size alignment '" + (this.dvrfs_header.chunk_size * DVRFsd.DVRFS_BLOCK_SIZE).ToString("X") + "' needs to be bound to 0x20000.");
                    }
                }
                else
                {
                    throw new InvalidDataException("Bad header version '" + this.dvrfs_header.version.ToString() + "'");
                }
            }
            else
            {
                throw new InvalidDataException("Corrupt DVRFS header detected (checksum or signature bad).");
            }

            return false;
        }

        public byte[] get_fs_info_data(long entry_index)
        {
            var fs_info_data = new byte[DVRFsd.FS_INFO_SIZE];

            var fs_info_offset = (long)this.block_offset_to_byte_offset(this.dvrfs_header.fs_info_offset) + (entry_index * DVRFsd.FS_INFO_SIZE);

            this.Read(fs_info_data, 0, fs_info_offset, fs_info_data.Length);

            return fs_info_data;
        }

        public fs_info unserialize_fs_info(byte[] fs_info_data)
        {
            GCHandle fs_info_handle = new GCHandle();

            try
            {
                fs_info_handle = GCHandle.Alloc(fs_info_data, GCHandleType.Pinned);
                var fs_info_pointer = fs_info_handle.AddrOfPinnedObject();

                return (fs_info)Marshal.PtrToStructure(fs_info_pointer, typeof(fs_info));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse FS Info. " + e.Message);
            }
            finally
            {
                if (fs_info_handle.IsAllocated)
                {
                    fs_info_handle.Free();
                }
            }
        }

        public FSInfo populate_fs_info(long entry_index)
        {
            var fs_info_data = this.get_fs_info_data(entry_index);

            var fs_info = this.unserialize_fs_info(fs_info_data);

            var found_checksum = this.calculate_checksum(fs_info_data);

            return new FSInfo()
            {
                checksum_negative = BitConverter.ToUInt32(fs_info.checksum_negative, 0),
                found_checksum = found_checksum,
                signature = BitConverter.ToUInt32(fs_info.signature, 0),
                file_name = fs_info.file_name,
                chunk_start = BitConverter.ToUInt32(fs_info.chunk_start, 0),
                unknown2 = BitConverter.ToUInt32(fs_info.unknown2, 0),
                padding_chunks = BitConverter.ToUInt32(fs_info.padding_chunks, 0),
                unknown4 = BitConverter.ToUInt32(fs_info.unknown4, 0),
            };
        }


        public bool parse_fs_info(ref FSInfo fs_info_entry)
        {
            if (fs_info_entry.found_checksum == 0x00 || fs_info_entry.signature == FS_INFO_SIGNATURE)
            {
                return true;
            }
            else
            {
                throw new InvalidDataException("Corrupt FS info detected (checksum or signature bad).");
            }

            return false;
        }

        public bool parse_fs_info_entries()
        {
            this.dvrfs_header.fs_info_entries = new List<FSInfo>();

            for (var i = 0; i < DVRFsd.FS_INFO_COUNT; i++)
            {
                try
                {
                    var fs_info_entry = this.populate_fs_info(i);

                    if (this.parse_fs_info(ref fs_info_entry))
                    {
                        if (fs_info_entry.file_name == "")
                        {
                            fs_info_entry.enabled = false;
                            fs_info_entry.discarded = false;
                        }
                        else
                        {
                            fs_info_entry.enabled = true;
                            fs_info_entry.discarded = false;

                            fs_info_entry.chunk_offsets = new List<ulong>();
                            ulong origin_offset = this.block_offset_to_byte_offset(this.dvrfs_header.first_chunk_offset);
                            ulong chunk_size = this.dvrfs_header.chunk_size << 9;
                            for (ulong ii = fs_info_entry.chunk_start; ii < (ulong)this.dvrfs_header.first_chain_table.Length; ii = this.dvrfs_header.first_chain_table[ii])
                            {
                                var read_offset = origin_offset + ii * chunk_size;
                                fs_info_entry.chunk_offsets.Add(read_offset);
                            }

                            fs_info_entry.chunk_count = (ulong)fs_info_entry.chunk_offsets.Count;
                        }

                        this.dvrfs_header.fs_info_entries.Add(fs_info_entry);
                    }
                }
                catch (Exception e)
                {
                    //System.Windows.MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
                    continue;
                }
            }

            return (this.dvrfs_header.fs_info_entries.Count > 0);
        }

        public DVRFileStream open_file(string dvrfs_file_name, bool search_disabled = false)
        {
            for (var i = 0; i < this.dvrfs_header.fs_info_entries.Count; i++)
            {
                var fs_info_entry = this.dvrfs_header.fs_info_entries[i];

                if ((fs_info_entry.enabled || !search_disabled) && fs_info_entry.file_name.ToLower() == dvrfs_file_name.ToLower())
                {
                    var chunk_size = this.dvrfs_header.chunk_size << 9;
                    var last_chunk_size = fs_info_entry.padding_chunks * DVRFsd.PADDING_CHUNK_SIZE;

                    var file_size = (chunk_size * (fs_info_entry.chunk_count - 1)) + last_chunk_size;

                    return new DVRFileStream()
                    {
                        file_name = fs_info_entry.file_name,
                        file_size = file_size,
                        fs_info_index = i,
                        chunk_size = chunk_size,
                        current_chunk_index = 0,
                        last_chunk_index = fs_info_entry.chunk_count - 1,
                        last_chunk_size = fs_info_entry.padding_chunks * DVRFsd.PADDING_CHUNK_SIZE,
                    };
                }
            }

            return null;
        }

        public byte[] read_file(ref DVRFileStream stream, int chunk_index = -1)
        {
            if(stream.fs_info_index < this.dvrfs_header.fs_info_entries.Count)
            {
                var fs_info_entry = this.dvrfs_header.fs_info_entries[stream.fs_info_index];

                ulong read_size = 0;
                if (stream.current_chunk_index == stream.last_chunk_index)
                {
                    read_size = stream.last_chunk_size;
                }
                else if(stream.current_chunk_index < stream.last_chunk_index)
                {
                    read_size = stream.chunk_size;
                }

                if (read_size > 0)
                {
                    var data = new byte[read_size];
                    this.Read(data, 0, (long)fs_info_entry.chunk_offsets[(int)stream.current_chunk_index], (int)read_size);

                    stream.current_chunk_index++;

                    return data;
                }
            }

            return null;
        }

        public byte[] get_dvr_file(string dvrfs_file_name)
        {
            var hFILE = this.open_file(dvrfs_file_name);

            if (hFILE != null)
            {
                var data = new byte[hFILE.file_size];

                ulong i = 0;
                while (true)
                {
                    var _data = this.read_file(ref hFILE);

                    if (_data != null)
                    {
                        _data.CopyTo(data, (int)i);

                        i += (ulong)_data.Length;
                    }
                    else
                    {
                        break;
                    }
                }

                return data;
            }

            return null;
        }

        public ulong read_uint64(ref byte[] value, int startindex)
        {
            return (BitConverter.ToUInt32(value, startindex) << 0x20) + BitConverter.ToUInt32(value, startindex + 0x04);
        }

        public void scan_dvr_index(string recording_id)
        {
            var dvrfs_file_name = recording_id + ".index2";

            var index_data = this.get_dvr_file(dvrfs_file_name);

            var out_index_file_name = "\\\\10.0.0.80\\WebTVHacking\\Client\\Hacking\\dvrfs_research\\test_files\\" + dvrfs_file_name;
            var hIFILE = new BinaryWriter(new FileStream(out_index_file_name, FileMode.Create));
            hIFILE.Write(index_data);
            hIFILE.Close();

            var cool = "";
            for (var i = 0; i < index_data.Length;)
            {
                var grouping_index = BitConverter.ToUInt32(index_data, i);
                i += 0x04;

                var grouping_entry_count = BitConverter.ToUInt32(index_data, i);
                i += 0x04;

                var unknown1 = BitConverter.ToUInt32(index_data, i);
                i += 0x04;
                var unknown2 = BitConverter.ToUInt32(index_data, i);
                i += 0x04;
                var unknown3 = BitConverter.ToUInt32(index_data, i);
                i += 0x04;

                i += 0x0C;

                cool += "GROUP," + grouping_index.ToString("X") + "," + grouping_entry_count.ToString("X") + "," + unknown1.ToString("X") + "," + unknown2.ToString("X") + "," + unknown3.ToString("X") + "\n";
                for (ulong ii = 0; ii < grouping_entry_count && i < index_data.Length; ii++)
                {
                    var _unknown1 = BitConverter.ToUInt64(index_data, i);
                    i += 0x08;

                    var indexed_offset = BitConverter.ToUInt64(index_data, i);
                    i += 0x08;

                    var _unknown2 = BitConverter.ToUInt64(index_data, i);
                    i += 0x08;

                    var _unknown3 = BitConverter.ToUInt32(index_data, i);
                    i += 0x04;

                    var _unknown4 = BitConverter.ToUInt32(index_data, i);
                    i += 0x04;

                    var indexed_size = BitConverter.ToUInt32(index_data, i);
                    i += 0x04;
                    i += 0x04;

                    cool += _unknown1.ToString("X") + "," + indexed_offset.ToString("X") + "," + _unknown2.ToString("X") + "," + _unknown3.ToString("X") + "," + _unknown4.ToString("X") + "," + indexed_size.ToString("X") + "\n";
                }
            }

            var out_meta_file_name = "\\\\10.0.0.80\\WebTVHacking\\Client\\Hacking\\dvrfs_research\\test_files\\" + recording_id + ".csv";
            var hMFILE = new BinaryWriter(new FileStream(out_meta_file_name, FileMode.Create));
            hMFILE.Write(cool);
            hMFILE.Close();


            var out_video_file_name = "\\\\10.0.0.80\\WebTVHacking\\Client\\Hacking\\dvrfs_research\\test_files\\" + recording_id + ".mpg";
            var hVFILE = new BinaryWriter(new FileStream(out_video_file_name, FileMode.Create));
            var hRFILE = this.open_file(recording_id);
            if (hVFILE != null && hRFILE != null)
            {
                ulong i = 0;
                while (true)
                {
                    var _data = this.read_file(ref hRFILE);

                    if (_data != null)
                    {
                        hVFILE.Write(_data);
                    }
                    else
                    {
                        hVFILE.Close();
                        break;
                    }
                }
            }
            //System.Windows.MessageBox.Show(cool.ToString());
        }

        public DVRRecordings enumerate_dvr_recodings()
        {
            var dvr_recordings = new DVRRecordings();

            try
            {
                if (this.parse_dvrfs_header() && this.parse_fs_info_entries())
                {
                    //this.scan_dvr_index();
                    for (var i = 0; i < this.dvrfs_header.fs_info_entries.Count; i++)
                    {
                        var fs_info_entry = this.dvrfs_header.fs_info_entries[i];

                        if (fs_info_entry.enabled)
                        {
                            var out_video_file_name = "\\\\10.0.0.80\\WebTVHacking\\Client\\Hacking\\dvrfs_research\\test_files\\" + fs_info_entry.file_name.ToLower();
                            var hVFILE = new BinaryWriter(new FileStream(out_video_file_name, FileMode.Create));
                            var hRFILE = this.open_file(fs_info_entry.file_name);
                            if (hVFILE != null && hRFILE != null)
                            {
                                while (true)
                                {
                                    var _data = this.read_file(ref hRFILE);

                                    if (_data != null)
                                    {
                                        hVFILE.Write(_data);
                                    }
                                    else
                                    {
                                        hVFILE.Close();
                                        break;
                                    }
                                }
                            }
                        }
                    }


                }
                }
            catch (Exception e)
            {
                throw new InvalidDataException("Error reading DVRFsd recordings. " + e.Message);
            }

            return dvr_recordings;
        }

        public DVRFsd(WebTVDiskIO io, DiskCageBounds dvr_partition_bounds)
            : base(io, dvr_partition_bounds)
        {
        }

        public DVRFsd(string file_name) 
            : base(file_name)
        {
            this.byte_converter = null;
        }

        public DVRFsd(Stream reader)
            : base(reader)
        {
        }

    }
}
