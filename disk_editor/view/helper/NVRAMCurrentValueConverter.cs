using System;
using System.Windows;
using System.Windows.Data;
using System.Text;

namespace disk_editor
{
    class NVRAMCurrentValueConverter : IValueConverter
    {
        public static byte[] Bytes(NVSetting nvram_setting, string converted_value)
        {
            byte[] edited_value = null;

            if (nvram_setting != null)
            {
                if (nvram_setting.data_type == NVDataType.NULL_TERMINATED_STRING)
                {
                    edited_value = new byte[converted_value.Length + 1];

                    Encoding.Default.GetBytes(converted_value).CopyTo(edited_value, 0);
                }
                else if (nvram_setting.data_type == NVDataType.UINT16)
                {
                    var value = System.Convert.ToUInt16(converted_value);

                    edited_value = BigEndianConverter.GetBytes(value);
                }
                else if (nvram_setting.data_type == NVDataType.INT16)
                {
                    var value = System.Convert.ToInt16(converted_value);

                    edited_value = BigEndianConverter.GetBytes(value);
                }
                else if (nvram_setting.data_type == NVDataType.UINT32)
                {
                    var value = System.Convert.ToUInt32(converted_value);

                    edited_value = BigEndianConverter.GetBytes(value);
                }
                else if (nvram_setting.data_type == NVDataType.INT32
                     || nvram_setting.data_type == NVDataType.YUV_COLOR)
                {
                    var value = System.Convert.ToInt32(converted_value);

                    edited_value = BigEndianConverter.GetBytes(value);
                }
                else if (nvram_setting.data_type == NVDataType.UINT64)
                {
                    var value = System.Convert.ToUInt64(converted_value);

                    edited_value = BigEndianConverter.GetBytes(value);
                }
                else if (nvram_setting.data_type == NVDataType.INT64)
                {
                    var value = System.Convert.ToInt64(converted_value);

                    edited_value = BigEndianConverter.GetBytes(value);
                }
                else if (nvram_setting.data_type == NVDataType.BOOLEAN)
                {
                    edited_value = new byte[1];

                    if(converted_value.Trim().ToLower() == "true")
                    {
                        edited_value[0] = 0x01;
                    }
                    else
                    {
                        edited_value[0] = 0x00;
                    }

                }
                else if (nvram_setting.data_type == NVDataType.CHAR)
                {
                    edited_value = Encoding.Default.GetBytes(converted_value);
                }
                else if (nvram_setting.data_type == NVDataType.BINARY_BLOB)
                {
                    edited_value = new byte[converted_value.Length / 2];

                    for (var i = 0; i < converted_value.Length; i += 2)
                    {
                        edited_value[i / 2] = System.Convert.ToByte(converted_value.Substring(i, 2), 16);
                    }
                }
            }

            return edited_value;
        }

        public static string String(NVSetting nvram_setting)
        {
            string converted_value = "";

            // edited_value is the more current value.
            if (nvram_setting != null && nvram_setting.value.edited_value != null)
            {
                if (nvram_setting.data_type == NVDataType.NULL_TERMINATED_STRING)
                {
                    converted_value = new string(Encoding.Default.GetChars(nvram_setting.value.edited_value));
                    if (converted_value.IndexOf("\0") > 0)
                    {
                        converted_value = converted_value.Remove(converted_value.IndexOf("\0"));
                    }
                }
                else if (nvram_setting.data_type == NVDataType.UINT16)
                {
                    converted_value = BigEndianConverter.ToUInt16(nvram_setting.value.edited_value, 0).ToString();
                }
                else if (nvram_setting.data_type == NVDataType.INT16)
                {
                    converted_value = BigEndianConverter.ToInt16(nvram_setting.value.edited_value, 0).ToString();
                }
                else if (nvram_setting.data_type == NVDataType.UINT32)
                {
                    converted_value = BigEndianConverter.ToUInt32(nvram_setting.value.edited_value, 0).ToString();
                }
                else if (nvram_setting.data_type == NVDataType.INT32)
                {
                    converted_value = BigEndianConverter.ToInt32(nvram_setting.value.edited_value, 0).ToString();
                }
                else if (nvram_setting.data_type == NVDataType.UINT64)
                {
                    converted_value = BigEndianConverter.ToUInt64(nvram_setting.value.edited_value, 0).ToString();
                }
                else if (nvram_setting.data_type == NVDataType.INT64)
                {
                    converted_value = BigEndianConverter.ToInt64(nvram_setting.value.edited_value, 0).ToString();
                }
                else if (nvram_setting.data_type == NVDataType.BOOLEAN)
                {
                    if (nvram_setting.value.edited_value[0] == 0x01)
                    {
                        converted_value = "True";
                    }
                    else
                    {
                        converted_value = "False";
                    }
                }
                else if (nvram_setting.data_type == NVDataType.CHAR)
                {
                    converted_value = new string(Encoding.Default.GetChars(nvram_setting.value.edited_value));
                }
                else if (nvram_setting.data_type == NVDataType.BINARY_BLOB)
                {
                    converted_value = BitConverter.ToString(nvram_setting.value.edited_value).Replace("-", string.Empty);
                }
                else if (nvram_setting.data_type == NVDataType.YUV_COLOR)
                {
                    var rgb = BitConverter.ToString(nvram_setting.value.edited_value).Replace("-", string.Empty);
                    var yuv = BitConverter.ToString(NVRAM.RGBtoYUV(nvram_setting.value.edited_value)).Replace("-", string.Empty);

                    converted_value = "YUV≈0x" + yuv + ", RGB≈0x" + rgb;
                }
            }

                return converted_value;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var nvram_setting = value as NVSetting;

            return NVRAMCurrentValueConverter.String(nvram_setting);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
