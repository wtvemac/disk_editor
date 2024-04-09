using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace disk_editor
{
    class DVRFSInfo
    {
        public uint header_checksum_negative { get; set; }
        public uint found_header_checksum { get; set; }
        public uint version { get; set; }
        public uint signature { get; set; }
        public ulong chunk_size { get; set; }
        public ulong total_chunks { get; set; }
        public ulong first_chunk_offset { get; set; }
        public ulong first_chain_table_offset { get; set; }
        public ushort[] first_chain_table { get; set; }
        public uint first_chain_table_found_checksum { get; set; }
        public ulong second_chain_table_offset { get; set; }
        public ushort[] second_chain_table { get; set; }
        public uint second_chain_table_found_checksum { get; set; }
        public ulong fs_info_offset { get; set; }
        public uint chain_table_checksum { get; set; }
        public byte selected_chain_table { get; set; }
        public ulong fsinfo_capacity_offset { get; set; }
        public bool using_avdisk_driver { get; set; }
        public List<FSInfo> fs_info_entries { get; set; }
    }
}
