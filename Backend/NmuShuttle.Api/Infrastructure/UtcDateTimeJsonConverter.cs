using System.Text.Json;
using System.Text.Json.Serialization;

namespace NmuShuttle.Api.Infrastructure;

/// <summary>
/// Serializes every <see cref="DateTime"/> the API returns as a UTC instant with a
/// trailing "Z" (ISO 8601, e.g. "2026-10-15T12:30:00.000Z").
///
/// Why this exists: all timestamps are stored in SQL Server as <c>datetime2</c> in UTC,
/// but EF Core materializes them with <see cref="DateTimeKind.Unspecified"/> (SQL has no
/// offset). System.Text.Json serializes an Unspecified <see cref="DateTime"/> WITHOUT a
/// "Z", so clients — notably Dart's <c>DateTime.parse</c> — read it as LOCAL time. On a
/// SAST (UTC+2) device that makes a brand-new row look two hours old. Stamping the "Z"
/// here is the single systemic fix for every timestamp the API emits.
///
/// Convention: a value with <see cref="DateTimeKind.Unspecified"/> is assumed to already
/// be UTC (matches the storage rule); a <see cref="DateTimeKind.Local"/> value is converted
/// to UTC first.
/// </summary>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind switch
        {
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        // "O" on a Utc-kind DateTime always ends with "Z".
        writer.WriteStringValue(utc.ToString("O"));
    }
}

/// <summary>
/// Nullable companion to <see cref="UtcDateTimeJsonConverter"/> so <c>DateTime?</c>
/// properties (e.g. Report.UpdatedAt) get the same UTC/"Z" treatment.
/// </summary>
public sealed class NullableUtcDateTimeJsonConverter : JsonConverter<DateTime?>
{
    private static readonly UtcDateTimeJsonConverter Inner = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        return Inner.Read(ref reader, typeof(DateTime), options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        Inner.Write(writer, value.Value, options);
    }
}
