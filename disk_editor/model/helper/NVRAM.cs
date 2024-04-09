using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;

namespace disk_editor
{
    class NVSettings
    {
        public uint checksum { get; set; }
        public byte[] unknown1 { get; set; }
        public ObservableCollection<NVSetting> nv_setting { get; set; }
    }

    class NVRAM : WebTVIO
    {
        public const short WEBTV_Y_BLACK = 0x10;
        public const short WEBTV_Y_WHITE = 0xEB;
        public const short WEBTV_Y_RANGE = WEBTV_Y_WHITE - WEBTV_Y_BLACK;
        public const short WEBTV_UV_OFFSET = 0x80;
        public const int HD_NVRAM_ALIGNMENT = 0x04;
        public const long MAX_NVRAM_SIZE = 0x4000;
        public bool is_viewer_nvram = false;

        public uint calculate_checksum(byte[] nvram_data, bool big_endian = true)
        {
            uint checksum = 0;
            for (int i = 0x04; i < nvram_data.Length; i += 4)
            {
                uint checksum_block = 0;
                if (big_endian)
                {
                    checksum_block = BigEndianConverter.ToUInt32(nvram_data, i);
                }
                else
                {
                    checksum_block = BitConverter.ToUInt32(nvram_data, i);
                }

                checksum += checksum_block;
            }

            return checksum;
        }

        public uint calculate_checksum(bool big_endian = true)
        {
            var nvram_length = Math.Min(this.io.Length, MAX_NVRAM_SIZE);

            var nvram_data = new byte[nvram_length];

            this.Read(nvram_data, 0, 0, (int)nvram_length);

            return this.calculate_checksum(nvram_data, big_endian);
        }

        private void populate_nvram_settings(ObservableCollection<NVSetting> nv_settings, ref byte[] nvram_data, ref uint data_position, ref long nvram_length, bool priority = false, bool big_endian = true)
        {
            for (var i = 0; i < nv_settings.Count && data_position < nvram_length; i++)
            {
                var nv_setting = nv_settings[i];

                if (priority != nv_setting.priority || nv_setting.removed)
                {
                    continue;
                }

                var setting_name = Encoding.Default.GetBytes(nv_setting.name);

                uint setting_length = 0;
                byte[] setting_value = null;
                if (nv_setting.value.edited_value.Count() == 0)
                {
                    setting_value = new byte[1];
                    setting_length = 1;
                }
                else
                {
                    if (nv_setting.data_type == NVDataType.YUV_COLOR)
                    {
                        setting_value = NVRAM.RGBtoYUV(nv_setting.value.edited_value);
                    }
                    else
                    {
                        setting_value = nv_setting.value.edited_value;
                    }
                    setting_length = (uint)setting_value.Count();
                }

                byte[] _setting_length = null;
                if (big_endian)
                {
                    _setting_length = BigEndianConverter.GetBytes(setting_length);
                }
                else
                {
                    _setting_length = BitConverter.GetBytes(setting_length);
                    setting_name = BigEndianConverter.ReorderBytes(setting_name, 0, 4);

                    if (nv_setting.data_type == NVDataType.UINT16
                    || nv_setting.data_type == NVDataType.INT16
                    || nv_setting.data_type == NVDataType.UINT32
                    || nv_setting.data_type == NVDataType.INT32
                    || nv_setting.data_type == NVDataType.UINT64
                    || nv_setting.data_type == NVDataType.INT64
                    || nv_setting.data_type == NVDataType.YUV_COLOR)
                    {
                        setting_value = BigEndianConverter.ReorderBytes(setting_value, 0, setting_value.Length);
                    }
                }

                _setting_length.CopyTo(nvram_data, data_position);
                data_position += 0x04;
                setting_name.CopyTo(nvram_data, data_position);
                data_position += 0x04;
                setting_value.CopyTo(nvram_data, data_position);
                data_position += setting_length;

                uint alignmet_skew = setting_length % HD_NVRAM_ALIGNMENT;
                if (alignmet_skew > 0)
                {
                    data_position += (HD_NVRAM_ALIGNMENT - alignmet_skew);
                }
            }
        }

        public byte[] build_nvram(NVSettings nvram_settings, bool big_endian = true)
        {
            var nvram_length = this.io.Length;

            var nvram_data = new byte[nvram_length];

            uint data_position = 0x00;
            data_position += 0x10; // past checksum (+0x04) and +0x0C section

            this.populate_nvram_settings(nvram_settings.nv_setting, ref nvram_data, ref data_position, ref nvram_length, true, big_endian);
            this.populate_nvram_settings(nvram_settings.nv_setting, ref nvram_data, ref data_position, ref nvram_length, false, big_endian);

            var checksum = calculate_checksum(nvram_data, big_endian);

            if (big_endian)
            {
                var _checksum = BigEndianConverter.GetBytes(checksum);
                _checksum.CopyTo(nvram_data, 0x00);
            }
            else
            {
                var _checksum = BitConverter.GetBytes(checksum);
                _checksum.CopyTo(nvram_data, 0x00);
            }

            return nvram_data;
        }

        public void write_nvram_settings(NVSettings nvram_settings)
        {
            var nvram_data = this.build_nvram(nvram_settings);

            this.Write(nvram_data, 0, 0, nvram_data.Length);
        }

        public NVSettings enumerate_nvram_settings(bool big_endian = true)
        {
            var nvram_settings = new NVSettings();

            try
            {
                this.io.Position = 0;

                nvram_settings.checksum = this.ReadUint32(big_endian);
                nvram_settings.unknown1 = this.ReadBytes(12);
                nvram_settings.nv_setting = new ObservableCollection<NVSetting>();

                while (true)
                {
                    var setting_length = this.ReadUint32(big_endian);

                    if (setting_length > (this.io.Length - this.io.Position) || setting_length > MAX_NVRAM_SIZE)
                    {
                        throw new EndOfStreamException("Setting size too large to read. Bogus data? Bad endineness? [offset=0x" + ((this.io.Position - 0x04).ToString("X")) + "]");
                    }
                    else if (setting_length == 0)
                    {
                        break;
                    }

                    var setting_name = "";
                    if (!big_endian)
                    {
                        setting_name = new string(Encoding.Default.GetChars(BigEndianConverter.ReorderBytes(this.ReadBytes(4), 0, 4)));
                    }
                    else
                    {
                        setting_name = new string(this.ReadChars(4));
                    }
                    if(setting_name.IndexOf("\0") > 0)
                    {
                        setting_name = setting_name.Remove(setting_name.IndexOf("\0"));
                    }


                    nvram_settings.nv_setting.Add(this.create_nvsetting(setting_name, this.ReadBytes(setting_length), big_endian));

                    uint alignmet_skew = setting_length % HD_NVRAM_ALIGNMENT;
                    if (alignmet_skew > 0)
                    {
                        this.Move(HD_NVRAM_ALIGNMENT - alignmet_skew);
                    }

                    if (this.io.Position > MAX_NVRAM_SIZE || nvram_settings.nv_setting.Count > MAX_NVRAM_SIZE)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidDataException("Error reading NVRAM. " + e.Message);
            }

            return nvram_settings;
        }

        public static byte[] YUVtoRGB(byte[] yuv_color)
        {
            if (yuv_color.Length >= 3)
            {
                var start_index = yuv_color.Length - 3;
                var y = (short)yuv_color[start_index + 0];
                var u = (short)yuv_color[start_index + 1];
                var v = (short)yuv_color[start_index + 2];

                y -= WEBTV_Y_BLACK;
                y = (short)(((y << 8) + 0x80) / WEBTV_Y_RANGE);

                short _u = (short)(((455 * (u - WEBTV_UV_OFFSET)) + 0x80) >> 8);
                short _v = (short)(((358 * (v - WEBTV_UV_OFFSET)) + 0x80) >> 8);
                short uvg = (short)((((50 * _u) + (131 * _v)) + 0x80) >> 8);

                short r = (short)(y + _v);
                if (r < 0)
                {
                    r = 0;
                }
                else if (r > 0xFF)
                {
                    r = 0xFF;
                }

                short g = (short)(y - uvg);
                if (g < 0)
                {
                    g = 0;
                }
                else if (g > 0xFF)
                {
                    g = 0xFF;
                }

                short b = (short)(y + _u);
                if (b < 0)
                {
                    b = 0;
                }
                else if (b > 0xFF)
                {
                    b = 0xFF;
                }

                return new byte[4]
                {
                    (byte)0,
                    (byte)r,
                    (byte)g,
                    (byte)b
                };
            }
            else
            {
                return yuv_color;
            }
        }
        public static byte[] RGBtoYUV(byte[] rgb_color)
        {
            if (rgb_color.Length >= 3)
            {
                var start_index = rgb_color.Length - 3;
                var r = (short)rgb_color[start_index + 0];
                var g = (short)rgb_color[start_index + 1];
                var b = (short)rgb_color[start_index + 2];

                short _y = (short)(((77 * r) + (150 * g) + (29 * b) + 0x80) >> 8);

                short _u = (short)(WEBTV_UV_OFFSET + (((144 * (b - _y)) + 0x80) >> 8));
                if(_u < WEBTV_Y_BLACK)
                {
                    _u = WEBTV_Y_BLACK;
                }
                else if (_u > WEBTV_Y_WHITE)
                {
                    _u = WEBTV_Y_WHITE;
                }

                short _v = (short)(WEBTV_UV_OFFSET + (((183 * (r - _y)) + 0x80) >> 8));
                if (_v < WEBTV_Y_BLACK)
                {
                    _v = WEBTV_Y_BLACK;
                }
                else if (_v > WEBTV_Y_WHITE)
                {
                    _v = WEBTV_Y_WHITE;
                }

                _y = (short)(WEBTV_Y_BLACK + (((_y * WEBTV_Y_RANGE) + 0x80) >> 8));
                if (_y < WEBTV_Y_BLACK)
                {
                    _y = WEBTV_Y_BLACK;
                }
                else if (_y > WEBTV_Y_WHITE)
                {
                    _y = WEBTV_Y_WHITE;
                }

                return new byte[4]
                {
                    (byte)0,
                    (byte)_y,
                    (byte)_u,
                    (byte)_v
                };
            }
            else
            {
                return rgb_color;
            }
        }

        private NVSetting create_nvsetting(string name, byte[] value, bool big_endian = true)
        {
            var nvram_setting = new NVSetting()
            {
                id = Guid.NewGuid(),
                name = name,
                title = "",
                notes = "",
                data_type = NVDataType.BINARY_BLOB,
                data_editor = NVDataEditor.HEX_EDITOR,
                edited = false,
                removed = false
            };

            NVSetting default_value = null;
            if (NVDefaults.defaults.TryGetValue(name, out default_value)
            || NVDefaults.defaults.TryGetValue(name.ToUpper(), out default_value)
            || NVDefaults.defaults.TryGetValue(name.ToLower(), out default_value))
            {
                nvram_setting.title = default_value.title;
                nvram_setting.notes = default_value.notes;
                nvram_setting.data_type = default_value.data_type;
                nvram_setting.data_editor = default_value.data_editor;

                if (!big_endian)
                {
                    if (default_value.data_type == NVDataType.UINT16
                    || default_value.data_type == NVDataType.INT16
                    || default_value.data_type == NVDataType.UINT32
                    || default_value.data_type == NVDataType.INT32
                    || default_value.data_type == NVDataType.UINT64
                    || default_value.data_type == NVDataType.INT64
                    || default_value.data_type == NVDataType.YUV_COLOR)
                    {
                        value = BigEndianConverter.ReorderBytes(value, 0, value.Length);
                    }
                }

                if(default_value.data_type == NVDataType.YUV_COLOR)
                {
                    value = NVRAM.YUVtoRGB(value);
                }

                nvram_setting.value = new NVSettingValue()
                {
                    stored_value = value,
                    edited_value = value,
                    default_value = default_value.value.default_value
                };
            }
            else
            { 
                nvram_setting.value = new NVSettingValue()
                {
                    stored_value = value,
                    edited_value = value,
                    default_value = null
                };
            }

            return nvram_setting;
        }

        public NVRAM(WebTVDiskIO io, ObjectLocation nvram_location)
            : base(io, get_object_bounds(nvram_location))
        {
        }

        public NVRAM(string file_name) 
            : base(file_name)
        {
            this.byte_converter = null;
        }

        public NVRAM(Stream reader)
            : base(reader)
        {
        }

    }
}
