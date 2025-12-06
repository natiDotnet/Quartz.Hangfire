using System.Text.Json;

namespace Quartz.Hangfire;

public static class ArgumentDeserializer
{
    public static object?[] DeserializeArgs(string json)
    {
        List<ArgInfo>? items = JsonSerializer.Deserialize<List<ArgInfo>>(json)!;

        object?[] result = new object?[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            result[i] = ConvertToType(items[i].Value, items[i].Type);
        }
        return result;
    }

    private static object? ConvertToType(JsonElement? value, string typeFullName)
    {
        if (value is null || value.Value.ValueKind == JsonValueKind.Null)
            return null;

        var type = Type.GetType(typeFullName, throwOnError: true)!;

        return type switch
        {
            // Fast path for common types
            not null when type == typeof(string) => value.Value.GetString(),
            not null when type == typeof(int) => value.Value.GetInt32(),
            not null when type == typeof(long) => value.Value.GetInt64(),
            not null when type == typeof(bool) => value.Value.GetBoolean(),
            not null when type == typeof(double) => value.Value.GetDouble(),
            not null when type == typeof(decimal) => value.Value.GetDecimal(),
            not null when type == typeof(DateTime) => value.Value.GetDateTime(),
            not null when type == typeof(DateTimeOffset) => value.Value.GetDateTimeOffset(),
            not null when type == typeof(Guid) => value.Value.GetGuid(),

            // Fallback: use ChangeType or JsonSerializer
            _ => JsonSerializer.Deserialize(value.Value.GetRawText(), type!)
                 ?? Convert.ChangeType(value.Value.ToString(), type!)
        };
    }

    record ArgInfo(JsonElement? Value, string Type);
}
