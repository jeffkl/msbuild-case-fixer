using System;
using System.Collections;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents a read-only wrapper around an <see cref="IDictionary{TKey, TValue}" />.
    /// </summary>
    internal record struct ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public ReadOnlyDictionaryWrapper(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        public int Count => _dictionary.Count;

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values;

        public TValue this[TKey key] => _dictionary[key];

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);
    }
}