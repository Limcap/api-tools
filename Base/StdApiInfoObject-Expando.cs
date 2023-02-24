using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace StandardApiTools {

    public class StdApiInfoObject: IEnumerable<KeyValuePair<string, object>> {

        public StdApiInfoObject(IDictionary<string,object> source = null) {
            eo = new ExpandoObject();
            items = eo; 
        }


        private static string[] ReservedKeys = new string[] { "message", "content" };


        private readonly ExpandoObject eo;
        private ICollection<KeyValuePair<string, object>> items;


        public int Count => items.Count;


        //public Dictionary<string, object>.KeyCollection Keys => dict.Keys;
        public IEnumerable<string> Keys => items.Select(i => i.Key);


        public void Clear() => items.Clear();


        public bool ContainsKey(string key) => Keys.Contains(key);


        public string AddAutoConcat(KeyValuePair<string, object> entry) => AddAutoConcat(entry.Key, entry.Value);
        public string AddAutoConcat(string key, object value) {
            var newkey = key;
            int keyCount = 1;
            while (ReservedKeys.Contains(newkey)) {
                keyCount++;
                key = key += $"_";
                newkey = $"{key}({keyCount++})";
            }
            if (ContainsKey(newkey)) {
                var currentValue = dict[newkey];
                List<object> list;
                if (currentValue is List<object> currentList) {
                    list = currentList;
                    list.Add(value);
                }
                else {
                    list = new List<object>();
                    list.Add(currentValue);
                    list.Add(value);
                    dict[newkey] = list;
                }
            }
            else dict.Add(newkey, value);
            return newkey;
        }




        public string AddAutoRename(KeyValuePair<string,object> entry) => AddAutoRename(entry.Key, entry.Value);
        public string AddAutoRename(string key, object value) {
            var newkey = key;
            int keyCount = 2;
            while (dict.ContainsKey(newkey) || ReservedKeys.Contains(newkey)) {
                if (keyCount == int.MaxValue) { key += "_"; keyCount = 1; }
                newkey = $"{key}({keyCount++})";
            }
            dict.Add(newkey, value);
            return newkey;
        }
 



        public bool Set(string key, object value) {
            if (ReservedKeys.Contains(key)) return false;
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
            return true;
        }




        public object Get(string key) {
            if (items.TryGetValue(key, out var value)) return value; return null;
        }




        public void FillReservedKeys(StdApiResult result) {
            items.TryAdd("message", result.Message);
            items["message"] = result.Message;
            dict.TryAdd("content", result.Content);
            dict["content"] = result.Content;
        }




        public string ToJson() {
            var opt = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            var serialized = JsonSerializer.Serialize(items, opt);
            return serialized;
        }



        public object ToObject(bool ignoreNullValues = false) {
            var eo = new ExpandoObject();
            var eoc = (ICollection<KeyValuePair<string, object>>)eo;
            foreach (var item in items) if(!ignoreNullValues || item.Value != null) eoc.Add(item);
            return eo;
            //var opt = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            //var json = ToJson();
            //var obj = JsonSerializer.Deserialize<object>(json, opt);
            //return obj;
        }




        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => items.GetEnumerator();
    }
}
