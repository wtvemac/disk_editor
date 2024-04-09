using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace disk_editor
{
    class FSInfo
    {
        public uint checksum_negative { get; set; }
        public uint found_checksum { get; set; }
        public uint signature { get; set; }
        public string file_name { get; set; }
        public ulong chunk_start { get; set; }
        public ulong chunk_count { get; set; }
        public List<ulong> chunk_offsets { get; set; }
        public ulong unknown2 { get; set; }
        public ulong padding_chunks { get; set; }
        public ulong unknown4 { get; set; }
        public bool enabled { get; set; }
        public bool discarded { get; set; }
    }
}
