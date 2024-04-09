using System;
using System.Collections.ObjectModel;

namespace disk_editor
{
    public class ObjectLocationCollection : ObservableCollection<ObjectLocation>
    {
        private void add_lc2_build_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.BROWSER0_LOCATION,
                offset = 0x80600,
                size_bytes = 0x800000,
                selected = (selected == ObjectLocationType.BROWSER0_LOCATION),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.BROWSER1_LOCATION,
                offset = 0x880600,
                size_bytes = 0x800000,
                selected = (selected == ObjectLocationType.BROWSER1_LOCATION),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.DIAG_LOCATION,
                offset = 0x40600,
                size_bytes = 0x40000,
                selected = (selected == ObjectLocationType.DIAG_LOCATION),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.TIER3_DIAG_LOCATION,
                offset = 0x10C1000,
                size_bytes = 0x400000,
                selected = (selected == ObjectLocationType.TIER3_DIAG_LOCATION),
            });
            

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.UNKNOWN0,
                offset = 0x40000,
                size_bytes = 0x40600,
                selected = (selected == ObjectLocationType.EXPLODED_PRIMARY_LOCATION),
            });

            /*this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.EXPLODED_PRIMARY_LOCATION,
                offset = 0x880600,
                size_bytes = 0xC40A00,
                selected = (selected == ObjectLocationType.EXPLODED_PRIMARY_LOCATION),
            });*/
        }

        private void add_webstar_build_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.BROWSER0_LOCATION,
                offset = 0x80600,
                size_bytes = 0x1000000,
                selected = (selected == ObjectLocationType.BROWSER0_LOCATION),
            });
        }

        private void add_utv_build_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.BROWSER0_LOCATION,
                offset = 0x13480600,
                size_bytes = 0x2000000,
                selected = (selected == ObjectLocationType.BROWSER0_LOCATION),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.BROWSER1_LOCATION,
                offset = 0x15480600,
                size_bytes = 0x2000000,
                selected = (selected == ObjectLocationType.BROWSER1_LOCATION),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.DIAG_LOCATION,
                offset = 0x13440600,
                size_bytes = 0x40000,
                selected = (selected == ObjectLocationType.DIAG_LOCATION),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.BUILD,
                type = ObjectLocationType.TIER3_DIAG_LOCATION,
                offset = 0x174C1000,
                size_bytes = 0x400000,
                selected = (selected == ObjectLocationType.TIER3_DIAG_LOCATION),
            });
        }

        public void add_enumerated_build(WebTVPartition part, ObjectLocationType? selected)
        {
            if (part.sector_start == 0 && part.disk != null)
            {
                if (part.disk.layout == DiskLayout.UTV)
                {
                    this.add_utv_build_offsets(selected);
                }
                else if (part.disk.layout == DiskLayout.WEBSTAR)
                {
                    this.add_webstar_build_offsets(selected);
                }
                else
                {
                    this.add_lc2_build_offsets(selected);
                }
            }
            else
            {
                if (part.disk.layout == DiskLayout.WEBSTAR && part.type == PartitionType.RAW2)
                {
                    this.Add(new ObjectLocation()
                    {
                        category = ObjectLocationCategory.BUILD,
                        type = ObjectLocationType.PRIMARY_LOCATION,
                        offset = 0,
                        size_bytes = (part.sector_length * part.disk.sector_bytes_length),
                        selected = (selected == ObjectLocationType.PRIMARY_LOCATION),
                    });
                }
                else if (part.type == PartitionType.RAW)
                {
                    this.Add(new ObjectLocation()
                    {
                        category = ObjectLocationCategory.BUILD,
                        type = ObjectLocationType.PRIMARY_LOCATION,
                        offset = WebTVPartitionManager.PARTITON_DATA_OFFSET,
                        size_bytes = (part.sector_length * part.disk.sector_bytes_length) - WebTVPartitionManager.PARTITON_DATA_OFFSET,
                        selected = (selected == ObjectLocationType.PRIMARY_LOCATION),
                    });
                }
            }
        }

        private void add_lc2_primary_nvram_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.PRIMARY_NVRAM,
                type = ObjectLocationType.NVRAM_PRIMARY0,
                offset = 0x200,
                size_bytes = 0x4000,//0x40000,
                selected = (selected == ObjectLocationType.NVRAM_PRIMARY0),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.PRIMARY_NVRAM,
                type = ObjectLocationType.NVRAM_PRIMARY1,
                offset = 0x1080800,
                size_bytes = 0x4000,//0x40000,
                selected = (selected == ObjectLocationType.NVRAM_PRIMARY1),
            });
        }

        private void add_webstar_primary_nvram_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.PRIMARY_NVRAM,
                type = ObjectLocationType.NVRAM_PRIMARY0,
                offset = 0x200,
                size_bytes = 0x4000,//0x40000,
                selected = (selected == ObjectLocationType.NVRAM_PRIMARY0),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.PRIMARY_NVRAM,
                type = ObjectLocationType.NVRAM_PRIMARY1,
                offset = 0x1080800,
                size_bytes = 0x4000,//0x40000,
                selected = (selected == ObjectLocationType.NVRAM_PRIMARY0),
            });
        }

        private void add_utv_primary_nvram_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.PRIMARY_NVRAM,
                type = ObjectLocationType.NVRAM_PRIMARY0,
                offset = 0x200,
                size_bytes = 0x40000,
                selected = (selected == ObjectLocationType.NVRAM_PRIMARY0),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.PRIMARY_NVRAM,
                type = ObjectLocationType.NVRAM_PRIMARY1,
                offset = 0x17480800,
                size_bytes = 0x40000,
                selected = (selected == ObjectLocationType.NVRAM_PRIMARY0),
            });
        }

        public void add_enumerated_primary_nvram(WebTVDisk disk, ObjectLocationType? selected)
        {
            if (disk.layout == DiskLayout.UTV)
            {
                this.add_utv_primary_nvram_offsets(selected);
            }
            else if (disk.layout == DiskLayout.WEBSTAR)
            {
                this.add_webstar_primary_nvram_offsets(selected);
            }
            else
            {
                this.add_lc2_primary_nvram_offsets(selected);
            }
        }

        private void add_lc2_secondary_nvram_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.SECONDARY_NVRAM,
                type = ObjectLocationType.NVRAM_SECONDARY0,
                offset = 0x40200,
                size_bytes = 0x400,
                selected = (selected == ObjectLocationType.NVRAM_SECONDARY0),
            });

            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.SECONDARY_NVRAM,
                type = ObjectLocationType.NVRAM_SECONDARY1,
                offset = 0x10C0800,
                size_bytes = 0x400,
                selected = (selected == ObjectLocationType.NVRAM_SECONDARY1),
            });
        }

        private void add_webstar_secondary_nvram_offsets(ObjectLocationType? selected)
        {
            this.Add(new ObjectLocation()
            {
                category = ObjectLocationCategory.SECONDARY_NVRAM,
                type = ObjectLocationType.NVRAM_SECONDARY0,
                offset = 0x40200,
                size_bytes = 0x400,
                selected = (selected == ObjectLocationType.NVRAM_SECONDARY0),
            });
        }

        private void add_utv_secondary_nvram_offsets(ObjectLocationType? selected)
        {
        }

        public void add_enumerated_secondary_nvram(WebTVDisk disk, ObjectLocationType? selected)
        {
            if (disk.layout == DiskLayout.UTV)
            {
                this.add_utv_secondary_nvram_offsets(selected);
            }
            else if (disk.layout == DiskLayout.WEBSTAR)
            {
                this.add_webstar_secondary_nvram_offsets(selected);
            }
            else
            {
                this.add_lc2_secondary_nvram_offsets(selected);
            }
        }
    }
}
