using System;
using System.Collections.Generic;

namespace Ae.Galeriya.Piwigo
{
    internal static class DictionaryExtensions
    {
        public static TValue? GetOptionalValue<TValue>(this IReadOnlyDictionary<string, IConvertible> dictionary, string key)
            where TValue : notnull
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                return (TValue)value.ToType(typeof(TValue), null);
            }

            return default;
        }

        public static bool TryGetOptionalValue<TValue>(this IReadOnlyDictionary<string, IConvertible> dictionary, string key, out TValue value)
            where TValue : notnull
        {
            if (dictionary.TryGetValue(key, out var v))
            {
                value = (TValue)v.ToType(typeof(TValue), null);
                return true;
            }

            value = default;
            return false;
        }

        public static TValue GetRequiredValue<TValue>(this IReadOnlyDictionary<string, IConvertible> dictionary, string key)
            where TValue : notnull
        {
            return (TValue)dictionary[key].ToType(typeof(TValue), null);
        }
    }
}
