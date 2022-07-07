using System;
using System.Collections;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    internal class ReadOnlyDictionaryWrapper : IReadOnlyDictionary<string, string>
    {
        private readonly IDictionary<string, string> _dictionary;

        public ReadOnlyDictionaryWrapper(IDictionary<string, string> dictionary)
        {
            _dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        public int Count => _dictionary.Count;
        
        public IEnumerable<string> Keys => _dictionary.Keys;
        
        public IEnumerable<string> Values => _dictionary.Values;
        
        public string this[string key] => _dictionary[key];

        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public bool TryGetValue(string key, out string value) => _dictionary.TryGetValue(key, out value);
    }
}