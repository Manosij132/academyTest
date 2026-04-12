using System.Text.Json;

namespace Academy.API.Helpers
{
    public class JsonOutputParser<T>
    {
        public T Parse(string json)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                return JsonSerializer.Deserialize<T>(json, options) ?? throw new JsonException("Deserialized object is null.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse JSON.", ex);
            }
        }
    }
}
