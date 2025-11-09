using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace RemoteFlutterSharp.Dynamic;

public sealed class DynamicContentBuilder
{
    private readonly Dictionary<string, object> _root = new(StringComparer.Ordinal);

    public DynamicContentBuilder Set(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key is required.", nameof(key));
        }

        _root[key] = Normalize(value);
        return this;
    }

    public IReadOnlyDictionary<string, object> Build() => _root;

    public string ToJsonString(bool indented = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
        };

        return JsonSerializer.Serialize(_root, options);
    }

    private static object Normalize(object value)
    {
        switch (value)
        {
            case null:
                throw new ArgumentNullException(nameof(value), "Dynamic content does not allow null values.");
            case string or bool:
                return value;
            case sbyte or byte or short or ushort or int or uint or long:
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            case float f:
                return (double)f;
            case double or decimal:
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            case IDictionary dictionary:
                return NormalizeDictionary(dictionary);
            case IEnumerable enumerable when value is not string:
                return NormalizeList(enumerable);
            default:
                throw new ArgumentException($"Unsupported dynamic content type: {value.GetType().FullName}");
        }
    }

    private static List<object> NormalizeList(IEnumerable source)
    {
        var list = new List<object>();
        foreach (var item in source)
        {
            if (item is null)
            {
                throw new ArgumentException("Dynamic content lists cannot contain null values.");
            }

            list.Add(Normalize(item));
        }

        return list;
    }

    private static IDictionary<string, object> NormalizeDictionary(IDictionary source)
    {
        var dict = new Dictionary<string, object>(source.Count, StringComparer.Ordinal);
        foreach (DictionaryEntry entry in source)
        {
            if (entry.Key is not string key)
            {
                throw new ArgumentException("Dynamic content maps require string keys.");
            }

            if (entry.Value is null)
            {
                throw new ArgumentException("Dynamic content maps cannot contain null values.");
            }

            dict[key] = Normalize(entry.Value);
        }

        return dict;
    }
}
