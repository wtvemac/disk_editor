using System;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;

namespace disk_editor
{
    public class WebTVDiskCollection : ObservableCollection<WebTVDisk> 
    {
        public void add_enumerated_disks(DiskWMIEntries wmi_disk_table)
        {
            for (int i = 0; i < wmi_disk_table.Count; i++)
            {
                add_physical_disk(wmi_disk_table[i]);
            }
        }

        public WebTVDisk add_physical_disk(DiskWMIEntry wmi_disk)
        {
            return this.add_disk(wmi_disk.open_disk());
        }

        public WebTVDisk add_disk_image(string image_path)
        {
            var fi = new FileInfo(image_path);

            if (fi != null && fi.Exists)
            {
                var disk = new WebTVDisk()
                {
                    name = fi.Name,
                    type = DiskType.IMAGE,
                    path = image_path,
                    size_bytes = (ulong)fi.Length,
                    sector_bytes_length = 512
                };

                return this.add_disk(disk);
            }
            else 
            {
                throw new FileNotFoundException("Couldn't find specified WebTV image file.");
            }
        }

        public WebTVDisk add_disk(WebTVDisk disk)
        {
            if(disk != null)
            {
                var converter = new WebTVDiskCollationConverter();

                var io = new WebTVDiskIO(disk, converter, FileAccess.ReadWrite);

                disk.start_io(io);

                disk.detect_collation(converter);

                var partition_manager = new WebTVPartitionManager(disk);
                disk.detect_layout(partition_manager);

                disk.update_disk_state();

                this.Add(disk);
            }

            return disk;
        }

        public void clear_disks()
        {
            foreach (var disk in this)
            {
                this.cleanup_disk(disk);
            }

            this.Clear();
        }

        public void cleanup_disk(WebTVDisk disk)
        {
            try
            {
                disk.clear_partitions();
                disk.stop_io();
            }
            catch
            {

            }
        }

        public void remove_disk(WebTVDisk disk)
        {
            this.cleanup_disk(disk);
            this.Remove(disk);
        }

        public int get_index(WebTVDisk disk)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].id == disk.id)
                {
                    return i;
                }
            }

            return -1;
        }

        public WebTVDisk get_disk_by_path(string path)
        {
            foreach (WebTVDisk disk in this)
            {
                if (disk.path == path)
                {
                    return disk;
                }
            }

            return null;
        }

        public WebTVDisk get_disk_by_id(Guid id)
        {
            foreach (WebTVDisk disk in this)
            {
                if (disk.id == id)
                {
                    return disk;
                }
            }

            return null;
        }

        public WebTVPartition get_partition_by_id(Guid id)
        {
            foreach (WebTVDisk disk in this)
            {
                var found_part = disk.partition_table.get_partition_by_id(id);
                if (found_part != null)
                {
                    return found_part;
                }
            }

            return null;
        }
    }
}
