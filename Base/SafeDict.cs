using System.Collections;
using System.Collections.Generic;

namespace StandardApiTools {
    public class SafeDict : IEnumerable<KeyValuePair<string, object>> {

        private readonly Dictionary<string, object> dict = new Dictionary<string, object>();


        public int Count => dict.Count;


        public Dictionary<string, object>.KeyCollection Keys => dict.Keys;


        public void Clear() => dict.Clear();


        public bool ContainsKey(string key) => dict.ContainsKey(key);


        public bool Add(string key, object value) => dict.TryAdd(key, value);


        public bool AutoAdd(string key, object value) {
            var newkey = key;
            int keyCount = 2;
            while (dict.ContainsKey(newkey)) {
                if (keyCount == int.MaxValue) return false;
                newkey = $"{key}({keyCount++})";
            }
            dict.Add(key, value);
            return true;
        }


        public void Set(string key, object value) {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }


        public object Get(string key) {
            if (dict.TryGetValue(key, out var value)) return value; return null;
        }

        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => dict.GetEnumerator();
    }
}
