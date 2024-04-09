using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LTR.IO.ImDisk;
using System.IO;

namespace disk_editor
{
    enum IMDPROXY_REQ : ulong
    {
        IMDPROXY_REQ_NULL,
        IMDPROXY_REQ_INFO,
        IMDPROXY_REQ_READ,
        IMDPROXY_REQ_WRITE,
        IMDPROXY_REQ_CONNECT,
        IMDPROXY_REQ_CLOSE
    }

    enum IMDPROXY_FLAGS : ulong
    {
        IMDPROXY_FLAG_NONE = 0,
        IMDPROXY_FLAG_RO = 1
    }

    // <summary>
    // Constants used in connection with ImDisk/Devio proxy communication.
    // </summary>
    public static class IMDPROXY_CONSTANTS
    {
        // <summary>
        // Header size when communicating using a shared memory object.
        // </summary>
        public const int IMDPROXY_HEADER_SIZE = 4096;

        // <summary>
        // Default required alignment for I/O operations.
        // </summary>
        public const int REQUIRED_ALIGNMENT = 1;
    }

    public class ImDiskInfo
    {
        public const string HUMAN_DOWNLOAD_URL = "http://www.ltr-data.se/opencode.html/#ImDisk";

        public bool IsInstalled()
        {
            try
            {
                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "imdisk.exe")))
                {
                    return true;
                }
                else
                {
                    throw new FileNotFoundException("ImDisk is not installed.");
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
