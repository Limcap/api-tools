using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace StandardApiTools {

    public class StdApiCustomData: IEnumerable<KeyValuePair<string, object>> {

        public StdApiCustomData(IDictionary<string,object> source = null) {
            dict = source ?? new Dictionary<string, object>();
        }
        

        private static string[] ReservedKeys = new string[] { "message", "content" };


        private readonly IDictionary<string, object> dict = new Dictionary<string, object>();


        public int Count => dict.Count;


        //public Dictionary<string, object>.KeyCollection Keys => dict.Keys;
        public ICollection<string> Keys => dict.Keys;


        public void Clear() => dict.Clear();


        public bool ContainsKey(string key) => dict.ContainsKey(key);


        //public bool Add(string key, object value) => dict.TryAdd(key, value);
        public string Add(string key, object value) {
            var newkey = key;
            int keyCount = 2;
            while (dict.ContainsKey(newkey) || ReservedKeys.Contains(newkey)) {
                if (keyCount == int.MaxValue) return null;
                newkey = $"{key}({keyCount++})";
            }
            dict.Add(newkey, value);
            return newkey;
        }
        public string Add(KeyValuePair<string,object> entry) => Add(entry.Key, entry.Value);


        public bool Set(string key, object value) {
            if (ReservedKeys.Contains(key)) return false;
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
            return true;
        }


        public object Get(string key) {
            if (dict.TryGetValue(key, out var value)) return value; return null;
        }


        public void FillReservedKeys(StdApiResult result) {
            dict.TryAdd("message", result.Message);
            dict["message"] = result.Message;
            dict.TryAdd("content", result.Content);
            dict["content"] = result.Content;
        }


        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => dict.GetEnumerator();
    }
}
