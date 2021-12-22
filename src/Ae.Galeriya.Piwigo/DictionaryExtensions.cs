using System;
using System.Collections.Generic;

namespace Ae.Galeriya.Piwigo
{
    internal static class DictionaryExtensions
    {
        public static TValue? GetOptional<TValue>(this IReadOnlyDictionary<string, IConvertible> dictionary, string key)
            where TValue : struct
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return GetValue<TValue>(value);
            }

            return default;
        }

        public static string? GetOptional(this IReadOnlyDictionary<string, IConvertible> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                var str = value.ToString(null);
                return string.IsNullOrWhiteSpace(str) ? null : str;
            }

            return null;
        }

        public static bool TryGetOptional<TValue>(this IReadOnlyDictionary<string, IConvertible> dictionary, string key, out TValue value)
            where TValue : notnull
        {
            if (dictionary.TryGetValue(key, out var convertible))
            {
                value = GetValue<TValue>(convertible);
                return true;
            }

            value = default;
            return false;
        }

        public static TValue GetRequired<TValue>(this IReadOnlyDictionary<string, IConvertible> dictionary, string key)
            where TValue : notnull
        {
            return GetValue<TValue>(dictionary[key]);
        }

        private static TValue GetValue<TValue>(IConvertible convertible)
        {
            return (TValue)convertible.ToType(typeof(TValue), null); ;
        }
    }
}
