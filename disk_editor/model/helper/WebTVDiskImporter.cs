using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace disk_editor
{
    class WebTVDiskImporter : WebTVIO
    {
        public delegate void writing_complete();
        public delegate void block_written(long bytes_written, long bytes_written_since, long bytes_total);

        public const int READ_SIZE = 131072;

        public FileStream file_reader { get; set; }

        public void import_image(block_written writing_callback = null, writing_complete complete_callback = null)
        {
            if (this.file_reader != null && this.io != null)
            {
                long bytes_written = 0;

                long bytes_total = Math.Min(this.file_reader.Length, this.io.Length);

                var buffer = new byte[WebTVBuildWriter.READ_SIZE];

                this.file_reader.Seek(0, SeekOrigin.Begin);

                do
                {
                    var read_size = this.file_reader.Read(buffer, 0, WebTVBuildWriter.READ_SIZE);

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

        public void open_import_file(string file_name)
        {
            this.file_reader = File.Open(file_name, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            if (this.file_reader == null)
            {
                throw new FileLoadException("Couldn't open image file for reading.");
            }
        }

        public WebTVDiskImporter(WebTVPartition part, ObjectLocation object_location, string file_name)
            : this(part.disk.io, get_object_bounds(part, object_location), file_name)
        {
        }

        public WebTVDiskImporter(WebTVDiskIO io, DiskCageBounds cage_bounds, string file_name)
            : base(io, cage_bounds)
        {
            this.open_import_file(file_name);
        }

        public void close_importer()
        {
            if (this.file_reader != null)
            {
                this.file_reader.Close();
                this.file_reader = null;
            }
        }

        ~WebTVDiskImporter()
        {
            this.Close();
        }
    }
}
