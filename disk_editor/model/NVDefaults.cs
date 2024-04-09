using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    static class NVDefaults
    {
        /*
         */
        public static Dictionary<string, NVSetting> defaults = new Dictionary<string, NVSetting>()
        {
            {
                "tlly", new NVSetting()
                        {
                            name = "tlly",
                            title = "TellyScript",
                            notes = "ReadTellyScriptFromNV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.TELLYSCRIPT_EDITOR,
                            priority = true,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DLSC", new NVSetting()
                        {
                            name = "DLSC",
                            title = "DialScript",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.TELLYSCRIPT_EDITOR,
                            priority = true,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "hn", new NVSetting()
                        {
                            name = "hn",
                            title = "Head-waiter Host",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "hp", new NVSetting()
                        {
                            name = "hp",
                            title = "Head-waiter Port",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.UINT16,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "hf", new NVSetting()
                        {
                            name = "hf",
                            title = "Head-waiter Flags",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.UINT32,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "hl", new NVSetting()
                        {
                            name = "hl",
                            title = "Head-waiter Limit",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.CHAR,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "*n", new NVSetting()
                        {
                            name = "hf",
                            title = "Star Host",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "*p", new NVSetting()
                        {
                            name = "*p",
                            title = "Star Port",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.UINT16,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "*f", new NVSetting()
                        {
                            name = "*f",
                            title = "Star Flags",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.UINT32,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "*l", new NVSetting()
                        {
                            name = "*l",
                            title = "Star Limit",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.CHAR,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },


            {
                "FLip", new NVSetting()
                        {
                            name = "FLip",
                            title = "Flash IP",
                            notes = "UpdateFlash",
                            data_type = NVDataType.UINT32,
                            data_editor = NVDataEditor.IP_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "FLpo", new NVSetting()
                        {
                            name = "FLpo",
                            title = "Flash Port",
                            notes = "UpdateFlash",
                            data_type = NVDataType.UINT16,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "FLth", new NVSetting()
                        {
                            name = "FLth",
                            title = "Flash Path",
                            notes = "UpdateFlash",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },



            {
                "boot", new NVSetting()
                        {
                            name = "boot",
                            title = "Boot URL",
                            notes = "Network::RestoreBootURL/Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "wtch", new NVSetting()
                        {
                            name = "wtch",
                            title = "Message Watch URL",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },

            {
                "TOUR", new NVSetting()
                        {
                            name = "TOUR",
                            title = "Tourist Enabled",
                            notes = "RestoreTouristState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DLLI", new NVSetting()
                        {
                            name = "DLLI",
                            title = "Download Login URL",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DLLS", new NVSetting()
                        {
                            name = "DLLS",
                            title = "Download List URL",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "Dial", new NVSetting()
                        {
                            name = "Dial",
                            title = "Working Dial Number",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "FONE", new NVSetting()
                        {
                            name = "FONE",
                            title = "Phone Settings",
                            notes = "InitPhoneSettings/InstallPhoneSettings/Network::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "ANI ", new NVSetting()
                        {
                            name = "ANI ",
                            title = "Telephone number ID (ANI)",
                            notes = "Network::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "SEID", new NVSetting()
                        {
                            name = "SEID",
                            title = "Metering Session ID",
                            notes = "Network::RestoreMeteringDataFromNVRam",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "MCTM", new NVSetting()
                        {
                            name = "MCTM",
                            title = "Metering Charged Time",
                            notes = "Network::RestoreMeteringDataFromNVRam",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "MFTM", new NVSetting()
                        {
                            name = "MFTM",
                            title = "Metering Free Time",
                            notes = "Network::RestoreMeteringDataFromNVRam",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "clog", new NVSetting()
                        {
                            name = "clog",
                            title = "Phone Call Log",
                            notes = "InitializePhoneLog",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TZNA", new NVSetting()
                        {
                            name = "TZNA",
                            title = "Time Zone Name",
                            notes = "Clock::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TZOF", new NVSetting()
                        {
                            name = "TZOF",
                            title = "Time Zone Offset",
                            notes = "Clock::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TZDR", new NVSetting()
                        {
                            name = "TZDR",
                            title = "Time Zone DST Rule",
                            notes = "",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "D3EK", new NVSetting()
                        {
                            name = "D3EK",
                            title = "Cookie Encryption Key",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DLOF", new NVSetting()
                        {
                            name = "DLOF",
                            title = "Data Download Check Time Offset",
                            notes = "",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "MWOF", new NVSetting()
                        {
                            name = "MWOF",
                            title = "Message Watch Check Time Offset",
                            notes = "AlarmClock::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DLOC", new NVSetting()
                        {
                            name = "DLOC",
                            title = "Data Download Enabled",
                            notes = "",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVSS", new NVSetting()
                        {
                            name = "TVSS",
                            title = "TV Signal Source",
                            notes = "System::RestoreStatePhase2",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVAL", new NVSetting()
                        {
                            name = "TVAL",
                            title = "TV Listings Auto-download (Old)",
                            notes = "",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "UPRQ", new NVSetting()
                        {
                            name = "UPRQ",
                            title = "Upgrade Path",
                            notes = "RestoreUpgradePath",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVDL", new NVSetting()
                        {
                            name = "TVDL",
                            title = "TV Listings Auto-download",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVZP", new NVSetting()
                        {
                            name = "TVZP",
                            title = "TV Zip Code",
                            notes = "System::RestoreStatePhase1",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVHM", new NVSetting()
                        {
                            name = "TVHM",
                            title = "TV Home State",
                            notes = "TVHome::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "INTR", new NVSetting()
                        {
                            name = "INTR",
                            title = "TV Home Intro Version",
                            notes = "TVHome::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVCH", new NVSetting()
                        {
                            name = "TVCH",
                            title = "Current TV Channel",
                            notes = "TVController::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVLH", new NVSetting()
                        {
                            name = "TVLH",
                            title = "TV List Headend",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVEC", new NVSetting()
                        {
                            name = "TVEC",
                            title = "TV Channel Record Flags",
                            notes = "TVController::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVBI", new NVSetting()
                        {
                            name = "TVBI",
                            title = "TV IR Blaster Info",
                            notes = "TVController::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAPW", new NVSetting()
                        {
                            name = "CAPW",
                            title = "TV Access Control/CAM Password",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAAL", new NVSetting()
                        {
                            name = "CAAL",
                            title = "TV Access Control/CAM 'Not Rated' Blocked",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAPL", new NVSetting()
                        {
                            name = "CAPL",
                            title = "TV Access Control/CAM Panel Locked",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAVL", new NVSetting()
                        {
                            name = "CAVL",
                            title = "TV Access Control/CAM PPV Locked",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CASL", new NVSetting()
                        {
                            name = "CASL",
                            title = "TV Access Control/CAM PPV Spending Limit",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "GALK", new NVSetting()
                        {
                            name = "GALK",
                            title = "Game Access Locked",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "GARA", new NVSetting()
                        {
                            name = "GARA",
                            title = "Game Access ESRB Rating Limit",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAER", new NVSetting()
                        {
                            name = "CAER",
                            title = "TV Access/CAM Extended Rating Limits",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CATR", new NVSetting()
                        {
                            name = "CATR",
                            title = "TV Access/CAM TV Rating Limit",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAMR", new NVSetting()
                        {
                            name = "CAMR",
                            title = "TV Access/CAM MPAA Rating Limit",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },



            {
                "DVRP", new NVSetting()
                        {
                            name = "DVRP",
                            title = "????",
                            notes = "DVRInterface::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DVRK", new NVSetting()
                        {
                            name = "DVRK",
                            title = "DVRFsd MPEG ID",
                            notes = "DVRInterface::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DigA", new NVSetting()
                        {
                            name = "DigA",
                            title = "Digital Audio Mode",
                            notes = "DigitalAudioRestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },

            {
                "TVC2", new NVSetting()
                        {
                            name = "TVC2",
                            title = "TV Channel Flags",
                            notes = "TVController::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVCM", new NVSetting()
                        {
                            name = "TVCM",
                            title = "TV Channel Mode",
                            notes = "TVController::RestoreState",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "RCNT", new NVSetting()
                        {
                            name = "RCNT",
                            title = "Recent TV Channels",
                            notes = "TVRecent::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVLD", new NVSetting()
                        {
                            name = "TVLD",
                            title = "TV Log Disabled Mask",
                            notes = "TVLog::Open",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "VRSI", new NVSetting()
                        {
                            name = "VRSI",
                            title = "VCR Controller IR Setup Info",
                            notes = "VCRController::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "VRON", new NVSetting()
                        {
                            name = "VRON",
                            title = "VCR Controller IR Enabled",
                            notes = "VCRController::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "MLOC", new NVSetting()
                        {
                            name = "MLOC",
                            title = "Movie Search URL",
                            notes = "TVDatabase::RestoreState",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "IRBD", new NVSetting()
                        {
                            name = "IRBD",
                            title = "IR Blaster Delay",
                            notes = "IRBlasterDB::RestoreDelay",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "IRCS", new NVSetting()
                        {
                            name = "IRCS",
                            title = "IR Blaster Code Set History",
                            notes = "IRBlasterDB::RestoreCodeSetHistory",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "PWRF", new NVSetting()
                        {
                            name = "PWRF",
                            title = "Power on Flags",
                            notes = "System::PowerOnLoop/System::RestoreStatePhase1/System::RestoreStatePhase2",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "COPW", new NVSetting()
                        {
                            name = "COPW",
                            title = "Connect On Power Enabled",
                            notes = "System::RestoreStatePhase1",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "COPU", new NVSetting()
                        {
                            name = "COPU",
                            title = "Connect On Power URL",
                            notes = "System::RestoreStatePhase1",
                            data_type = NVDataType.NULL_TERMINATED_STRING,
                            data_editor = NVDataEditor.STRING_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CORF", new NVSetting()
                        {
                            name = "CORF",
                            title = "????",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },

            {
                "PrSt", new NVSetting()
                        {
                            name = "PrSt",
                            title = "????",
                            notes = "System::RestoreStatePhase2",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },


            {
                "TLCK", new NVSetting()
                        {
                            name = "TLCK",
                            title = "TV Access/CAM Blocked Channels Temoraily Unlocked???",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "LOCK", new NVSetting()
                        {
                            name = "LOCK",
                            title = "TV Access/CAM Blocked Channels Enabled",
                            notes = "TVAccess::RestoreState",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "BRDR", new NVSetting()
                        {
                            name = "BRDR",
                            title = "Screen Border Color",
                            notes = "System::RestoreStatePhase2",
                            data_type = NVDataType.YUV_COLOR,
                            data_editor = NVDataEditor.RGB_COLOR_PICKER,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "locp", new NVSetting()
                        {
                            name = "locp",
                            title = "Local Dialup POP Count",
                            notes = "System::RestoreStatePhase2",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "ITVE", new NVSetting()
                        {
                            name = "ITVE",
                            title = "iTV Enabled",
                            notes = "System::RestoreStatePhase2",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "SPos", new NVSetting()
                        {
                            name = "SPos",
                            title = "Screen Position",
                            notes = "VideoDriver::RestoreDisplayState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CHAN", new NVSetting()
                        {
                            name = "CHAN",
                            title = "Stored TV Tuner Band",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },

            {
                "LANG", new NVSetting()
                        {
                            name = "LANG",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DVRM", new NVSetting()
                        {
                            name = "DVRM",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "FMod", new NVSetting()
                        {
                            name = "FMod",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAMP", new NVSetting()
                        {
                            name = "CAMP",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CATV", new NVSetting()
                        {
                            name = "CATV",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CAEX", new NVSetting()
                        {
                            name = "CAEX",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CANR", new NVSetting()
                        {
                            name = "CANR",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "WEBL", new NVSetting()
                        {
                            name = "WEBL",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "RDVR", new NVSetting()
                        {
                            name = "RDVR",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "RDVL", new NVSetting()
                        {
                            name = "RDVL",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "WGPS", new NVSetting()
                        {
                            name = "WGPS",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "AUDL", new NVSetting()
                        {
                            name = "AUDL",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "AUDD", new NVSetting()
                        {
                            name = "AUDD",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "Anam", new NVSetting()
                        {
                            name = "Anam",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "JOYY", new NVSetting()
                        {
                            name = "JOYY",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "DKON", new NVSetting()
                        {
                            name = "DKON",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "REGD", new NVSetting()
                        {
                            name = "REGD",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "CHTV", new NVSetting()
                        {
                            name = "CHTV",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "UPIN", new NVSetting()
                        {
                            name = "UPIN",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "UPVR", new NVSetting()
                        {
                            name = "UPVR",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "HSCI", new NVSetting()
                        {
                            name = "HSCI",
                            title = "High Speed????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BOOLEAN,
                            data_editor = NVDataEditor.BOOLEAN_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "BILD", new NVSetting()
                        {
                            name = "BILD",
                            title = "????",
                            notes = "UltimateTV",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVUC", new NVSetting()
                        {
                            name = "TVUC",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVVI", new NVSetting()
                        {
                            name = "TVVI",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVEX", new NVSetting()
                        {
                            name = "TVEX",
                            title = "",
                            notes = "",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },            {
                "TVID", new NVSetting()
                        {
                            name = "TVID",
                            title = "",
                            notes = "",
                            data_type = NVDataType.INT32,
                            data_editor = NVDataEditor.INTEGER_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "SPOT", new NVSetting()
                        {
                            name = "SPOT",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "RTCT", new NVSetting()
                        {
                            name = "RTCT",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVSC", new NVSetting()
                        {
                            name = "TVSC",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "KARA", new NVSetting()
                        {
                            name = "KARA",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "VRCM", new NVSetting()
                        {
                            name = "VRCM",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVOT", new NVSetting()
                        {
                            name = "TVOT",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "TVMP", new NVSetting()
                        {
                            name = "TVMP",
                            title = "",
                            notes = "",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
            {
                "VAPt", new NVSetting()
                        {
                            name = "VAPt",
                            title = "Video Ad State",
                            notes = "VideoAdd::SaveState/VideoAdd::RestoreState",
                            data_type = NVDataType.BINARY_BLOB,
                            data_editor = NVDataEditor.HEX_EDITOR,
                            value = new NVSettingValue()
                            {
                                default_value = null
                            }
                        }
            },
        };
    }
}
