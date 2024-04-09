using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace disk_editor
{
    public class WebTVJSONContractResolver : DefaultContractResolver
    {
        public static readonly WebTVJSONContractResolver Instance = new WebTVJSONContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(WebTVPartition)
            && (property.PropertyName != "id" && property.PropertyName != "name" && property.PropertyName != "type" && property.PropertyName != "sector_start" && property.PropertyName != "sector_length" && property.PropertyName != "lost_sector_length" && property.PropertyName != "found_sector_length" && property.PropertyName != "delegate_filename"))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        /*
                        WebTVPartition e = (WebTVPartition)instance;
                        var value = e.GetType().GetProperty(property.PropertyName).GetValue(e, null);
                        */

                        return false;
                    };
            }

            return property;
        }
    }

    public class WebTVJSONConverter : JsonConverter
    {

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            List<KeyValuePair<string, object>> list = value as List<KeyValuePair<string, object>>;
            writer.WriteStartArray();
            foreach (var item in list)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(item.Key);

                var jsonValue = JsonConvert.SerializeObject(item.Value);
                writer.WriteValue(jsonValue);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<KeyValuePair<string, object>>);
        }
    }
}
