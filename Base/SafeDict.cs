using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace StandardApiTools {
    public class AutoDict: IEnumerable<KeyValuePair<string, object>> {

        public AutoDict(IDictionary<string, object> source = null) {
            dict = source ?? new Dictionary<string, object>();
        }


        private readonly IDictionary<string, object> dict = new Dictionary<string, object>();


        public int Count => dict.Count;


        public ICollection<string> Keys => dict.Keys;


        public void Clear() => dict.Clear();


        public bool ContainsKey(string key) => dict.ContainsKey(key);




        public void AddAutoConcat(KeyValuePair<string, object> entry) => AddAutoConcat(entry.Key, entry.Value);
        public void AddAutoConcat(string key, object value) {
            if (dict.ContainsKey(key)) {
                GetOrCreateList(key).Add(value);
            }
            else {
                dict.Add(key, value);
            }
        }




        private List<object> GetOrCreateList(string key) {
            var currentValue = dict[key];
            List<object> list;
            if (currentValue is List<object> currentList) {
                list = currentList;
            }
            else {
                list = new List<object>();
                list.Add(currentValue);
                dict[key] = list;
            }
            return list;
        }




        public string AddAutoRename(KeyValuePair<string, object> entry) => AddAutoRename(entry.Key, entry.Value);
        public string AddAutoRename(string key, object value) {
            var newkey = RenameKey(key);
            dict.Add(newkey, value);
            return newkey;
        }




        private string RenameKey(string key) {
            var newkey = key;
            int keyCount = 2;
            //while (dict.ContainsKey(newkey) || ReservedKeys.Contains(newkey)) {
            while (dict.ContainsKey(newkey)) {
                if (keyCount == int.MaxValue) { key += "_"; keyCount = 1; }
                newkey = $"{key}({keyCount++})";
            }
            return newkey;
        }




        public void Set(string key, object value) {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }




        public object Get(string key) {
            if (dict.TryGetValue(key, out var value)) return value; return null;
        }




        public void FillReservedKeys(StdApiErrorResult result) {
            dict.TryAdd("message", result.Message);
            dict["message"] = result.Message;
            dict.TryAdd("content", result.Content);
            dict["content"] = result.Content;
        }




        public string ToJson() {
            var opt = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            var serialized = JsonSerializer.Serialize(dict, opt);
            return serialized;
        }




        public object ToObject(bool ignoreNullValues = false) {
            var eo = new ExpandoObject();
            var eoc = (ICollection<KeyValuePair<string, object>>)eo;
            foreach (var item in dict) if (!ignoreNullValues || item.Value != null) eoc.Add(item);
            return eo;
        }




        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => dict.GetEnumerator();
    }
}
