using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace disk_editor
{
    class WebTVIO
    {
        public Stream io;
        public WebTVDiskCollationConverter byte_converter = null;
        private bool is_caged_reader;
        private bool is_file_reader;

        public void set_converter(WebTVDiskCollationConverter byte_converter)
        {
            this.byte_converter = byte_converter;
        }

        public long Move(long move_size, SeekOrigin origin = SeekOrigin.Begin)
        {
            var current_position = this.io.Position;

            return this.io.Seek(current_position + move_size, origin);
        }

        public byte[] ReadBytes(uint count)
        {
            if (this.io != null)
            {
                byte[] bytes = new byte[count];

                this.Read(bytes, 0, this.io.Position, (int)count);
                if(!this.is_file_reader)
                {
                    this.Move((long)count);
                }

                return bytes;
            }
            else
            {
                return null;
            }
        }

        public char[] ReadChars(uint count)
        {
            byte[] data = this.ReadBytes(count);
            if (data != null)
            {
                return Encoding.Default.GetChars(data);
            }
            else
            {
                return null;
            }
        }

        public uint ReadUint32(bool big_endian = true)
        {
            byte[] data = this.ReadBytes(4);
            if (data != null)
            {
                if (big_endian)
                {
                    return BigEndianConverter.ToUInt32(data, 0);
                }
                else
                {
                    return BitConverter.ToUInt32(data, 0);
                }
            }
            else
            {
                return 0;
            }
        }

        public ushort ReadUint16(bool big_endian = true)
        {
            byte[] data = this.ReadBytes(2);
            if (data != null)
            {
                if (big_endian)
                {
                    return BigEndianConverter.ToUInt16(data, 0);
                }
                else
                {
                    return BitConverter.ToUInt16(data, 0);
                }
            }
            else
            {
                return 0;
            }
        }

        public int Read(byte[] buffer, int buffer_offset, long seek_offset, int read_size)
        {
            this.io.Seek(seek_offset, SeekOrigin.Begin);

            var read_ret = this.io.Read(buffer, buffer_offset, read_size);

            if (this.byte_converter != null)
            {
                this.byte_converter.convert_bytes(ref buffer, (uint)buffer_offset, (uint)read_size);
            }

            return read_ret;
        }

        public void WriteBytes(byte[] value)
        {
            if (this.io != null)
            {
                this.Write(value, 0, this.io.Position, value.Length);
                if(!this.is_file_reader)
                {
                    this.Move((long)value.Length);
                }
            }
        }

        public void WriteChars(char[] value)
        {
            this.WriteBytes(Encoding.Default.GetBytes(value));
        }

        public void WriteUint32(uint value)
        {
            this.WriteBytes(BigEndianConverter.GetBytes(value));
        }

        public void WriteUint16(ushort value)
        {
            this.WriteBytes(BigEndianConverter.GetBytes(value));
        }

        public void Write(byte[] buffer, int buffer_offset, long seek_offset, int write_size)
        {
            this.io.Seek(seek_offset, SeekOrigin.Begin);

            if (this.byte_converter != null)
            {
                this.byte_converter.convert_bytes(ref buffer, (uint)buffer_offset, (uint)write_size);
            }

            this.io.Write(buffer, buffer_offset, write_size);
        }

        public void Close()
        {
            if (this.io != null)
            {
                if (!this.is_caged_reader)
                {
                    this.io.Close();
                }
                else 
                {
                    this.io.Dispose();
                }
            }
        }

        public static DiskCageBounds get_object_bounds(ObjectLocation object_location)
        {
            DiskCageBounds cage_bounds;

            cage_bounds.start_position = object_location.offset;
            cage_bounds.end_position = object_location.offset + object_location.size_bytes;

            return cage_bounds;
        }

        public static DiskCageBounds get_object_bounds(WebTVPartition part, ObjectLocation object_location)
        {
            DiskCageBounds cage_bounds;

            cage_bounds.start_position = 0;
            cage_bounds.end_position = 0;

            if (object_location == null)
            {
                cage_bounds.start_position = (part.sector_start * part.disk.sector_bytes_length);
                cage_bounds.end_position = cage_bounds.start_position + (part.sector_length * part.disk.sector_bytes_length);

                //cage_bounds.start_position = 0x880600;
                //cage_bounds.end_position = 0x1080600;
            }
            else
            {
                cage_bounds.start_position = (part.sector_start * part.disk.sector_bytes_length) + object_location.offset;
                cage_bounds.end_position = cage_bounds.start_position + object_location.size_bytes;
            }

            return cage_bounds;
        }

        public static WebTVDiskCagedIO get_cio(WebTVPartition part, ObjectLocation object_location)
        {
            return new WebTVDiskCagedIO(part.disk.io, get_object_bounds(part, object_location));
        }

        public WebTVIO(WebTVPartition part, ObjectLocation object_location)
            : this(part.disk.io, get_object_bounds(part, object_location))
        {
        }

        public WebTVIO(WebTVPartition part) :
            this(part.disk.io, get_object_bounds(part, null))
        {
        }

        public WebTVIO(WebTVDiskIO io, DiskCageBounds cage_bounds)
        {
            this.is_caged_reader = true;
            this.io = new WebTVDiskCagedIO(io, cage_bounds);
        }

        public WebTVIO(string file_name)
        {
            FileStream file = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (file != null)
            {
                this.is_file_reader = true;
                this.io = file;

                var converter = new WebTVDiskCollationConverter();
                converter.byte_transform = converter.detect_build_byte_transform(this.io, 0);

                this.set_converter(converter);
            }
            else 
            {
                throw new FileLoadException("Couldn't open image file.");
            }
        }

        public WebTVIO(Stream reader)
        {
            this.io = reader;
        }
    }
}
