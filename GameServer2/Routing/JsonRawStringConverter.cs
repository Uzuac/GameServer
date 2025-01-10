using System.Text.Json.Serialization;
using System.Text.Json;

namespace GameServer.Application.Routing
{
    public class JsonRawStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            return jsonDoc.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.Parse(value);
            jsonDoc.WriteTo(writer);
        }
    }
}
