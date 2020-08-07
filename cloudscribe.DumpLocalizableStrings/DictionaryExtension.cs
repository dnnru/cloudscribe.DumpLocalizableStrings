#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace cloudscribe.DumpLocalizableStrings
{
    public static class DictionaryExtension
    {
        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            return Get(dictionary, key, default(T));
        }

        public static T Get<T>(this IDictionary<string, object> dictionary, string key, T defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            object value;
            if (dictionary.TryGetValue(key, out value))
            {
                return (T) value;
            }

            return defaultValue;
        }

        public static TValue GetValueOrDefault<TValue, TKey>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            CheckDictionaryIsNull(dictionary);
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }

        public static TValue GetValue<TValue, TKey>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValue)
        {
            CheckDictionaryIsNull(dictionary);
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue();
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createFn)
        {
            CheckDictionaryIsNull(dictionary);
            lock (dictionary)
            {
                TValue val;
                if (!dictionary.TryGetValue(key, out val))
                {
                    dictionary[key] = val = createFn(key);
                }

                return val;
            }
        }

        public static bool IsNullOrEmpty(this IDictionary dictionary)
        {
            return dictionary == null || dictionary.Count == 0;
        }

        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Action<TKey, TValue> onEachFn)
        {
            CheckDictionaryIsNull(dictionary);
            foreach (var entry in dictionary)
            {
                onEachFn(entry.Key, entry.Value);
            }
        }

        public static void AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            CheckDictionaryIsNull(dictionary);
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }

        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            CheckDictionaryIsNull(dictionary);
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
            else
            {
                dictionary[key] = value;
            }
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary.ToDictionary(val => val.Key, val => val.Value);
        }

        public static void DeleteIfExistsKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            CheckDictionaryIsNull(dictionary);
            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
            }
        }

        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            CheckDictionaryIsNull(dictionary);
            CheckKeyValuePairIsNull(key, value);
            dictionary[key] = value;
        }

        public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> pair)
        {
            CheckDictionaryIsNull(dictionary);
            CheckKeyValuePairIsNull(pair);
            dictionary[pair.Key] = pair.Value;
        }

        public static void DeleteIfExistsValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            CheckDictionaryIsNull(dictionary);
            if (!dictionary.ContainsValue(value))
            {
                return;
            }

            var key = dictionary.GetKeyFromValue(value);
            dictionary.Remove(key);
        }

        public static bool AreValuesEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            CheckDictionaryIsNull(dictionary);
            return dictionary.All(x => x.Value == null);
        }

        public static bool AreKeysEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            CheckDictionaryIsNull(dictionary);
            return dictionary.All(x => x.Key == null);
        }

        private static TKey GetKeyFromValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            var keys = new List<TKey>();
            foreach (var pair in dictionary)
            {
                AddToKeysList(keys, pair, value);
            }

            CheckCountGreaterZero(keys.Count, value);
            return !keys.Any() ? default(TKey) : keys.First();
        }

        private static void AddToKeysList<TKey, TValue>(List<TKey> keys, KeyValuePair<TKey, TValue> pair, TValue value)
        {
            if (pair.Value.Equals(value))
            {
                keys.Add(pair.Key);
            }
        }

        private static void CheckCountGreaterZero<TValue>(int count, TValue value)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > 1)
            {
                throw new ArgumentException(nameof(value));
            }
        }

        private static void CheckDictionaryIsNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
        }

        private static void CheckKeyValuePairIsNull<TKey, TValue>(KeyValuePair<TKey, TValue> pair)
        {
            if (pair.Key == null || pair.Value == null)
            {
                throw new ArgumentNullException(nameof(pair));
            }
        }

        private static void CheckKeyValuePairIsNull<TKey, TValue>(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
        }
    }
}
