using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace disk_editor
{
    class WebTVBuildWriter : WebTVIO
    {
        public delegate void writing_complete();
        public delegate void block_written(long bytes_written, long bytes_written_since, long bytes_total);

        public const int READ_SIZE = 1048576;

        public WebTVPartition part { get; set; }
        public ObjectLocation build_location { get; set; }
        public FileStream file_reader { get; set; }
        public uint build_code_checksum { get; set; }
        public bool use_provided_code_checksum { get; set; }
        public uint build_romfs_checksum { get; set; }
        public bool use_provided_romfs_checksum { get; set; }

        public void write_build(block_written writing_callback = null, writing_complete complete_callback = null)
        {
            if (this.file_reader != null && this.io != null)
            {
                int bytes_written = 0;
                uint romfs_checksum_offset = 0;
                int bytes_total = (int)Math.Min(this.file_reader.Length, this.io.Length);

                var buffer = new byte[WebTVBuildWriter.READ_SIZE];

                this.file_reader.Seek(0, SeekOrigin.Begin);
                
                do
                {
                    var read_size = this.file_reader.Read(buffer, 0, WebTVBuildWriter.READ_SIZE);

                    if (this.use_provided_romfs_checksum)
                    {
                        if (bytes_written == 0)
                        {
                            var romfs_base_address = BigEndianConverter.ToUInt32(buffer, 0x24);
                            var build_base_address = BigEndianConverter.ToUInt32(buffer, 0x30);

                            if (romfs_base_address > 0 && romfs_base_address != WebTVBuildInfo.NO_ROMFS && romfs_base_address > build_base_address)
                            {
                                romfs_checksum_offset = (romfs_base_address - build_base_address) - 4;
                            }
                        }

                        if (romfs_checksum_offset > 0 && (romfs_checksum_offset > bytes_written && romfs_checksum_offset < (bytes_written + read_size)))
                        {
                            var romfs_checksum_buffer_offset = (int)(romfs_checksum_offset - bytes_written);

                            Buffer.BlockCopy(BigEndianConverter.GetBytes(this.build_romfs_checksum), 0, buffer, romfs_checksum_buffer_offset, 4);
                        }
                    }

                    if (bytes_written == 0 && this.use_provided_code_checksum)
                    {
                        Buffer.BlockCopy(BigEndianConverter.GetBytes(this.build_code_checksum), 0, buffer, 0x8, 4);
                    }

                    this.Write(buffer, 0, bytes_written, read_size);

                    bytes_written += read_size;

                    if (writing_callback != null)
                    {
                        writing_callback.Invoke(bytes_written, read_size, bytes_total);
                    }
                } while (bytes_written < bytes_total);
            }

            if (complete_callback != null)
            {
                complete_callback.Invoke();
            }
        }

        public void set_code_checksum(uint build_code_checksum)
        {
            this.build_code_checksum = build_code_checksum;
            this.use_provided_code_checksum = true;
        }

        public void set_romfs_checksum(uint build_romfs_checksum)
        {
            this.build_romfs_checksum = build_romfs_checksum;
            this.use_provided_romfs_checksum = true;
        }

        public void unset_wchecksum()
        {
            this.unset_code_checksum();
            this.unset_romfs_checksum();
        }

        public void unset_code_checksum()
        {
            this.build_code_checksum = 0;
            this.use_provided_code_checksum = false;
        }

        public void unset_romfs_checksum()
        {
            this.build_romfs_checksum = 0;
            this.use_provided_romfs_checksum = false;
        }

        public void open_build_file(string file_name)
        {
            this.file_reader = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (this.file_reader != null)
            {
                var converter = new WebTVDiskCollationConverter();
                converter.byte_transform = converter.detect_build_byte_transform(this.file_reader, 0);

                this.set_converter(converter);
            }
            else
            {
                throw new FileLoadException("Couldn't open WebTV build image.");
            }
        }

        public WebTVBuildWriter(WebTVPartition part, ObjectLocation build_location, string file_name)
            : base(part, build_location)
        {
            this.part = part;
            this.build_location = build_location;
            this.open_build_file(file_name);
        }

        public void close_writer()
        {
            if (this.file_reader != null)
            {
                this.file_reader.Close();
                this.file_reader = null;
            }
        }

        ~WebTVBuildWriter()
        {
            this.Close();
        }

    }
}
