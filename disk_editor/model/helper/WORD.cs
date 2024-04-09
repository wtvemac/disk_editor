using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    class WORD
    {
        public enum WordSizes
        {
            WORD = 2,
            DWORD = 4,
            QWORD = 8
        }

        public const int WORD_SIZE_BYTES = 0x2;
        public const int DWORD_SIZE_BYTES = WORD_SIZE_BYTES * 2;

        public static int size_bound_to_xwords(int x, WordSizes word_byte_size = WORD.WordSizes.WORD)
        {
            int size = x;

            if (x > 0)
            {
                size = (int)Math.Ceiling((double)x / (double)word_byte_size) * (int)word_byte_size;
            }
            else
            {
                size = (int)word_byte_size;
            }

            return size;
        }
    }
}
