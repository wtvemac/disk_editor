using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    public class LZSS
    {
        public int r = 0;
        public int ring_buffer_size = 0x1000;
        public int root_index = 0x1000;
        public int max_match_length = 0x11;
        public int match_threshold = 0x03;
        public int match_position = 0x00;
        public int match_length = 0x00;
        public byte[] ring_buffer;
        public int[] parent;
        public int[] lchild;
        public int[] rchild;

        public LZSS()
        {
            this.clear();
        }

        public void clear()
        {
            this.match_position = 0x00;
            this.match_length = 0x00;
            this.ring_buffer = new byte[this.ring_buffer_size + this.max_match_length];
            this.parent = Enumerable.Repeat(0x00000000, this.ring_buffer_size + 0x01).ToArray();
            this.rchild = Enumerable.Repeat(0x00000000, (this.ring_buffer_size * 2) + 0x01).ToArray();
            this.lchild = Enumerable.Repeat(0x00000000, (this.ring_buffer_size * 2) + 0x01).ToArray();

            for (var ii = 0; ii < (this.ring_buffer_size - this.max_match_length); ii++)
            {
                this.ring_buffer[ii] = 0x20;
            }

            for (var ii = (this.ring_buffer_size + 1); ii < this.rchild.Length; ii++)
            {
                this.rchild[ii] = this.root_index;
            }

            for (var ii = 0; ii < (this.ring_buffer_size - 1); ii++)
            {
                this.parent[ii] = this.root_index;
            }
        }

        private void InsertNode(int i)
        {
            int keyi = this.ring_buffer[i];
            int keyii = (int)this.ring_buffer[i + 1] ^ (int)this.ring_buffer[i + 2];
            keyii = ((keyii ^ (keyii >> 4)) & 0x0F) << 8;

            int parent_index = i;
            int parent_link = (this.ring_buffer_size + 1) + keyi + keyii;
            int child_index = parent_link;
            int child_link = i;

            this.rchild[i] = this.root_index;
            this.lchild[i] = this.root_index;
            this.match_length = 0x00;
            ref var matched_list = ref this.rchild;
            int cmp_index = 1;
            var looped = 0;
            while (true)
            {
                looped++;

                if (looped >= 0xFFFF)
                {
                    throw new Exception("Runaway loop!");
                }

                if (cmp_index >= 0)
                {
                    cmp_index = this.rchild[parent_link];
                    matched_list = ref this.rchild;
                }
                else
                {
                    cmp_index = this.lchild[parent_link];
                    matched_list = ref this.lchild;
                }

                if (cmp_index == this.root_index)
                {
                    parent_index = i;
                    child_index = parent_link;
                    child_link = i;
                    break;
                }

                parent_link = cmp_index;
                var ii = 1;
                while (ii < this.max_match_length)
                {
                    if(this.ring_buffer[i + ii] != this.ring_buffer[parent_link + ii])
                    {
                        break;
                    }

                    ii++;
                }

                if (ii > this.match_length)
                {
                    this.match_length = ii;
                    this.match_position = parent_link;

                    if (ii > (this.max_match_length - 1))
                    {
                        this.parent[i] = this.parent[parent_link];

                        this.rchild[i] = this.rchild[parent_link];
                        this.lchild[i] = this.lchild[parent_link];

                        this.parent[this.rchild[i]] = i;
                        this.parent[this.lchild[i]] = i;

                        if (this.rchild[this.parent[parent_link]] != parent_link)
                        {
                            matched_list = ref this.lchild;
                        }
                        else
                        {
                            matched_list = ref this.rchild;
                        }

                        child_index = this.parent[parent_link];
                        child_link = i;
                        parent_index = parent_link;
                        parent_link = this.root_index;
                        break;
                    }
                }
            }

            this.parent[parent_index] = parent_link;

            matched_list[child_index] = child_link;
        }

        private void DeleteNode(int i)
        {
            if (this.parent[i] != this.root_index)
            {
                var ii = 0;

                if (this.rchild[i] == this.root_index)
                {
                    ii = this.lchild[i];
                }
                else if (this.lchild[i] == this.root_index)
                {
                    ii = this.rchild[i];
                }
                else
                {
                    ii = this.lchild[i];

                    if (ii != this.root_index)
                    {
                        var looped = 0;
                        while (ii != this.root_index)
                        {
                            looped++;

                            if (looped >= 0xFFFF)
                            {
                                throw new Exception("Runaway loop!");
                            }

                            ii = this.rchild[ii];
                        }

                        this.rchild[this.parent[ii]] = this.lchild[ii];
                        this.parent[this.lchild[ii]] = this.parent[ii];

                        this.lchild[ii] = this.lchild[i];
                        this.parent[this.lchild[i]] = ii;
                    }

                    this.rchild[ii] = this.rchild[i];
                    this.parent[this.rchild[i]] = ii;
                }

                this.parent[ii] = this.parent[i];

                var parent_link = this.parent[i];
                if (this.rchild[parent_link] != i)
                {
                    this.lchild[parent_link] = ii;
                }
                else
                {
                    this.rchild[parent_link] = ii;
                }

                this.parent[i] = this.root_index;
            }
        }

        public byte[] Compress(byte[] uncompressed_data)
        {
            var uncompressed_size = uncompressed_data.Length;
            var i = 0;
            var ring_index = 0;
            var ring_footer_start = this.ring_buffer_size - this.max_match_length - 1;
            var footer_index = ring_footer_start;
            var length = 0;

            for (; length <= this.max_match_length && i < uncompressed_size; length++)
            {
                this.ring_buffer[ring_footer_start + length] = uncompressed_data[i++];
            }

            byte mask = 1;
            var code_buffer = new byte[0x14];
            var code_buffer_index = 1;

            var compressed_data = new List<byte>();

            this.InsertNode(ring_footer_start);
            while (length > 0)
            {
                if (this.match_length > length)
                {
                    this.match_length = length;
                }

                if (this.match_length >= this.match_threshold)
                {
                    var _match_position = footer_index - this.match_position - 1;
                    if (_match_position < 0)
                    {
                        _match_position += this.ring_buffer_size;
                    }

                    code_buffer[code_buffer_index++] = (byte)_match_position;
                    code_buffer[code_buffer_index++] = (byte)(((_match_position >> 4) & 0xF0) | (byte)(this.match_length - this.match_threshold));
                }
                else
                {
                    this.match_length = 1;
                    code_buffer[0] |= mask;
                    code_buffer[code_buffer_index] = this.ring_buffer[footer_index];
                    code_buffer_index++;
                }

                mask <<= 1;

                if (mask == 0x00)
                {
                    for (var ii = 0; ii < code_buffer_index; ii++)
                    {
                        compressed_data.Add(code_buffer[ii]);
                    }

                    code_buffer[0] = 0;
                    mask = 1;
                    code_buffer_index = 1;
                }

                var last_match_length = this.match_length;
                if (last_match_length > 0)
                {
                    for (var ii = 0; ii < last_match_length; ii++, i++)
                    {
                        this.DeleteNode(ring_index);

                        if (i < uncompressed_size)
                        {
                            this.ring_buffer[ring_index] = uncompressed_data[i];

                            if (ring_index <= (this.max_match_length - 1))
                            {
                                this.ring_buffer[this.ring_buffer_size + ring_index] = uncompressed_data[i];
                            }
                        }
                        else
                        {
                            i = (uncompressed_size - 1);
                            length--;
                        }

                        ring_index = (ring_index + 1) & (this.ring_buffer_size - 1);
                        footer_index = (footer_index + 1) & (this.ring_buffer_size - 1);

                        if (length != 0)
                        {
                            this.InsertNode(footer_index);
                        }
                    }
                }
            }


            if (code_buffer_index > 1)
            {
                for (var ii = 0; ii < code_buffer_index; ii++)
                {
                    compressed_data.Add(code_buffer[ii]);
                }
            }

            return compressed_data.ToArray();
        }

        public byte[] Expand(byte[] compressed_data, int uncompressed_size = 0, uint flags_start = 0x0000)
        {
            var compressed_size = compressed_data.Length;

            var uncompressed_data = new List<byte>();

            uint flags = flags_start;
            var i = 0;
            while (i < compressed_size)
            {
                if ((flags & 0x100) == 0)
                {
                    flags = (uint)compressed_data[i] | 0xFF00;
                    i++;
                }

                uint current_byte = compressed_data[i];
                if ((flags & 0x01) == 0x01)
                {
                    uncompressed_data.Add((byte)current_byte);
                }
                else
                {
                    uint next_byte = compressed_data[++i];

                    var m = ((next_byte & 0xF0) << 4) | current_byte;

                    for (var ii = 0; ii < ((next_byte & 0x0F) + this.match_threshold); ii++)
                    {
                        uncompressed_data.Add(uncompressed_data[(int)(uncompressed_data.Count - (m + 1))]);
                    }
                }

                flags >>= 1;
                i++;

                if(uncompressed_size > 0 && uncompressed_data.Count >= uncompressed_size)
                {
                    break;
                }
            }

            return uncompressed_data.ToArray();
        }
    }
}
