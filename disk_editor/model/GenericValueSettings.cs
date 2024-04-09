using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    class GenericValueSetting
    {
        public string title { get; set; }
        public string value { get; set; }
        public override string ToString()
        {
            return value;
        }
    }

    internal class GenericValueSettings
    {
        public bool check_duplicate_values = true;
        public List<GenericValueSetting> settings = null;

        public int contains_value(string value)
        {
            for (var i = 0; i < this.settings.Count; i++)
            {
                if (this.settings[i].value == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public void add_value(string value, string title = "")
        {
            if(value == null || value == "")
            {
                return;
            }

            var current_index = this.contains_value(value);

            if (this.check_duplicate_values && current_index >= 0)
            {
                var _settings = new List<GenericValueSetting>();

                _settings.Add(this.settings[current_index]);

                for (var i = 0; i < this.settings.Count; i++)
                {
                    if(i != current_index)
                    {
                        _settings.Add(this.settings[i]);
                    }
                }

                this.settings = _settings;
            }
            else
            {
                this.settings.Add(
                    new GenericValueSetting()
                    {
                        title = title,
                        value = value
                    }
                );
            }
        }

        public List<GenericValueSetting> get_settings()
        {
            return this.settings;
        }

        public GenericValueSettings(List<GenericValueSetting> settings, bool check_duplicate_values = true)
        {
            if (settings == null)
            {
                this.settings = new List<GenericValueSetting>();
            }
            else
            {
                this.settings = settings;
            }

            this.check_duplicate_values = check_duplicate_values;
        }
    }
}
