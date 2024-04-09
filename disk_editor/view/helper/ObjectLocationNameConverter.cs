using System;
using System.Windows;
using System.Windows.Data;

namespace disk_editor
{
    class ObjectLocationNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var object_location = value as ObjectLocation;

            if (object_location != null)
            {
                var object_location_name = "";

                switch (object_location.type)
                {
                    case ObjectLocationType.FILE_LOCATION:
                        object_location_name += "Build File";
                        break;
                    case ObjectLocationType.BROWSER0_LOCATION:
                        object_location_name += "Browser 0";
                        break;
                    case ObjectLocationType.BROWSER1_LOCATION:
                        object_location_name += "Browser 1";
                        break;
                    case ObjectLocationType.DIAG_LOCATION:
                        object_location_name += "Diag";
                        break;
                    case ObjectLocationType.TIER3_DIAG_LOCATION:
                        object_location_name += "Tier3 Diag";
                        break;
                    case ObjectLocationType.PRIMARY_LOCATION:
                        object_location_name += "Primary";
                        break;
                    case ObjectLocationType.EXPLODED_PRIMARY_LOCATION:
                        object_location_name += "Exploded Primary";
                        break;

                    case ObjectLocationType.NVRAM_PRIMARY0:
                        object_location_name += "Primary NVRAM 0";
                        break;
                    case ObjectLocationType.NVRAM_PRIMARY1:
                        object_location_name += "Primary NVRAM 1";
                        break;
                    case ObjectLocationType.NVRAM_SECONDARY0:
                        object_location_name += "Secondary NVRAM 0";
                        break;
                    case ObjectLocationType.NVRAM_SECONDARY1:
                        object_location_name += "Secondary NVRAM 1";
                        break;

                    case ObjectLocationType.UNKNOWN0:
                        object_location_name += "Unknown 0";
                        break;
                    case ObjectLocationType.UNKNOWN1:
                        object_location_name += "Unknown 1";
                        break;
                    case ObjectLocationType.UNKNOWN2:
                        object_location_name += "Unknown 2";
                        break;

                    default:
                        object_location_name += "Unknown";
                        break;
                }

                if (object_location.type != ObjectLocationType.FILE_LOCATION)
                {
                    if(object_location.type != ObjectLocationType.PRIMARY_LOCATION
                    && object_location.type != ObjectLocationType.EXPLODED_PRIMARY_LOCATION)
                    {
                        object_location_name += ", offset=" + object_location.offset.ToString("X");
                    }

                    if (object_location.type != ObjectLocationType.NVRAM_PRIMARY0
                    && object_location.type != ObjectLocationType.NVRAM_PRIMARY1
                    && object_location.type != ObjectLocationType.NVRAM_SECONDARY0
                    && object_location.type != ObjectLocationType.NVRAM_SECONDARY1)
                    {
                        object_location_name += ", size=" + BytesToString.bytes_to_iec(object_location.size_bytes);
                    }
                }

                return object_location_name;
            }

            return "Bad";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
