using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    class WebTVBuild
    {
        public uint jump_instruction { get; set; }
        public uint pre_jump_instruction { get; set; }
        public uint jump_offset { get; set; }
        public uint code_checksum { get; set; }
        public uint dword_length { get; set; }
        public uint dword_code_length { get; set; }
        public uint build_number { get; set; }
        public uint build_flags { get; set; }
        public uint build_base_address { get; set; }
        public uint romfs_base_address { get; set; }
        public uint romfs_checksum { get; set; }
        public uint dword_romfs_size { get; set; }
        public uint heap_data_address { get; set; }
        public uint dword_heap_data_size { get; set; }
        public uint dword_heap_free_size { get; set; }
        public uint dword_heap_compressed_data_size { get; set; }
        public uint code_compressed_address { get; set; }
        public uint dword_code_compressed_size { get; set; }
        public bool is_classic_build { get; set; }
    }
}
