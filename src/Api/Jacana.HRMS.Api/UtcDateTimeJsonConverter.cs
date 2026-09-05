using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Jacana.HRMS.Api;

/// <summary>
/// Ensures every DateTime deserialized from JSON is UTC before it reaches the
/// persistence layer. Npgsql rejects non-UTC DateTime values on PostgreSQL
/// 'timestamp with time zone' columns ("Cannot write DateTime with Kind=Unspecified"),
/// which previously made every user-supplied date (immunization administered dates,
/// condition onset, appointment times) fail with a 500. Date-only strings such as
/// "2026-09-05" are interpreted as UTC midnight so they round-trip cleanly.
/// </summary>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();
        if (string.IsNullOrWhiteSpace(raw))
            return default;

        // Date-only payloads ("yyyy-MM-dd") → UTC midnight of that calendar date.
        if (DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateOnly))
            return DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed.Kind switch
            {
                DateTimeKind.Utc => parsed,
                DateTimeKind.Local => parsed.ToUniversalTime(),
                // No offset/zone info in the payload — treat as UTC (server convention).
                _ => DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
            };
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : value.Kind == DateTimeKind.Local
                ? value.ToUniversalTime()
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utc.ToString("O", CultureInfo.InvariantCulture));
    }
}
