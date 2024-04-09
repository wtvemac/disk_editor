using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace disk_editor
{
    public enum TellyScriptState
    {
        UNKNOWN = -1,
        PACKED = 0,
        TOKENIZED = 1,
        RAW = 2
    };

    public enum TellyScriptType
    {
        ORIGINAL = 0,
        DIALSCRIPT = 1
    };

    [StructLayout(LayoutKind.Sequential, Size = TellyScript.PACKED_TELLYSCRIPT_HEADER_SIZE), Serializable]
    public unsafe struct packed_tellyscript_header
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] magic;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] version_major;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] version_minor;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] script_id;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] script_mod;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] compressed_data_length;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] decompressed_data_length; // tokenized data

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] decompressed_checksum; // tokenized data

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown1;
    }

    internal class TellyScript
    {
        public const int PACKED_TELLYSCRIPT_HEADER_SIZE = 0x24;
        public const uint UNKNOWN1_VALUE = 0x0101FFFF;

        public TellyScriptType tellyscript_type;

        public byte[] packed_data;
        public PackedTellyScriptHeader packed_header;
        public byte[] tokenized_data;
        public string raw_data;

        public uint crc32(byte[] data)
        {
            // from http://web.mit.edu/freebsd/head/sys/libkern/crc32.c
            //
            // COPYRIGHT (C) 1986 Gary S. Brown.  You may use this program, or
            // code or tables extracted from it, as desired without restriction.
            uint[] crc32_tab = new uint[] {
                0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f,
                0xe963a535, 0x9e6495a3, 0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988,
                0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91, 0x1db71064, 0x6ab020f2,
                0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
                0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9,
                0xfa0f3d63, 0x8d080df5, 0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172,
                0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b, 0x35b5a8fa, 0x42b2986c,
                0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
                0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423,
                0xcfba9599, 0xb8bda50f, 0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924,
                0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d, 0x76dc4190, 0x01db7106,
                0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
                0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d,
                0x91646c97, 0xe6635c01, 0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e,
                0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457, 0x65b0d9c6, 0x12b7e950,
                0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
                0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7,
                0xa4d1c46d, 0xd3d6f4fb, 0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0,
                0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9, 0x5005713c, 0x270241aa,
                0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
                0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81,
                0xb7bd5c3b, 0xc0ba6cad, 0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a,
                0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683, 0xe3630b12, 0x94643b84,
                0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
                0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb,
                0x196c3671, 0x6e6b06e7, 0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc,
                0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5, 0xd6d6a3e8, 0xa1d1937e,
                0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
                0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55,
                0x316e8eef, 0x4669be79, 0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236,
                0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f, 0xc5ba3bbe, 0xb2bd0b28,
                0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
                0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f,
                0x72076785, 0x05005713, 0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38,
                0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21, 0x86d3d2d4, 0xf1d4e242,
                0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
                0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69,
                0x616bffd3, 0x166ccf45, 0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2,
                0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db, 0xaed16a4a, 0xd9d65adc,
                0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
                0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693,
                0x54de5729, 0x23d967bf, 0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94,
                0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
            };

            uint crc = 0xFFFFFFFF;
            for (var i = 0; i < data.Length; i++)
            {
                crc = crc32_tab[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            }

            return crc;
            //return crc ^ 0xFFFFFFFF;
        }

        public packed_tellyscript_header unserialize_packed_tellscript_header()
        {
            GCHandle packed_header_handle = new GCHandle();

            try
            {
                packed_header_handle = GCHandle.Alloc(this.packed_data, GCHandleType.Pinned);
                var packed_header_pointer = packed_header_handle.AddrOfPinnedObject();

                return (packed_tellyscript_header)Marshal.PtrToStructure(packed_header_pointer, typeof(packed_tellyscript_header));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse packed TellyScript header. " + e.Message);
            }
            finally
            {
                if (packed_header_handle.IsAllocated)
                {
                    packed_header_handle.Free();
                }
            }
        }

        public byte[] serialize_packed_tellscript_header(packed_tellyscript_header _tellyscript_header)
        {
            int tellyscript_header_size = Marshal.SizeOf(_tellyscript_header);

            var tellyscript_header = new byte[tellyscript_header_size];

            var tellyscript_header_pointer = Marshal.AllocHGlobal(tellyscript_header_size);
            Marshal.StructureToPtr(_tellyscript_header, tellyscript_header_pointer, true);
            Marshal.Copy(tellyscript_header_pointer, tellyscript_header, 0, tellyscript_header_size);
            Marshal.FreeHGlobal(tellyscript_header_pointer);

            return tellyscript_header;
        }

        public byte[] unpack()
        {
            var packed_tellscript_header = this.unserialize_packed_tellscript_header();

            this.packed_header = new PackedTellyScriptHeader()
            {
                magic =
                    Convert.ToChar(packed_tellscript_header.magic[0]).ToString() +
                    Convert.ToChar(packed_tellscript_header.magic[1]).ToString() +
                    Convert.ToChar(packed_tellscript_header.magic[2]).ToString() +
                    Convert.ToChar(packed_tellscript_header.magic[3]).ToString(),
                version_major = BigEndianConverter.ToUInt32(packed_tellscript_header.version_major, 0),
                version_minor = BigEndianConverter.ToUInt32(packed_tellscript_header.version_minor, 0),
                script_id = BigEndianConverter.ToUInt32(packed_tellscript_header.script_id, 0),
                script_mod = BigEndianConverter.ToUInt32(packed_tellscript_header.script_mod, 0),
                compressed_data_length = BigEndianConverter.ToUInt32(packed_tellscript_header.compressed_data_length, 0),
                decompressed_data_length = BigEndianConverter.ToUInt32(packed_tellscript_header.decompressed_data_length, 0),
                decompressed_checksum = BigEndianConverter.ToUInt32(packed_tellscript_header.decompressed_checksum, 0),
                unknown1 = BigEndianConverter.ToUInt32(packed_tellscript_header.unknown1, 0)
            };

            var tellyscript_start_offset = TellyScript.PACKED_TELLYSCRIPT_HEADER_SIZE;
            byte[] compressed_data = new byte[this.packed_data.Length - tellyscript_start_offset];
            for (var i = 0; i < compressed_data.Length; i++)
            {
                compressed_data[i] = this.packed_data[tellyscript_start_offset + i];
            }

            var l = new LZSS();
            this.tokenized_data = l.Expand(compressed_data, (int)this.packed_header.decompressed_data_length);
            this.packed_header.actual_decompressed_checksum = this.crc32(this.tokenized_data);

            return this.tokenized_data;
        }

        public string detokenize()
        {
            this.raw_data = new TellyScriptDetokenizer(this.tokenized_data).detokenize();

            return this.raw_data;
        }

        public byte[] tokenize()
        {
            this.tokenized_data = new TellyScriptTokenizer(this.raw_data).tokenize();

            return this.tokenized_data;
        }

        public byte[] pack()
        {
            var l = new LZSS();
            var compressed_data = l.Compress(this.tokenized_data);
            var crc32 = this.crc32(this.tokenized_data);

            var random = new Random();
            uint script_id = 0;
            script_id = ((uint)random.Next(1 << 16) << 16) | (uint)random.Next(1 << 16);

            // Add identifier so that a server knows this is a TellyScript from this editor.
            uint end_value = 0x1B39; // 6969
            uint neg_bit = (script_id & 0x80000000);
            if (neg_bit == 0x80000000)
            {
                end_value = 0xDD67; // -6969
            }
            script_id = neg_bit | (((uint)((script_id & 0x7FFFFFFF) / 10000) * 10000) + end_value);

            this.packed_header = new PackedTellyScriptHeader()
            {
                magic = (this.tellyscript_type == TellyScriptType.DIALSCRIPT) ? "VKAT" : "ANDY",
                version_major = (this.packed_header != null && this.packed_header.version_major != 0) ? this.packed_header.version_major : 1,
                version_minor = (this.packed_header != null && this.packed_header.version_major != 0) ? this.packed_header.version_minor : 1,
                script_id = script_id,
                script_mod = (uint)DateTimeOffset.Now.ToUnixTimeSeconds(),
                compressed_data_length = (uint)compressed_data.Length,
                decompressed_data_length = (uint)this.tokenized_data.Length,
                decompressed_checksum = crc32,
                actual_decompressed_checksum = crc32,
                unknown1 = UNKNOWN1_VALUE
            };

            var packed_data = new byte[TellyScript.PACKED_TELLYSCRIPT_HEADER_SIZE + compressed_data.Length];

            var packed_header = this.serialize_packed_tellscript_header(
                new packed_tellyscript_header()
                {
                    magic = Encoding.Default.GetBytes(this.packed_header.magic),
                    version_major = BigEndianConverter.GetBytes(this.packed_header.version_major),
                    version_minor = BigEndianConverter.GetBytes(this.packed_header.version_minor),
                    script_id = BigEndianConverter.GetBytes(this.packed_header.script_id),
                    script_mod = BigEndianConverter.GetBytes(this.packed_header.script_mod),
                    compressed_data_length = BigEndianConverter.GetBytes(this.packed_header.compressed_data_length),
                    decompressed_data_length = BigEndianConverter.GetBytes(this.packed_header.decompressed_data_length),
                    decompressed_checksum = BigEndianConverter.GetBytes(this.packed_header.decompressed_checksum),
                    unknown1 = BigEndianConverter.GetBytes(this.packed_header.unknown1)
                }
            );

            for (var i = 0; i < packed_data.Length; i++)
            {
                if (i < TellyScript.PACKED_TELLYSCRIPT_HEADER_SIZE)
                {
                    packed_data[i] = packed_header[i];
                }
                else
                {
                    packed_data[i] = compressed_data[i - TellyScript.PACKED_TELLYSCRIPT_HEADER_SIZE];
                }
            }

            this.packed_data = packed_data;

            return this.packed_data;
        }

        public TellyScriptState auto_detect_state(byte[] tellyscript_data)
        {
            if (tellyscript_data.Length > 4)
            {
                var magic = BigEndianConverter.ToUInt32(tellyscript_data, 0);

                if (magic == 0x414E4459)
                {
                    this.tellyscript_type = TellyScriptType.ORIGINAL;

                    return TellyScriptState.PACKED;
                }
                else if (magic == 0x564B4154)
                {
                    this.tellyscript_type = TellyScriptType.DIALSCRIPT;

                    return TellyScriptState.PACKED;
                }
                else
                {
                    bool has_null = false;
                    bool has_eof = false;
                    for (var i = 0; i < tellyscript_data.Length; i++)
                    {
                        if (tellyscript_data[i] == 0x00)
                        {
                            has_null = true;
                        }
                        else if (tellyscript_data[i] == 0xFF)
                        {
                            has_eof = true;
                        }
                    }

                    if (has_null && has_eof)
                    {
                        return TellyScriptState.TOKENIZED;
                    }
                    else
                    {
                        return TellyScriptState.RAW;
                    }
                }
            }
            else
            {
                return TellyScriptState.UNKNOWN;
            }
        }

        public TellyScriptState auto_detect_state(string tellyscript_data)
        {
            return TellyScriptState.RAW;
        }

        public void process(byte[] tellyscript_data, TellyScriptState tellyscript_data_state)
        {
            if (tellyscript_data_state == TellyScriptState.UNKNOWN)
            {
                tellyscript_data_state = this.auto_detect_state(tellyscript_data);
            }

            if (tellyscript_data_state == TellyScriptState.PACKED)
            {
                this.packed_data = tellyscript_data;

                this.unpack();
                this.detokenize();
            }
            else if (tellyscript_data_state == TellyScriptState.TOKENIZED)
            {
                this.tokenized_data = tellyscript_data;

                this.pack();
                this.detokenize();
            }
            else if (tellyscript_data_state == TellyScriptState.RAW)
            {
                this.process(Encoding.GetEncoding(932).GetString(tellyscript_data), tellyscript_data_state);
            }
        }

        public void process(string tellyscript_data, TellyScriptState tellyscript_data_state)
        {
            if (tellyscript_data_state == TellyScriptState.UNKNOWN)
            {
                tellyscript_data_state = this.auto_detect_state(tellyscript_data);
            }

            if (tellyscript_data_state == TellyScriptState.PACKED)
            {
                this.process(Encoding.Default.GetBytes(tellyscript_data), tellyscript_data_state);
            }
            else if (tellyscript_data_state == TellyScriptState.TOKENIZED)
            {
                this.process(Encoding.Default.GetBytes(tellyscript_data), tellyscript_data_state);
            }
            else if (tellyscript_data_state == TellyScriptState.RAW)
            {
                this.raw_data = tellyscript_data;

                this.tokenize();
                this.pack();
            }
        }

        public TellyScript(byte[] tellyscript_data, TellyScriptState data_state = TellyScriptState.PACKED, TellyScriptType tellyscript_type = TellyScriptType.ORIGINAL)
        {
            this.tellyscript_type = tellyscript_type;

            this.process(tellyscript_data, data_state);
        }

        public TellyScript(string tellyscript_data, TellyScriptState data_state = TellyScriptState.PACKED, TellyScriptType tellyscript_type = TellyScriptType.ORIGINAL)
            : this(Encoding.Default.GetBytes(tellyscript_data), data_state, tellyscript_type)
        {
        }
    }
}
