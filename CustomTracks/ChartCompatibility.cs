using System;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace TrombLoader.CustomTracks;

public class ChartCompatibility
{
    private static ManualLogSource _logger = Logger.CreateLogSource("TrombLoader.Compatibility");

    /**
     * Helper to fix up charts that have strings and floats specified for integer fields.
     * TrombLoader 1 was too lenient and accepted these unquestionably.
     * Any chart created after TrombLoader 2 is unlikely to have these issues.
     */
    public class IntOrFloatConverter : JsonConverter<int>
    {
        private readonly string _fieldName;

        public IntOrFloatConverter(string fieldName)
        {
            _fieldName = fieldName;
        }

        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) throw new JsonException("Expected number, got null");

            if (reader.TokenType is JsonToken.Float or JsonToken.String)
            {
                if (reader.TokenType is JsonToken.Float)
                {
                    var chartName = (string) serializer.Context.Context;
                    _logger.LogWarning($"Chart '{chartName}' has invalid type {reader.TokenType} on field {_fieldName} (expected an integer)");
                }

                return (int) Convert.ToDouble(reader.Value);
            }

            if (reader.TokenType != JsonToken.Integer)
            {
                throw new JsonException($"Expected number, got {reader.TokenType}");
            }

            return Convert.ToInt32(reader.Value);
        }
    }
}
