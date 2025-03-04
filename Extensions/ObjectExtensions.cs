using System;
using System.Diagnostics.CodeAnalysis;
using Shared.EnvVarLoader;

namespace Shared.Extensions;

public static class Extension
{
    public static string WithEnv(this string data) => $"{data}_{Env.STAGE}";

    public static string NameOf(this object o)
    {
        return o.GetType().Name;
    }

    public static bool IsNotNull([NotNullWhen(true)] this object? data) => data != null;

    public static bool IsNull([NotNullWhen(false)] this object? data) => data == null;

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }

    static Random random = new Random();

    public static string GetRandomHexNumber(this int digits)
    {
        byte[] buffer = new byte[digits / 2];
        random.NextBytes(buffer);
        string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
        if (digits % 2 == 0)
        {
            return result;
        }
        return result + random.Next(16).ToString("X");
    }

    public static async Task Delay(int deltaStart = 1000, int deltaEnd = 2300)
    {
        Random rnd = new Random();
        int time = rnd.Next(deltaStart, deltaEnd);
        await Task.Delay(time);
        return;
    }
}
