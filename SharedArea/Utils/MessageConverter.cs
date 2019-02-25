using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharedArea.Entities;

namespace SharedArea.Utils
{
    public class MessageConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Message));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);

            switch (jo["type"].Value<string>())
            {
                case "TextMessage":
                    return jo.ToObject<TextMessage>(serializer);
                case "PhotoMessage":
                    return jo.ToObject<PhotoMessage>(serializer);
                case "AudioMessage":
                    return jo.ToObject<AudioMessage>(serializer);
                case "VideoMessage":
                    return jo.ToObject<VideoMessage>(serializer);
                case "ServiceMessage":
                    return jo.ToObject<ServiceMessage>(serializer);
                default:
                {
                    return jo.ToObject<Message>(Newtonsoft.Json.JsonSerializer.CreateDefault());
                }
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}