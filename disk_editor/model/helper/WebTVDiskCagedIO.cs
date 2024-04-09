using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace disk_editor
{
    public struct DiskCageBounds
    {
        public ulong start_position;
        public ulong end_position;
    }

    public class WebTVDiskCagedIO : Stream
    {
        public WebTVDiskIO parent_io { get; set; }
        public DiskCageBounds cage_bounds { get; set; }

        public override bool CanRead
        {
            get
            {
                return this.parent_io.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.parent_io.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this.parent_io.CanSeek;
            }
        }

        public override long Length
        {
            get
            {
                return (long)(this.cage_bounds.end_position - this.cage_bounds.start_position);
            }
        }

        public ulong _UPosition { get; set; }
        public override long Position
        {
            get
            {
                return (long)this._UPosition;
            }
            set
            {
                this._UPosition = (ulong)value;
            }
        }

        static DiskCageBounds get_cage_bounds(ulong start_position, ulong end_position)
        {
            DiskCageBounds cage_bounds;

            cage_bounds.start_position = start_position;
            cage_bounds.end_position = end_position;

            return cage_bounds;
        }

        public WebTVDiskCagedIO(WebTVDiskIO parent_io, DiskCageBounds cage_bounds)
        {
            this.parent_io = parent_io;
            this.cage_bounds = cage_bounds;

            if (this.parent_io == null || !this.parent_io.open_disk())
            {
                throw new FileNotFoundException("Disk stream not available.");
            }
        }

        public WebTVDiskCagedIO(WebTVPartition part) :
            this(part.disk.io, get_cage_bounds((part.sector_start) * part.disk.sector_bytes_length, (part.sector_start + part.sector_length) * part.disk.sector_bytes_length))
        {
        }

        public WebTVDiskCagedIO(WebTVDiskIO parent_io, ulong start_position, ulong end_position) :
            this(parent_io, get_cage_bounds(start_position, end_position))
        {
        }

        ~WebTVDiskCagedIO()
        {
            this.parent_io = null;
        }

        public override void SetLength(long value)
        {
            this.parent_io.SetLength(value);
        }

        public override void Close()
        {
            this.parent_io = null;
        }

        public override void Flush()
        {
            this.parent_io.Flush();
        }

        public override long Seek(long disk_offset, SeekOrigin origin)
        {
            this.Position = disk_offset;

            return this.Position;
        }

        public override int Read(byte[] buffer, int buffer_offset, int read_size)
        {
            ulong read_position = this.cage_bounds.start_position + this._UPosition;
            uint _read_size = (uint)read_size;

            if (read_position > this.cage_bounds.end_position)
            {
                return 0;
            }

            if ((read_position + _read_size) > this.cage_bounds.end_position)
            {
                _read_size = (uint)(this.cage_bounds.end_position - read_position);
            }

            return (int)this.parent_io.AbsRead(buffer, read_position, buffer_offset, _read_size);
        }

        public override void Write(byte[] buffer, int buffer_offset, int write_size)
        {
            ulong write_position = this.cage_bounds.start_position + this._UPosition;
            uint _write_size = (uint)write_size;

            if (write_position > this.cage_bounds.end_position)
            {
                return;
            }

            if ((write_position + _write_size) > this.cage_bounds.end_position)
            {
                _write_size = (uint)(this.cage_bounds.end_position - write_position);
            }

            this.parent_io.AbsWrite(buffer, write_position, buffer_offset, _write_size);
        }
    }
}
