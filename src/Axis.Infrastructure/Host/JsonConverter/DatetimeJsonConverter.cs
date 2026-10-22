using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axis.Infrastructure.Host.JsonConverter;

/// <summary>
/// 全局日期格式转换器（基于 System.Text.Json）。
///
/// <para><b>默认行为</b>：按 ISO 8601 读写（<c>yyyy-MM-ddTHH:mm:ss.fffK</c>），与 <see cref="Host.Responses.ResponseResult{T}.Timestamp"/>
/// 的 <see cref="DateTimeOffset"/> 保持一致，避免前端同一响应里拿到两种日期格式。</para>
///
/// <para>如果团队约定必须兼容 <c>yyyy-MM-dd HH:mm:ss</c> 这种无时区的旧格式，
/// 请用构造函数显式传入格式字符串。</para>
///
/// <para>Read 端兼容：字符串 ISO 8601 / 字符串 "yyyy-MM-dd HH:mm:ss" / 数字 unix 秒 / 空值。</para>
/// </summary>
public class DatetimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>推荐的默认写出格式（ISO 8601 带毫秒 + 时区）。</summary>
    public const string DefaultFormat = "yyyy-MM-ddTHH:mm:ss.fffK";

    private readonly string _format;

    /// <summary>默认 ISO 8601；传 <c>"yyyy-MM-dd HH:mm:ss"</c> 可还原旧行为。</summary>
    public DatetimeJsonConverter(string format = DefaultFormat) => _format = format;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var s = reader.GetString();
                return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var d)
                    ? d
                    : reader.GetDateTime();
            case JsonTokenType.Number:
                // 兼容 unix 秒 / 毫秒
                var n = reader.GetInt64();
                return n > 10_000_000_000L
                    ? DateTimeOffset.FromUnixTimeMilliseconds(n).LocalDateTime
                    : DateTimeOffset.FromUnixTimeSeconds(n).LocalDateTime;
            default:
                return reader.GetDateTime();
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
}

/// <summary>
/// <see cref="DateTime?"/> 对应的 Converter。默认复用 <see cref="DatetimeJsonConverter.DefaultFormat"/>。
/// </summary>
public class NullableDatetimeJsonConverter : JsonConverter<DateTime?>
{
    private readonly string _format;

    public NullableDatetimeJsonConverter(string format = DatetimeJsonConverter.DefaultFormat) => _format = format;

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.String && string.IsNullOrEmpty(reader.GetString())) return null;
        return DateTime.TryParse(reader.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var d)
            ? d
            : reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Value.ToString(_format, CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// <see cref="DateTimeOffset"/> Converter。推荐在 API 层对外使用 <c>DateTimeOffset</c> 而不是 <c>DateTime</c>，
/// 避免时区歧义（<see cref="Host.Responses.ResponseResult{T}.Timestamp"/> 就是本类型）。
/// </summary>
public class DateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    private readonly string _format;

    public DateTimeOffsetJsonConverter(string format = DatetimeJsonConverter.DefaultFormat) => _format = format;

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.String
            ? DateTimeOffset.Parse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            : reader.GetDateTimeOffset();

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
}

/// <summary><see cref="DateTimeOffset?"/> Converter。</summary>
public class NullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    private readonly string _format;

    public NullableDateTimeOffsetJsonConverter(string format = DatetimeJsonConverter.DefaultFormat) => _format = format;

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        var s = reader.GetString();
        if (string.IsNullOrEmpty(s)) return null;
        return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Value.ToString(_format, CultureInfo.InvariantCulture));
    }
}