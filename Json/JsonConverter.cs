using NpgsqlTypes;
using Shared.Database;
using Shared.Extensions;
using Shared.Logger;
using JsonResolver = Utf8Json.Resolvers.StandardResolver;

namespace Shared.Json;

public enum JsonType
{
    SnakeCase,
    CamelCase,
    PascalCase,
}

public static class JsonChecker
{
    public static bool IsArray(string? jsonString)
    {
        return !string.IsNullOrEmpty(jsonString) && jsonString.TrimStart().StartsWith("[");
    }

    public static bool IsObject(string? jsonString)
    {
        return !string.IsNullOrEmpty(jsonString) && jsonString.TrimStart().StartsWith("{");
    }
}

public static class JsonConverter
{
    public static async Task<bool> ToJsonFile(
        string fileName,
        object? data,
        JsonType? jsonType = null
    )
    {
        try
        {
            var root = Directory.GetCurrentDirectory();
            var path = $"{root}/{fileName}.json";

            await using FileStream createStream = File.Create(path);
            await System.Text.Json.JsonSerializer.SerializeAsync(createStream, data);
            return true;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public static async Task<T?> FromJsonFile<T>(
        string fileName,
        JsonType jsonType = JsonType.PascalCase
    )
    {
        try
        {
            string root = Directory.GetCurrentDirectory();
            string path = $"{root}/{fileName}.json";

            using var r = new StreamReader(path);
            var json = await r.ReadToEndAsync();
            var items = Deserialize<T>(json, jsonType);
            return items;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JsonConverter][FromJsonFile]: {ex.Message}");
            return default(T);
        }
    }

    public static string ToJson(object? data, JsonType? jsonType = null, bool? allowNull = true)
    {
        try
        {
            if (data == null)
            {
                return string.Empty;
            }

            switch (jsonType)
            {
                case JsonType.SnakeCase:
                {
                    var resolver =
                        allowNull == true
                            ? JsonResolver.AllowPrivateSnakeCase
                            : JsonResolver.AllowPrivateExcludeNullSnakeCase;
                    return Utf8Json.JsonSerializer.ToJsonString(data, resolver);
                }
                case JsonType.CamelCase:
                {
                    var resolver =
                        allowNull == true
                            ? JsonResolver.AllowPrivateCamelCase
                            : JsonResolver.AllowPrivateExcludeNullCamelCase;
                    return Utf8Json.JsonSerializer.ToJsonString(data, resolver);
                }
                case null:
                case JsonType.PascalCase:
                default:
                    return Utf8Json.JsonSerializer.ToJsonString(data);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return string.Empty;
        }
    }

    public static T Deserialize<T>(string? s, JsonType jsonType = JsonType.PascalCase)
    {
        try
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("string can not be empty");
            }

            switch (jsonType)
            {
                case JsonType.SnakeCase:
                {
                    var response = Utf8Json.JsonSerializer.Deserialize<T>(
                        s,
                        JsonResolver.AllowPrivateExcludeNullSnakeCase
                    );
                    return response;
                }
                case JsonType.CamelCase:
                {
                    var response = Utf8Json.JsonSerializer.Deserialize<T>(
                        s,
                        JsonResolver.AllowPrivateExcludeNullCamelCase
                    );
                    return response;
                }
                case JsonType.PascalCase:
                default:
                {
                    var pascalCaseResponse = Utf8Json.JsonSerializer.Deserialize<T>(s);
                    return pascalCaseResponse;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"JsonConverter: {e} - {s}");
            throw;
        }
    }

    public static T Deserialize<T>(
        string? s,
        T defaultValue,
        JsonType jsonType = JsonType.PascalCase
    )
    {
        try
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("string can not be empty");
            }

            switch (jsonType)
            {
                case JsonType.SnakeCase:
                {
                    var response = Utf8Json.JsonSerializer.Deserialize<T>(
                        s,
                        JsonResolver.AllowPrivateExcludeNullSnakeCase
                    );
                    return response;
                }
                case JsonType.CamelCase:
                {
                    var response = Utf8Json.JsonSerializer.Deserialize<T>(
                        s,
                        JsonResolver.AllowPrivateExcludeNullCamelCase
                    );
                    return response;
                }
                case JsonType.PascalCase:
                default:
                    var finalResponse = Utf8Json.JsonSerializer.Deserialize<T>(s);
                    return finalResponse;
            }
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"[JsonConverter][Deserialize]: {ex.Message} |input: {s}");
            return defaultValue;
        }
    }

    public static JsonParameter ToJsonParameter(
        string? payload,
        NpgsqlDbType type = NpgsqlDbType.Json,
        DefaultPgJsonType defaultPgJsonType = DefaultPgJsonType.Object
    )
    {
        if (payload.IsNull())
        {
            payload = defaultPgJsonType == DefaultPgJsonType.Array ? "[]" : "{}";
        }

        return new(payload, type);
    }
}
