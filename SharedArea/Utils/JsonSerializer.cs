
using Newtonsoft.Json;

namespace SharedArea.Utils
{
    public static class JsonSerializer
    {
        public static string SerializeObject(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
            settings.Converters.Add(new MessageConverter());
            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}