using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace disk_editor
{
    public class DVRRecording : ICloneable
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string notes { get; set; }
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
