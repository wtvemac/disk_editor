using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    class PackedTellyScriptHeader
    {
        public string magic { get; set; }
        public uint version_major { get; set; }
        public uint version_minor { get; set; }
        public uint script_id { get; set; }
        public uint script_mod { get; set; }
        public uint compressed_data_length { get; set; }
        public uint decompressed_data_length { get; set; }
        public uint decompressed_checksum { get; set; }
        public uint actual_decompressed_checksum { get; set; }
        public uint unknown1 { get; set; }
    }
}
