using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace StandardApiTools {
    public partial class StdApiWebException {
        public class CustomDataIndex: IDictionary<string, object> {
            public CustomDataIndex(Dictionary<string, object> dict = null) {
                this.dict = dict ?? new Dictionary<string, object>();
            }

            private Dictionary<string, object> dict;
            private static string[] ReservedKeys = new string[] { "message", "content" };

            public object this[string key] { get => dict[key]; set => dict[key] = value; }
            public ICollection<string> Keys => dict.Keys;
            public ICollection<object> Values => dict.Values;
            public int Count => ((ICollection<KeyValuePair<string, object>>)dict).Count;
            public bool IsReadOnly => ((ICollection<KeyValuePair<string, object>>)dict).IsReadOnly;


            public void Add(string key, object value) {
                var newkey = key;
                int keyCount = 2;
                while (dict.ContainsKey(newkey) || ReservedKeys.Contains(newkey)) {
                    if (keyCount == int.MaxValue) { key += "_"; keyCount = 1; }
                    newkey = $"{key}({keyCount++})";
                }
                dict.Add(newkey, value);
                //return newkey;
            }

            public void Add(KeyValuePair<string, object> item) {
                Add(item.Key, item.Value);
            }

            public void Clear() {
                ((ICollection<KeyValuePair<string, object>>)dict).Clear();
            }

            public bool Contains(KeyValuePair<string, object> item) {
                return ((ICollection<KeyValuePair<string, object>>)dict).Contains(item);
            }

            public bool ContainsKey(string key) {
                return ((IDictionary<string, object>)dict).ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
                ((ICollection<KeyValuePair<string, object>>)dict).CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
                return ((IEnumerable<KeyValuePair<string, object>>)dict).GetEnumerator();
            }

            public bool Remove(string key) {
                return ((IDictionary<string, object>)dict).Remove(key);
            }

            public bool Remove(KeyValuePair<string, object> item) {
                return ((ICollection<KeyValuePair<string, object>>)dict).Remove(item);
            }

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) {
                return ((IDictionary<string, object>)dict).TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return ((IEnumerable)dict).GetEnumerator();
            }
        }
    }
}
