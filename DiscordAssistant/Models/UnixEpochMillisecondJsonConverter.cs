using Newtonsoft.Json;
using System;

namespace DiscordAssistant.Models
{
    public class UnixEpochMillisecondJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) ||
                objectType == typeof(DateTimeOffset);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long time = 0;
            if (reader.Value is long timeLong)
            {
                time = timeLong;
            }
            else if (reader.Value is int timeInt)
            {
                time = timeInt;
            }
            else if (reader.Value is string timeStr)
            {
                time = long.Parse((string)reader.Value);
            }
            else
            {
                throw new Exception($"Unknown readerValue during UnixEpochMillisecondJsonConverter.ReadJson: {reader.Value.GetType()}.");
            }

            var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(time);

            if (objectType == typeof(DateTime))
            {
                return new DateTime(dateTimeOffset.Ticks, DateTimeKind.Utc);
            }
            else
            {
                return dateTimeOffset;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long miliseconds = 0;
            if (value is DateTime dt)
            {
                miliseconds = (long)(dt - new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
            else if (value is DateTimeOffset dto)
            {
                miliseconds = dto.ToUnixTimeMilliseconds();
            }
            writer.WriteValue(miliseconds);
        }
    }
}
