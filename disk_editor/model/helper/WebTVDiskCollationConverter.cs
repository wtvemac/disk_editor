using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace disk_editor
{
    public enum DiskByteTransform
    {
        // NULL value
        NONE = -1,

        // Read or write as-is
        NOSWAP = 0,

        // 16-bit little endien
        // Older WebTV boxes. [1234=>2143]
        BIT16SWAP = 1,

        // 16-bit little endien step with 32-bit little endien step
        // UltimateTV boxes (possibly other WinCE-based boxes). [1234=>3412]
        BIT1632SWAP = 3,

        // 32-bit little endien
        // No boxes known but support is here. [1234=>4321]
        BIT32SWAP = 2
    };

    public class WebTVDiskCollationConverter
    {
        public DiskByteTransform byte_transform = DiskByteTransform.NOSWAP;

        public DiskByteTransform detect_build_byte_transform(Stream reader, uint build_offset = 0x880600)
        {
            try
            {
                byte[] build_test_data = new byte[4];

                reader.Seek(build_offset, SeekOrigin.Begin);
                reader.Read(build_test_data, 0, 4);

                // This tests to see if there's a WebTV build.
                // WebTV builds always start with a jump instruction (0x10).
                // We test using 3 different disk collations.
                if (build_test_data[0] == 0x10) // (US)
                {
                    return DiskByteTransform.NOSWAP;
                }
                else if (build_test_data[1] == 0x10)  // (build first byte detect 16-bit swap)
                {
                    return DiskByteTransform.BIT16SWAP;
                }
                else if (build_test_data[2] == 0x10)  // (build first byte detect build 16+32-bit swap)
                {
                    return DiskByteTransform.BIT1632SWAP;
                }
                else if (build_test_data[3] == 0x10)  // (build first byte detect build 32-bit swap)
                {
                    return DiskByteTransform.BIT32SWAP;
                }
                else
                {
                    return DiskByteTransform.NOSWAP;
                }
            }
            catch
            {
                return DiskByteTransform.NOSWAP;
            }
        }

        public DiskByteTransform detect_partition_table_byte_transform(WebTVDiskIO reader, ulong partition_table_offset = WebTVPartitionManager.PARTITION_TABLE_LC2_OFFSET)
        {
            try
            {
                byte[] partition_table_test_data = new byte[4];
                reader.AbsRead(partition_table_test_data, partition_table_offset + 0x8, 0, 4, false);

                uint partition_table_test_uint = BigEndianConverter.ToUInt32(partition_table_test_data, 0);

                // This tests the order of the bytes in the partition partition table magic.
                // The WebTV partition table magic is "timn"
                if (partition_table_test_uint == 0x74696D6E) // timn (no swapping)
                {
                    return DiskByteTransform.NOSWAP;
                }
                else if (partition_table_test_uint == 0x69746E6D)  // itnm (16-bit swap)
                {
                    return DiskByteTransform.BIT16SWAP;
                }
                else if (partition_table_test_uint == 0x6D6E7469)  // mnti (16+32-bit swap)
                {
                    return DiskByteTransform.BIT1632SWAP;
                }
                else if (partition_table_test_uint == 0x6E6D6974)  // nmit (32-bit swap)
                {
                    return DiskByteTransform.BIT32SWAP;
                }
                else
                {
                    return DiskByteTransform.NOSWAP;
                }
            }
            catch
            {
                return DiskByteTransform.NOSWAP;
            }
        }

        private void convert_byte_group(DiskByteTransform from_transform, ref byte[] from_data, ulong offset, DiskByteTransform to_transform)
        {
            byte byte1 = from_data[offset];
            byte byte2 = from_data[offset + 1];
            byte byte3 = from_data[offset + 2];
            byte byte4 = from_data[offset + 3];

            if ((from_transform == DiskByteTransform.NOSWAP && to_transform == DiskByteTransform.BIT16SWAP)
            ||  (from_transform == DiskByteTransform.BIT16SWAP && to_transform == DiskByteTransform.NOSWAP)
            ||  (from_transform == DiskByteTransform.BIT32SWAP && to_transform == DiskByteTransform.BIT1632SWAP)
            ||  (from_transform == DiskByteTransform.BIT1632SWAP && to_transform == DiskByteTransform.BIT32SWAP))
            {
                from_data[offset] = byte2;
                from_data[offset + 1] = byte1;
                from_data[offset + 2] = byte4;
                from_data[offset + 3] = byte3;
            }
            else if ((from_transform == DiskByteTransform.NOSWAP && to_transform == DiskByteTransform.BIT1632SWAP)
                ||   (from_transform == DiskByteTransform.BIT16SWAP && to_transform == DiskByteTransform.BIT32SWAP)
                ||   (from_transform == DiskByteTransform.BIT32SWAP && to_transform == DiskByteTransform.BIT16SWAP)
                ||   (from_transform == DiskByteTransform.BIT1632SWAP && to_transform == DiskByteTransform.NOSWAP))
            {
                from_data[offset] = byte3;
                from_data[offset + 1] = byte4;
                from_data[offset + 2] = byte1;
                from_data[offset + 3] = byte2;
            }
            else if ((from_transform == DiskByteTransform.NOSWAP && to_transform == DiskByteTransform.BIT32SWAP)
                ||   (from_transform == DiskByteTransform.BIT32SWAP && to_transform == DiskByteTransform.NOSWAP)
                ||   (from_transform == DiskByteTransform.BIT16SWAP && to_transform == DiskByteTransform.BIT1632SWAP)
                ||   (from_transform == DiskByteTransform.BIT1632SWAP && to_transform == DiskByteTransform.BIT16SWAP))
            {
                from_data[offset] = byte4;
                from_data[offset + 1] = byte3;
                from_data[offset + 2] = byte2;
                from_data[offset + 3] = byte1;
            }
        }

        public DiskByteTransform disklayout_to_bytetransform(DiskLayout layout)
        {
            if (layout == DiskLayout.UTV)
            {
                return DiskByteTransform.BIT1632SWAP;
            }
            else if (layout == DiskLayout.WEBSTAR)
            {
                return DiskByteTransform.BIT16SWAP;
            }
            else if (layout == DiskLayout.LC2)
            {
                return DiskByteTransform.BIT16SWAP;
            }
            else
            {
                return DiskByteTransform.NOSWAP;
            }
        }

        public void convert_bytes(DiskLayout from_layout, ref byte[] from_data, ulong offset, uint size, DiskLayout to_layout = DiskLayout.PLAIN)
        {
            this.convert_bytes(disklayout_to_bytetransform(from_layout), ref from_data, offset, size, disklayout_to_bytetransform(to_layout));
        }

        public void convert_bytes(ref byte[] from_data, ulong offset, uint size, DiskByteTransform to_transform = DiskByteTransform.NOSWAP)
        {
            this.convert_bytes(this.byte_transform, ref from_data, offset, size, to_transform);
        }

        public void convert_bytes(DiskByteTransform from_transform, ref byte[] from_data, ulong offset, uint size, DiskByteTransform to_transform = DiskByteTransform.NOSWAP)
        {
            if ((size % 4) > 0)
            {
                throw new DataMisalignedException("Convert buffer length must be divisible by 4.");
            }

            for (ulong i = offset; i < size; i += 4)
            {
                this.convert_byte_group(from_transform, ref from_data, i, to_transform);
            }
        }
    }
}
