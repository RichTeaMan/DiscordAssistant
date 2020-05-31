using Newtonsoft.Json;
using System;

namespace DiscordAssistant.Models
{
    public class DurationMillisecondJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int) ||
                objectType == typeof(long) ||
                objectType == typeof(string) ||
                objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long duration = 0;
            if (reader.Value is long timeLong)
            {
                duration = timeLong;
            }
            else if (reader.Value is int timeInt)
            {
                duration = timeInt;
            }
            else if (reader.Value is string timeStr)
            {
                duration = long.Parse(timeStr);
            }
            else if(reader.Value is double timeDbl)
            {
                duration = (long)timeDbl;
            }
            else
            {
                throw new Exception($"Unknown readerValue during DurationMillisecondJsonConverter.ReadJson: {reader.Value.GetType()}.");
            }

            var timespan = TimeSpan.FromMilliseconds(duration);
            return timespan;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long miliseconds = 0;
            if (value is TimeSpan ts)
            {
                miliseconds = (long)ts.TotalMilliseconds;
            }
            writer.WriteValue(miliseconds);
        }
    }
}
