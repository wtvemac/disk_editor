using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace disk_editor
{
    public enum NVDataType
    {
        NULL_TERMINATED_STRING = 0,
        UINT16 = 1,
        INT16 = 2,
        UINT32 = 3,
        INT32 = 4,
        UINT64 = 5,
        INT64 = 6,
        BOOLEAN = 7,
        CHAR = 8,
        BINARY_BLOB = 9,
        YUV_COLOR = 10
    };

    public enum NVDataEditor
    {
        HEX_EDITOR = 1,
        BOOLEAN_EDITOR = 3,
        STRING_EDITOR = 4,
        INTEGER_EDITOR = 5,
        IP_EDITOR = 6,
        FILE_EDITOR = 7,
        TELLYSCRIPT_EDITOR = 8,
        RGB_COLOR_PICKER = 9
    };

    public struct NVSettingValue
    {
        public byte[] stored_value { get; set; }
        public byte[] edited_value { get; set; }
        public byte[] default_value { get; set; }
    }

    public class NVSetting : ICloneable
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string notes { get; set; }
        public NVDataType data_type { get; set; }
        public NVDataEditor data_editor { get; set; }
        public NVSettingValue value { get; set; }
        public bool priority { get; set; }
        public bool edited { get; set; }
        public bool removed { get; set; }
        public bool added { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public NVSetting Copy()
        {
            return (NVSetting)this.Clone();
        }
        public override String ToString()
        {
            return this.name;
        }
    }
}
