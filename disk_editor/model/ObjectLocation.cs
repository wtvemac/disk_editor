using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    public enum ObjectLocationCategory
    {
        BUILD = 0,
        PRIMARY_NVRAM = 1,
        SECONDARY_NVRAM = 2,
    };

    public enum ObjectLocationType
    {
        UNKNOWN = -3,
        EXPLODED_PRIMARY_LOCATION = -2,
        PRIMARY_LOCATION = -1,
        BROWSER0_LOCATION = 0,
        BROWSER1_LOCATION = 1,
        DIAG_LOCATION = 2,
        TIER3_DIAG_LOCATION = 3,
        FILE_LOCATION = 4,
        NVRAM_PRIMARY0 = 5,
        NVRAM_PRIMARY1 = 6,
        NVRAM_SECONDARY0 = 7,
        NVRAM_SECONDARY1 = 8,
        UNKNOWN0 = 9,
        UNKNOWN1 = 10,
        UNKNOWN2 = 11,
    };

    public class ObjectLocation
    {
        public ObjectLocationCategory category { get; set; }
        public ObjectLocationType type { get; set; }
        public ulong offset { get; set; }
        public ulong size_bytes { get; set; }
        public bool selected { get; set; }
    }
}
