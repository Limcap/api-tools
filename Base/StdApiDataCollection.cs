using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace StandardApiTools {

    public interface IStdApiDataCollection {
        int Count { get; }
        ICollection<string> Keys { get; }

        void Add(KeyValuePair<string, object> entry);
        void Add(string key, object value);
        void Clear();
        bool ContainsKey(string key);
        object Get(string key);
        void Set(string key, object value);
        string ToJson();
        object ToObject(bool ignoreNullValues = false);
    }

    public class StdApiDataCollection: IStdApiDataCollection, IEnumerable<KeyValuePair<string, object>> { 

        public StdApiDataCollection(IDictionary<string, object> source = null) {
            dict = source ?? new Dictionary<string, object>();
        }


        private readonly IDictionary<string, object> dict = new Dictionary<string, object>();


        public int Count => dict.Count;


        public ICollection<string> Keys => dict.Keys;


        public void Clear() => dict.Clear();


        public bool ContainsKey(string key) => dict.ContainsKey(key);




        public void Add(KeyValuePair<string, object> entry) => Add(entry.Key, entry.Value);
        public void Add(string key, object value) {
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




        public void Set(string key, object value) {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }




        public object Get(string key) {
            if (dict.TryGetValue(key, out var value)) return value; return null;
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
