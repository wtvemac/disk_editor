using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using LTR.IO.ImDisk;

namespace disk_editor
{
    public class ImDiskNamedPipeServer
    {
        private const uint NO_DEVICE = 0xFFFFFFFF;

        public delegate void notify_callback(bool success = true, string message = "");

        public string name;
        public string path;
        public bool read_only;
        public uint device_id;
        private NamedPipeServerStream named_server;
        private WebTVDiskCagedIO cio;
        public notify_callback unmount_callback;
        public notify_callback mounted_callback;
        private string mount_point;
        private string unmount_error_message;
        private string mount_error_message;
        private bool can_force_unmount = true;

        public ImDiskNamedPipeServer(WebTVPartition part, string mount_point = null, bool read_only = false, notify_callback unmount_callback = null, notify_callback mounted_callback = null)
        {
            this.device_id = ImDiskNamedPipeServer.NO_DEVICE;

            try
            {
                this.unmount_callback = unmount_callback;
                this.mounted_callback = mounted_callback;

                this.start_server(this.get_cio(part), part.id.ToString(), mount_point, read_only);
            }
            catch (Exception e)
            {
                throw new ApplicationException("Couldn't create partition device. " + e.Message);
            }
        }

        ~ImDiskNamedPipeServer()
        {
            this.internal_unmount();
        }

        public void mount_thread(object _mount_point)
        {
            lock (this)
            {
                try
                {
                    this.mount((string)_mount_point);
                }
                catch (Exception e)
                {
                    lock(this)
                    {
                        this.mount_error_message = e.Message;
                    }
                }
            }
        }

        public void async_mount(string mount_point)
        {
            this.mount_point = mount_point;

            if (this.mount_point != null)
            {
                var mount_thread = new Thread(new ParameterizedThreadStart(this.mount_thread));
                mount_thread.Start(mount_point);

                if (!mount_thread.Join(5000))
                {
                    string error_msg = "Took too long to mount partition.";

                    this.internal_unmount();
                    this.stop_server();
                    if (this.mounted_callback != null)
                    {
                        this.mounted_callback.Invoke(false, error_msg);
                    }
                    throw new ApplicationException(error_msg);
                }
                else if (this.device_id == ImDiskNamedPipeServer.NO_DEVICE)
                {
                    string error_msg = "Couldn't mount partition. " + this.mount_error_message;

                    this.stop_server();
                    if (this.mounted_callback != null)
                    {
                        this.mounted_callback.Invoke(false, error_msg);
                    }
                    throw new ApplicationException(error_msg);
                }
                else if(this.mounted_callback != null)
                {
                    this.mounted_callback.Invoke(true, "");
                }
            }
        }

        public void mount(string mount_point)
        {
            if(mount_point != null)
            {
                var flags = ImDiskFlags.DeviceTypeHD | ImDiskFlags.TypeProxy | ImDiskFlags.ProxyTypeDirect;

                if (this.read_only)
                {
                    flags |= ImDiskFlags.ReadOnly;
                }


                ImDiskAPI.CreateDevice(this.cio.Length,
                                        0,
                                        0,
                                        this.cio.parent_io.disk.sector_bytes_length,
                                        0,
                                        flags,
                                        this.path,
                                        true,
                                        mount_point,
                                        ref this.device_id,
                                        IntPtr.Zero);

                this.notify_change();
            }
        }

        private void internal_unmount()
        {
            try
            {
                this.unmount();
            }
            catch
            {
                throw new ApplicationException("Couldn't unmount!");
            }
        }

        public void remove_drive_letter()
        {
            var drive_letter = this.get_drive_letter();

            if (drive_letter != "")
            {
                ImDiskAPI.RemoveMountPoint(drive_letter + ":");
            }
        }

        private void notify_change()
        {
            var refresh_event = ImDiskAPI.OpenRefreshEvent();
            refresh_event.Notify();
        }

        private void clear_device()
        {
            this.notify_change();

            this.device_id = 0;

            this.stop_server();
        }

        public void force_unmount()
        {
            try
            {
                this.remove_drive_letter();

                ImDiskAPI.ForceRemoveDevice(this.device_id);
                this.clear_device();
            }
            catch (Exception e2)
            {
                this.unmount_error_message = e2.Message;

                throw new ApplicationException("Couldn't force unmount disk! " + e2.Message);
            }
        }

        public void unmount()
        {
            if (this.device_id != ImDiskNamedPipeServer.NO_DEVICE)
            {
                try
                {
                    //this.remove_drive_letter();

                    ImDiskAPI.RemoveDevice(this.device_id);
                    this.clear_device();
                }
                catch (Exception e)
                {
                    uint hres = (uint)Marshal.GetHRForLastWin32Error();

                    if (hres == 0x80070015 || hres == 0x80070002) // Device wasn't available, so no need to unmount.
                    {
                        this.clear_device();
                    }
                    else
                    {
                        if (this.can_force_unmount)
                        {
                            this.force_unmount();
                        }
                        else
                        {
                            this.unmount_error_message = e.Message;

                            throw new ApplicationException("Couldn't unmount disk! " + e.Message);
                        }
                    }

                }

                this.clear_device();

                if (this.unmount_callback != null)
                {
                    this.unmount_callback.Invoke(true, "");
                }
            }
        }

        public string get_drive_letter()
        {
            try
            {
                var imdisk_device = ImDiskAPI.QueryDevice(this.device_id);

                return imdisk_device.DriveLetter.ToString();
            }
            catch {}

            return "";
        }

        private WebTVDiskCagedIO get_cio(WebTVPartition part)
        {
            ulong partition_start_offset = (part.sector_start * part.disk.sector_bytes_length) + WebTVPartitionManager.PARTITON_DATA_OFFSET;
            ulong partition_end_offset = (part.sector_start + part.sector_length) * part.disk.sector_bytes_length;

            var cio = new WebTVDiskCagedIO(part.disk.io, partition_start_offset, partition_end_offset);

            return cio;
        }

        private void start_server(WebTVDiskCagedIO cio, string name, string mount_point = null, bool read_only = false)
        {
            this.name = "wtv_" + name;
            this.path = @"\Device\NamedPipe\" + this.name;
            this.cio = cio;
            this.read_only = read_only;

            this.named_server = new NamedPipeServerStream(this.name, 
                                                          PipeDirection.InOut, 
                                                          1, 
                                                          PipeTransmissionMode.Byte,
                                                          PipeOptions.WriteThrough | PipeOptions.Asynchronous);

            this.named_server.BeginWaitForConnection(new AsyncCallback(named_connection), this.named_server);

            this.async_mount(mount_point);
        }

        public void stop_server()
        {
            if (this.named_server != null)
            {
                named_server.Close();

                this.named_server = null;
            }

            if (this.cio != null)
            {
                this.cio.Dispose();
                this.cio = null;
            }
        }

        private void named_connection(IAsyncResult result)
        {
            var named_server = (NamedPipeServerStream)result.AsyncState;

            if (named_server != null)
            {
                named_server.EndWaitForConnection(result);

                var server_thread = new Thread(new ParameterizedThreadStart(this.server_loop));
                server_thread.Start(named_server);
            }
        }

        public void server_loop(object param_pass)
        {
            var named_server = (NamedPipeServerStream)param_pass;

            if (named_server != null)
            {
                var writer = new BinaryWriter(named_server, Encoding.Default);
                var reader = new BinaryReader(named_server, Encoding.Default);

                while (true)
                {
                    var exit_loop = false;

                    try
                    {
                        var request_code = (IMDPROXY_REQ)reader.ReadUInt64();

                        switch (request_code)
                        {
                            case IMDPROXY_REQ.IMDPROXY_REQ_INFO:
                                send_info(writer);
                                break;

                            case IMDPROXY_REQ.IMDPROXY_REQ_READ:
                                read_data(reader, writer);
                                break;

                            case IMDPROXY_REQ.IMDPROXY_REQ_WRITE:
                                write_data(reader, writer);
                                break;

                            case IMDPROXY_REQ.IMDPROXY_REQ_CLOSE:
                                exit_loop = true;
                                break;
                        }
                    }
                    catch
                    {
                        exit_loop = true;
                    }

                    if (exit_loop)
                    {
                        break;
                    }
                }
            }

            lock (this)
            {
                this.internal_unmount();
            }
        }

        public void send_info(BinaryWriter writer)
        {
            writer.Write((ulong)cio.Length);
            writer.Write((ulong)1);
            if (read_only)
            {
                writer.Write((ulong)IMDPROXY_FLAGS.IMDPROXY_FLAG_RO);
            }
            else 
            {
                writer.Write((ulong)IMDPROXY_FLAGS.IMDPROXY_FLAG_NONE);
            }
        }

        public void read_data(BinaryReader reader, BinaryWriter writer)
        {
            var offset = reader.ReadUInt64();
            var length = reader.ReadUInt64();

            var buffer = new byte[length];

            ulong error_code = 0;
            ulong read_length = 0;

            try
            {
                this.cio.Seek((long)offset, SeekOrigin.Begin);

                read_length = (ulong)this.cio.Read(buffer, 0, (int)length);
            }
            catch 
            {
                error_code = 1;
                read_length = 0;
            }

            writer.Write(error_code);
            writer.Write(read_length);
            if (read_length > 0)
            {
                writer.Write(buffer, 0, (int)read_length);
            }
        }

        public void write_data(BinaryReader reader, BinaryWriter writer)
        {
            var offset = reader.ReadUInt64();
            var length = reader.ReadUInt64();

            var buffer = new byte[length];

            ulong error_code = 0;
            ulong write_length = 0;

            try
            {
                write_length = (ulong)reader.Read(buffer, 0, (int)length);

                this.cio.Seek((long)offset, SeekOrigin.Begin);

                this.cio.Write(buffer, 0, (int)write_length);
            }
            catch
            {
                error_code = 1;
                write_length = 0;
            }

            writer.Write(error_code);
            writer.Write(write_length);
        }
    }
}
