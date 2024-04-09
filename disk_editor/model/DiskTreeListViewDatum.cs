using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace disk_editor
{
    class DiskTreeListViewDatum
    {
        public enum ItemObjectType
        {
            DISK = 0,
            PARTITION = 1
        };

        public Guid id { get; set; }
        public string tag { get; set; }
        public BitmapImage icon { get; set; }
        public string name { get; set; }
        public string mount_point { get; set; }
        public string collation { get; set; }
        public string type { get; set; }
        public string delegate_filename { get; set; }
        public ItemObjectType object_type { get; set; }
        public string status { get; set; }
        public string capacity { get; set; }
        public string range { get; set; }
        public WebTVPartitionCollection partition_table { get; set; }
    }
}
