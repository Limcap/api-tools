using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace StandardApiTools {

    public class StdApiDataCollection: IEnumerable<KeyValuePair<string, object>> { 

        public StdApiDataCollection(IDictionary<string, object> source = null) {
            dict = source ?? new Dictionary<string, object>();
        }

        public StdApiDataCollection(params KeyValuePair<string, object>[] items) {
            dict = new Dictionary<string, object>();
            foreach(var item in items) dict.Add(item);
        }

        public StdApiDataCollection(params (string key, object value)[] items) {
            dict = new Dictionary<string, object>();
            foreach (var item in items) Add(item.key, item.value);
        }


        private readonly IDictionary<string, object> dict = new Dictionary<string, object>();


        public int Count => dict.Count;


        public ICollection<string> Keys => dict.Keys;


        public void Clear() => dict.Clear();


        public bool ContainsKey(string key) => dict.ContainsKey(key);




        public void Add(KeyValuePair<string, object> entry) => Add(entry.Key, entry.Value);
        public void Add(string key, object value) {
            if (dict.ContainsKey(key)) {
                _GetOrSetList(key).Add(value);
            }
            else {
                dict.Add(key, value);
            }
        }




        private List<object> _GetOrSetList(string key) {
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




        public void Set(string key, object value) {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }




        public object Get(string key) {
            if (dict.TryGetValue(key, out var value)) return value; return null;
        }




        public bool Del(string key) {
            if (!dict.ContainsKey(key)) return false;
            dict.Remove(key);
            return true;
        }




        public string ToJson() {
            var opt = new JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true };
            var serialized = JsonSerializer.Serialize(dict, opt);
            return serialized;
        }




        public object ToObject(bool ignoreNullValues = false) {
            var items = ignoreNullValues
                ? dict.Where(i => i.Value != null).ToList()
                : dict.ToList();
            if (items.Count == 0) return null;
            var eo = new ExpandoObject();
            var eoc = (ICollection<KeyValuePair<string, object>>)eo;
            foreach (var item in items) eoc.Add(item);
            return eo;
        }




        public static implicit operator StdApiDataCollection(Dictionary<string, object> source) {
            return new StdApiDataCollection(source);
        }




        IEnumerator IEnumerable.GetEnumerator() => dict.GetEnumerator();
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => dict.GetEnumerator();
    }
}
