using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TreeStore.Model.Abstractions.Json
{
    public sealed class FacetPropertyValueResultConverter : JsonConverter<FacetPropertyValueResult>
    {
        public override FacetPropertyValueResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            static Guid ReadId(ref Utf8JsonReader reader)
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                    if (nameof(FacetPropertyValueResult.Id).Equals(reader.GetString(), StringComparison.OrdinalIgnoreCase))
                        if (reader.Read() && reader.TokenType == JsonTokenType.String)
                            if (reader.TryGetGuid(out var id))
                                return id;

                throw new InvalidOperationException("Id property not found");
            }

            static FacetPropertyTypeValues ReadType(ref Utf8JsonReader reader)
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                    if ("$type".Equals(reader.GetString(), StringComparison.OrdinalIgnoreCase))
                        if (reader.Read() && reader.TokenType == JsonTokenType.Number)
                            if (reader.TryGetInt32(out var type))
                                return (FacetPropertyTypeValues)type;

                throw new InvalidOperationException("$type property not found");
            }

            static object? ReadValue(ref Utf8JsonReader reader, FacetPropertyTypeValues valueType)
            {
                if (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                    if (nameof(FacetPropertyValueResult.Value).Equals(reader.GetString(), StringComparison.OrdinalIgnoreCase))
                        if (reader.Read())
                            return reader.TokenType switch
                            {
                                JsonTokenType.Number => valueType switch
                                {
                                    FacetPropertyTypeValues.Decimal => reader.GetDecimal(),
                                    FacetPropertyTypeValues.Double => reader.GetDouble(),
                                    FacetPropertyTypeValues.Long => reader.GetInt64(),

                                    _ => throw new InvalidOperationException($"Json value {reader.GetDecimal()} wasn't converted")
                                },

                                JsonTokenType.String => valueType switch
                                {
                                    FacetPropertyTypeValues.DateTime => reader.GetDateTime(),
                                    FacetPropertyTypeValues.Guid => reader.GetGuid(),
                                    FacetPropertyTypeValues.String => reader.GetString(),

                                    _ => throw new InvalidOperationException($"Json value {reader.GetDecimal()} wasn't converted")
                                },

                                JsonTokenType.True => valueType switch
                                {
                                    FacetPropertyTypeValues.Bool => true,

                                    _ => throw new InvalidOperationException($"Json value {reader.TokenType} wasn't converted")
                                },

                                JsonTokenType.False => valueType switch
                                {
                                    FacetPropertyTypeValues.Bool => false,

                                    _ => throw new InvalidOperationException($"Json value {reader.TokenType} wasn't converted")
                                },

                                JsonTokenType.Null => null,

                                _ => throw new InvalidOperationException($"Token type {reader.TokenType} is unexpected")
                            };

                throw new InvalidOperationException("Value property not found");
            }

            var id = ReadId(ref reader);
            var type = ReadType(ref reader);
            var value = ReadValue(ref reader, type);
            var result = new FacetPropertyValueResult(
                Id: id,
                Type: type,
                Value: value);

            if (reader.Read() && reader.TokenType == JsonTokenType.EndObject)
                return result;

            throw new InvalidOperationException("Object has wrong format");
        }

        public override void Write(Utf8JsonWriter writer, FacetPropertyValueResult value, JsonSerializerOptions options)
        {
            static void WriteValue(Utf8JsonWriter writer, FacetPropertyTypeValues valueType, object? value)
            {
                var propertyName = nameof(FacetPropertyValueResult.Value);
                if (value is null)
                {
                    writer.WriteNull(propertyName);
                }
                else
                {
                    switch (valueType)
                    {
                        case FacetPropertyTypeValues.Bool:
                            writer.WriteBoolean(propertyName, (bool)value);
                            break;

                        case FacetPropertyTypeValues.DateTime:
                            writer.WriteString(propertyName, ((DateTime)value).ToString("o"));
                            break;

                        case FacetPropertyTypeValues.Decimal:
                            writer.WriteNumber(propertyName, (decimal)value);
                            break;

                        case FacetPropertyTypeValues.Double:
                            writer.WriteNumber(propertyName, (double)value);
                            break;

                        case FacetPropertyTypeValues.Guid:
                            writer.WriteString(propertyName, (Guid)value);
                            break;

                        case FacetPropertyTypeValues.Long:
                            writer.WriteNumber(propertyName, (long)value);
                            break;

                        case FacetPropertyTypeValues.String:
                            writer.WriteString(propertyName, (string)value);
                            break;

                        default:
                            throw new ArgumentException($"{valueType} is unknown");
                    }
                }
            }
            writer.WriteStartObject();
            writer.WriteString(nameof(FacetPropertyValueResult.Id), value.Id);
            writer.WriteNumber("$type", (int)value.Type);
            WriteValue(writer, value.Type, value.Value);
            writer.WriteEndObject();
        }
    }
}