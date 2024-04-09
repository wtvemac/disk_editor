using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace disk_editor
{
    internal class JSONSettings
    {
        public static string base_path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\disk_editor";

        public static T get<T>(string location)
        {
            string[] parts = location.Split(new char[] { '\\', '/' });

            var path = "";
            var setting = parts[parts.Length - 1];

            if (parts.Length > 1)
            {
                path = "\\" + String.Join("\\", parts, 0, (parts.Length - 1));
            }

            try
            {
                var full_directory_path = base_path + "\\" + path;

                Directory.CreateDirectory(full_directory_path);

                var json = File.ReadAllText(full_directory_path + "\\" + setting + ".json");

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        public static bool set(string location, object settings)
        {
            string[] parts = location.Split(new char[] { '\\', '/' });

            var path = "";
            var setting = parts[parts.Length - 1];

            if (parts.Length > 1)
            {
                path = "\\" + String.Join("\\", parts, 0, (parts.Length - 1));
            }

            try
            {
                var full_directory_path = base_path + "\\" + path;

                Directory.CreateDirectory(full_directory_path);

                JsonSerializerSettings json_options = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = new WebTVJSONContractResolver(),
                    Converters = new[] { new WebTVJSONConverter() },
                };
                var json = JsonConvert.SerializeObject(settings, json_options);

                File.WriteAllText(full_directory_path + "\\" + setting + ".json", json);

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }

        }
    }
}
