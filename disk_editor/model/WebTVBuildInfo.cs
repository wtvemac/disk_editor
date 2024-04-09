using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace disk_editor
{
    [StructLayout(LayoutKind.Sequential, Size = WebTVBuildInfo.ROMFS_FOOTER_SIZE), Serializable]
    public unsafe struct webtv_romfs_footer
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_romfs_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] romfs_checksum;
    }

    [StructLayout(LayoutKind.Sequential, Size = WebTVBuildInfo.BUILD_HEADER_SIZE), Serializable]
    public unsafe struct webtv_build_header
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] jump_instruction;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] pre_jump_instruction;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] code_checksum;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_length;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_code_length;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] build_number;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] heap_data_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_heap_data_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_heap_free_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] romfs_base_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown1;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] unknown2;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] build_base_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] build_flags;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_heap_compressed_data_size;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] code_compressed_address;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] dword_code_compressed_size;
    }

    class WebTVBuildInfo : WebTVIO
    {
        public const int BUILD_HEADER_SIZE = 0x44;
        public const int ROMFS_FOOTER_SIZE = 0x8;
        public const uint NO_ROMFS = 0x4E6F4653;
        public const uint MAX_BUILD_SIZE = 0x2000000;

        public uint calculate_code_checksum()
        {
            var build_header = this.unserialize_build_header();

            var build_length = (BigEndianConverter.ToUInt32(build_header.dword_length, 0) - 3) * WORD.DWORD_SIZE_BYTES;
            var build_code_length = (BigEndianConverter.ToUInt32(build_header.dword_code_length, 0) - 3) * WORD.DWORD_SIZE_BYTES;

            if (build_code_length > build_length)
            {
                build_code_length = build_length;
            }

            if (build_code_length > WebTVBuildInfo.MAX_BUILD_SIZE)
            {
                build_code_length = WebTVBuildInfo.MAX_BUILD_SIZE;
            }

            var build_code = new byte[build_code_length];

            this.Read(build_code, 0, 0xC, (int)build_code_length);

            uint checksum = BigEndianConverter.ToUInt32(build_header.jump_instruction, 0);
            for (int i = 0; i < build_code_length; i += 4)
            {
                var build_block = BigEndianConverter.ToUInt32(build_code, i);

                checksum += build_block;
            }

            return checksum;
        }

        public uint calculate_romfs_checksum()
        {
            var build_header = this.unserialize_build_header();

            var romfs_base_address = BigEndianConverter.ToUInt32(build_header.romfs_base_address, 0);

            if (romfs_base_address != WebTVBuildInfo.NO_ROMFS)
            {
                var build_base_address = BigEndianConverter.ToUInt32(build_header.build_base_address, 0);
                var build_length = (BigEndianConverter.ToUInt32(build_header.dword_length, 0) - 3) * WORD.DWORD_SIZE_BYTES;

                try
                {
                    var romfs_footer = this.unserialize_romfs_footer(build_base_address, romfs_base_address);

                    var romfs_length = (BigEndianConverter.ToUInt32(romfs_footer.dword_romfs_size, 0)) * WORD.DWORD_SIZE_BYTES;

                    if (romfs_length > build_length)
                    {
                        romfs_length = build_length - WebTVBuildInfo.ROMFS_FOOTER_SIZE;
                    }

                    if (romfs_length > WebTVBuildInfo.MAX_BUILD_SIZE)
                    {
                        romfs_length = WebTVBuildInfo.MAX_BUILD_SIZE - WebTVBuildInfo.ROMFS_FOOTER_SIZE;
                    }

                    var build_code = new byte[romfs_length];

                    var romfs_offset = this.get_romfs_base_offset(build_base_address, romfs_base_address);

                    this.Read(build_code, 0, (romfs_offset - WebTVBuildInfo.ROMFS_FOOTER_SIZE - romfs_length), (int)romfs_length);

                    uint checksum = 0;
                    for (int i = 0; i < romfs_length; i += 4)
                    {
                        var build_block = BigEndianConverter.ToUInt32(build_code, i);

                        checksum += build_block;
                    }

                    return checksum;
                }
                catch
                {
                    return 0;
                }

            }

            return 0;
        }

        public uint get_romfs_base_offset(uint build_base_address, uint romfs_base_address)
        {
            if (romfs_base_address > build_base_address)
            {
                return (romfs_base_address - build_base_address);
            }
            else 
            {
                return 0;
            }
        }

        public WebTVBuild get_build_info()
        {
            var build_header = this.unserialize_build_header();

            bool is_classic_build = false;
            uint jump_instruction = 0;
            uint pre_jump_instruction = 0;
            uint jump_offset = 0;
            uint code_checksum = 0;
            uint dword_length = 0;
            uint dword_code_length = 0;
            uint build_number = 0;
            uint build_flags = 0;
            uint build_base_address = 0;
            uint romfs_base_address = 0;
            uint romfs_checksum = 0;
            uint dword_romfs_size = 0;
            uint heap_data_address = 0;
            uint dword_heap_data_size = 0;
            uint dword_heap_free_size = 0;
            uint dword_heap_compressed_data_size = 0;
            uint code_compressed_address = 0;
            uint dword_code_compressed_size = 0;

            if(build_header.jump_instruction[0] == 0x10)
            {
                jump_instruction = BigEndianConverter.ToUInt32(build_header.jump_instruction, 0);
                pre_jump_instruction = BigEndianConverter.ToUInt32(build_header.pre_jump_instruction, 0);
                code_checksum = BigEndianConverter.ToUInt32(build_header.code_checksum, 0);
                dword_length = BigEndianConverter.ToUInt32(build_header.dword_length, 0);
                dword_code_length = BigEndianConverter.ToUInt32(build_header.dword_code_length, 0);
                build_number = BigEndianConverter.ToUInt32(build_header.build_number, 0);
                build_flags = BigEndianConverter.ToUInt32(build_header.build_flags, 0);
                build_base_address = BigEndianConverter.ToUInt32(build_header.build_base_address, 0);
                romfs_base_address = BigEndianConverter.ToUInt32(build_header.romfs_base_address, 0);
                heap_data_address = BigEndianConverter.ToUInt32(build_header.heap_data_address, 0);
                dword_heap_data_size = BigEndianConverter.ToUInt32(build_header.dword_heap_data_size, 0);
                dword_heap_free_size = BigEndianConverter.ToUInt32(build_header.dword_heap_free_size, 0);
                dword_heap_compressed_data_size = BigEndianConverter.ToUInt32(build_header.dword_heap_compressed_data_size, 0);
                code_compressed_address = BigEndianConverter.ToUInt32(build_header.code_compressed_address, 0);
                dword_code_compressed_size = BigEndianConverter.ToUInt32(build_header.dword_code_compressed_size, 0);

                if (pre_jump_instruction == 0x202020)
                {
                    is_classic_build = true;
                }

                if (dword_code_length > dword_length)
                {
                    dword_code_length = 0;
                }

                jump_offset = ((jump_instruction & 0xFFFF) << 2) + 4;

                if (romfs_base_address == WebTVBuildInfo.NO_ROMFS)
                {
                    romfs_base_address = 0;
                }
                else
                {
                    try
                    {
                        var romfs_footer = this.unserialize_romfs_footer(build_base_address, romfs_base_address);

                        romfs_checksum = BigEndianConverter.ToUInt32(romfs_footer.romfs_checksum, 0);
                        dword_romfs_size = BigEndianConverter.ToUInt32(romfs_footer.dword_romfs_size, 0);

                        if (dword_romfs_size > dword_length)
                        {
                            dword_romfs_size = 0;
                        }
                    }
                    catch
                    {
                        romfs_checksum = 0;
                        dword_romfs_size = 0;
                    }
                }
            }

            return new WebTVBuild()
            {
                is_classic_build = is_classic_build,
                jump_instruction = jump_instruction,
                pre_jump_instruction = pre_jump_instruction,
                jump_offset = jump_offset,
                code_checksum = code_checksum,
                dword_length = dword_length,
                dword_code_length = dword_code_length,
                build_number = build_number,
                build_flags = build_flags,
                build_base_address = build_base_address,
                romfs_base_address = romfs_base_address,
                romfs_checksum = romfs_checksum,
                dword_romfs_size = dword_romfs_size,
                heap_data_address = heap_data_address,
                dword_heap_data_size = dword_heap_data_size,
                dword_heap_free_size = dword_heap_free_size,
                dword_heap_compressed_data_size = dword_heap_compressed_data_size,
                code_compressed_address = code_compressed_address,
                dword_code_compressed_size = dword_code_compressed_size
            };
        }

        public webtv_romfs_footer unserialize_romfs_footer()
        {
            var build_header = this.unserialize_build_header();

            uint build_base_address = BigEndianConverter.ToUInt32(build_header.build_base_address, 0);
            uint romfs_base_address = BigEndianConverter.ToUInt32(build_header.romfs_base_address, 0);

            return this.unserialize_romfs_footer(build_base_address, romfs_base_address);
        }

        public webtv_romfs_footer unserialize_romfs_footer(uint build_base_address, uint romfs_base_address)
        {
            GCHandle romfs_footer_handle = new GCHandle();

            try
            {
                if(romfs_base_address != WebTVBuildInfo.NO_ROMFS)
                {
                    var romfs_offset = this.get_romfs_base_offset(build_base_address, romfs_base_address);

                    if (romfs_offset > 0 && romfs_offset <= this.io.Length)
                    {
                        var romfs_footer_data = new byte[WebTVBuildInfo.ROMFS_FOOTER_SIZE];
                        
                        this.Read(romfs_footer_data, 0, (romfs_offset - (long)WebTVBuildInfo.ROMFS_FOOTER_SIZE), WebTVBuildInfo.ROMFS_FOOTER_SIZE);


                        romfs_footer_handle = GCHandle.Alloc(romfs_footer_data, GCHandleType.Pinned);
                        var romfs_footer_pointer = romfs_footer_handle.AddrOfPinnedObject();

                        return (webtv_romfs_footer)Marshal.PtrToStructure(romfs_footer_pointer, typeof(webtv_romfs_footer));
                    }
                    else
                    {
                        throw new DataMisalignedException("Couldn't seek to ROMFS base because of an invalid address.");
                    }
                }
                else
                {
                    throw new InvalidDataException("No ROMFS available.");
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse WebTV build. " + e.Message);
            }
            finally
            {
                if (romfs_footer_handle.IsAllocated)
                {
                    romfs_footer_handle.Free();
                }
            }
        }

        public webtv_build_header unserialize_build_header()
        {
            GCHandle build_header_handle = new GCHandle();

            try
            {
                var build_header_data = new byte[WebTVBuildInfo.BUILD_HEADER_SIZE];

                this.Read(build_header_data, 0, 0, WebTVBuildInfo.BUILD_HEADER_SIZE);

                build_header_handle = GCHandle.Alloc(build_header_data, GCHandleType.Pinned);
                var build_header_pointer = build_header_handle.AddrOfPinnedObject();

                return (webtv_build_header)Marshal.PtrToStructure(build_header_pointer, typeof(webtv_build_header));
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Couldn't parse WebTV build. " + e.Message);
            }
            finally
            {
                if (build_header_handle.IsAllocated)
                {
                    build_header_handle.Free();
                }
            }
        }

        public WebTVBuildInfo(WebTVPartition part, ObjectLocation build_location)
            : base(part, build_location)
        {
        }

        public WebTVBuildInfo(string file_name)
            : base(file_name)
        {
        }

        public WebTVBuildInfo(Stream reader)
            : base(reader)
        {
        }
    }
}
