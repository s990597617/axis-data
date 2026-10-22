using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axis.Infrastructure.Host.JsonConverter;

/// <summary>
/// 将 <see cref="long"/> 序列化为 JSON 字符串，
/// 避免前端 JavaScript 因超出 <c>Number.MAX_SAFE_INTEGER</c> 导致精度丢失。
/// Read 端同时兼容字符串和数字。
/// </summary>
public class LongJsonConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.String
            ? long.Parse(reader.GetString()!)
            : reader.GetInt64();

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}

/// <summary>
/// <see cref="long?"/> 对应的 Converter。
/// </summary>
public class NullableLongJsonConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        return reader.TokenType == JsonTokenType.String
            ? long.Parse(reader.GetString()!)
            : reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Value.ToString());
    }
}
