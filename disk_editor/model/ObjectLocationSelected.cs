using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace disk_editor
{
    internal class ObjectLocationSelected : WebTVIO
    {
        public const uint LC2_BROWSER_SELECT_OFFSET = 0x01080600;
        public const uint UTV_BROWSER_SELECT_OFFSET = 0x17480600;
        public const uint LC2_PRIMARY_NVRAM_SELECT_OFFSET = 0x10C0C00;
        public const uint UTV_PRIMARY_NVRAM_SELECT_OFFSET = 0x174C0C00;
        public const uint LC2_SECONDARY_NVRAM_SELECT_OFFSET = 0x10C0E00;
        public const uint UTV_SECONDARY_NVRAM_SELECT_OFFSET = 0x174C0E00;

        public WebTVDisk disk = null;
        public ObjectLocationCategory category = ObjectLocationCategory.BUILD;

        public byte read_selected(uint offset)
        {
            var block = new byte[512];

            this.Read(block, 0, offset, 512);

            return block[0];
        }

        public void write_selected(uint offset, byte selected)
        {
            var block = new byte[512];

            block[0] = selected;

            this.Write(block, 0, offset, 512);
        }

        public ObjectLocationType get_lc2_selected_browser()
        {
            var selected = this.read_selected(LC2_BROWSER_SELECT_OFFSET);

            if(selected == 0x00)
            {
                return ObjectLocationType.BROWSER0_LOCATION;
            }
            else
            {
                return ObjectLocationType.BROWSER1_LOCATION;
            }
        }

        public ObjectLocationType get_lc2_selected_primary_nvram()
        {
            var selected = this.read_selected(LC2_PRIMARY_NVRAM_SELECT_OFFSET);

            if (selected == 0x00)
            {
                return ObjectLocationType.NVRAM_PRIMARY0;
            }
            else
            {
                return ObjectLocationType.NVRAM_PRIMARY1;
            }
        }

        public ObjectLocationType get_lc2_selected_secondary_nvram()
        {
            var selected = this.read_selected(LC2_SECONDARY_NVRAM_SELECT_OFFSET);

            if (selected == 0x00)
            {
                return ObjectLocationType.NVRAM_SECONDARY0;
            }
            else
            {
                return ObjectLocationType.NVRAM_SECONDARY1;
            }
        }

        public ObjectLocationType get_utv_selected_browser()
        {
            var selected = this.read_selected(UTV_BROWSER_SELECT_OFFSET);

            if (selected == 0x00)
            {
                return ObjectLocationType.BROWSER0_LOCATION;
            }
            else
            {
                return ObjectLocationType.BROWSER1_LOCATION;
            }
        }

        public ObjectLocationType get_utv_selected_primary_nvram()
        {
            var selected = this.read_selected(UTV_PRIMARY_NVRAM_SELECT_OFFSET);

            if (selected == 0x00)
            {
                return ObjectLocationType.NVRAM_PRIMARY0;
            }
            else
            {
                return ObjectLocationType.NVRAM_PRIMARY1;
            }
        }

        public ObjectLocationType get_utv_selected_secondary_nvram()
        {
            var selected = this.read_selected(UTV_SECONDARY_NVRAM_SELECT_OFFSET);

            if (selected == 0x00)
            {
                return ObjectLocationType.NVRAM_SECONDARY0;
            }
            else
            {
                return ObjectLocationType.NVRAM_SECONDARY1;
            }
        }

        public ObjectLocationType get_selected()
        {
            if(category == ObjectLocationCategory.BUILD)
            {
                if(this.disk.layout == DiskLayout.LC2 || this.disk.layout == DiskLayout.WEBSTAR)
                {
                    return this.get_lc2_selected_browser();
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    return this.get_utv_selected_browser();
                }

                return ObjectLocationType.BROWSER0_LOCATION;
            }
            else if (category == ObjectLocationCategory.PRIMARY_NVRAM)
            {
                if (this.disk.layout == DiskLayout.LC2 || this.disk.layout == DiskLayout.WEBSTAR)
                {
                    return this.get_lc2_selected_primary_nvram();
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    return this.get_utv_selected_primary_nvram();
                }

                return ObjectLocationType.NVRAM_PRIMARY0;
            }
            else if (category == ObjectLocationCategory.SECONDARY_NVRAM)
            {
                if (this.disk.layout == DiskLayout.LC2 || this.disk.layout == DiskLayout.WEBSTAR)
                {
                    return this.get_lc2_selected_secondary_nvram();
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    return this.get_utv_selected_secondary_nvram();
                }

                return ObjectLocationType.NVRAM_SECONDARY0;
            }

            return ObjectLocationType.UNKNOWN;
        }

        public void set_lc2_selected_browser(ObjectLocationType type)
        {

            if (type == ObjectLocationType.BROWSER0_LOCATION)
            {
                this.write_selected(LC2_BROWSER_SELECT_OFFSET, 0);
            }
            else
            {
                this.write_selected(LC2_BROWSER_SELECT_OFFSET, 1);
            }
        }
        public void set_lc2_selected_primary_nvram(ObjectLocationType type)
        {

            if (type == ObjectLocationType.NVRAM_PRIMARY0)
            {
                this.write_selected(LC2_PRIMARY_NVRAM_SELECT_OFFSET, 0);
            }
            else
            {
                this.write_selected(LC2_PRIMARY_NVRAM_SELECT_OFFSET, 1);
            }
        }

        public void set_lc2_selected_secondary_nvram(ObjectLocationType type)
        {

            if (type == ObjectLocationType.NVRAM_SECONDARY0)
            {
                this.write_selected(LC2_SECONDARY_NVRAM_SELECT_OFFSET, 0);
            }
            else
            {
                this.write_selected(LC2_SECONDARY_NVRAM_SELECT_OFFSET, 1);
            }
        }

        public void set_utv_selected_browser(ObjectLocationType type)
        {

            if (type == ObjectLocationType.BROWSER0_LOCATION)
            {
                this.write_selected(UTV_BROWSER_SELECT_OFFSET, 0);
            }
            else
            {
                this.write_selected(UTV_BROWSER_SELECT_OFFSET, 1);
            }
        }
        public void set_utv_selected_primary_nvram(ObjectLocationType type)
        {

            if (type == ObjectLocationType.NVRAM_PRIMARY0)
            {
                this.write_selected(UTV_PRIMARY_NVRAM_SELECT_OFFSET, 0);
            }
            else
            {
                this.write_selected(UTV_PRIMARY_NVRAM_SELECT_OFFSET, 1);
            }
        }

        public void set_utv_selected_secondary_nvram(ObjectLocationType type)
        {

            if (type == ObjectLocationType.NVRAM_SECONDARY0)
            {
                this.write_selected(UTV_SECONDARY_NVRAM_SELECT_OFFSET, 0);
            }
            else
            {
                this.write_selected(UTV_SECONDARY_NVRAM_SELECT_OFFSET, 1);
            }
        }

        public void set_selected(ObjectLocationType type)
        {
            if (category == ObjectLocationCategory.BUILD)
            {
                if (this.disk.layout == DiskLayout.LC2 || this.disk.layout == DiskLayout.WEBSTAR)
                {
                    this.set_lc2_selected_browser(type);
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    this.set_utv_selected_browser(type);
                }
            }
            else if (category == ObjectLocationCategory.PRIMARY_NVRAM)
            {
                if (this.disk.layout == DiskLayout.LC2 || this.disk.layout == DiskLayout.WEBSTAR)
                {
                    this.set_lc2_selected_primary_nvram(type);
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    this.set_utv_selected_primary_nvram(type);
                }
            }
            else if (category == ObjectLocationCategory.SECONDARY_NVRAM)
            {
                if (this.disk.layout == DiskLayout.LC2 || this.disk.layout == DiskLayout.WEBSTAR)
                {
                    this.set_lc2_selected_secondary_nvram(type);
                }
                else if (this.disk.layout == DiskLayout.UTV)
                {
                    this.set_utv_selected_secondary_nvram(type);
                }
            }

        }

        public ObjectLocationSelected(ObjectLocationCategory category, WebTVDisk disk)
            : base(disk.io)
        {
            this.disk = disk;
            this.category = category;
        }
    }
}
