using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace disk_editor
{
    class WebTVDiskExporter : WebTVIO
    {
        public delegate void writing_complete();
        public delegate void block_written(long bytes_written, long bytes_written_since, long bytes_total);

        public const int READ_SIZE = 131072;

        public FileStream file_writer { get; set; }

        public void export_image(block_written writing_callback = null, writing_complete complete_callback = null)
        {
            if (this.file_writer != null && this.io != null)
            {
                long bytes_written = 0;
                long bytes_total = this.io.Length;

                var buffer = new byte[WebTVBuildWriter.READ_SIZE];

                do
                {
                    var read_size = this.Read(buffer, 0, bytes_written, WebTVBuildWriter.READ_SIZE);
                    
                    this.file_writer.Write(buffer, 0, read_size);

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

        public void open_export_file(string file_name)
        {
            this.file_writer = File.Open(file_name, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            if (this.file_writer == null)
            {
                throw new FileLoadException("Couldn't open image file for writing.");
            }
        }

        public WebTVDiskExporter(WebTVPartition part, ObjectLocation object_location, string file_name)
            : this(part.disk.io, get_object_bounds(part, object_location), file_name)
        {
        }

        public WebTVDiskExporter(WebTVDiskIO io, DiskCageBounds cage_bounds, string file_name)
            : base(io, cage_bounds)
        {
            this.open_export_file(file_name);
        }

        public void close_exporter()
        {
            if (this.file_writer != null)
            {
                this.file_writer.Close();
                this.file_writer = null;
            }
        }

        ~WebTVDiskExporter()
        {
            this.close_exporter();
        }
    }
}
