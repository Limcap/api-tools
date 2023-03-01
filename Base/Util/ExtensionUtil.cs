using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StandardApiTools
{
    public static class StdApiUtil
    {

        public static string ToEncodedString(this byte[] bytes, string encoding = null)
        {
            Encoding enc = null;
            if (encoding != null)
            {
                try
                {
                    enc = int.TryParse(encoding, out var codepage)
                    ? Encoding.GetEncoding(codepage)
                    : Encoding.GetEncoding(encoding);
                }
                catch { }
            }
            return bytes.ToEncodedString(enc);
        }




        public static string ToEncodedString(this byte[] bytes, Encoding encoding = null)
        {
            var rs = new MemoryStream(bytes);
            StreamReader sr = encoding != null ? new StreamReader(rs, encoding) : new StreamReader(rs, true);
            return sr.ReadToEnd();
        }




        /// <summary>
        /// Retorna somente a primeira exceção de uma <see cref="AggregateException"/>
        /// caso ela seja do tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo da exceção desejada</typeparam>
        public static T Deaggregate<T>(this AggregateException ex) where T : Exception
        {
            return ex.InnerExceptions[0] is T ex2 ? ex2 : null;
        }




        /// <summary>
        /// Retorna a primeira exceção interna caso o objeto seja uma <see cref="AggregateException"/>
        /// ou então retorna o próprio objeto.
        /// </summary>
        public static Exception Deaggregate(this Exception ex)
        {
            return ex is AggregateException agg && agg.InnerExceptions.Count == 1 ? agg.InnerExceptions[0] : ex;
        }




        /// <summary>
        /// Verificar se uma string ou conjunto de characters é uma sequencia hexadecimal.
        /// </summary>
        public static bool IsHex(this IEnumerable<char> chars)
        {
            foreach (var c in chars)
            {
                var isHex = c == '-' ||
                    c >= '0' && c <= '9' ||
                    c >= 'a' && c <= 'f' ||
                    c >= 'A' && c <= 'F'
                ;
                return isHex;
            }
            return false;
        }




        public static string TrimToNull(this string str)
        {
            str = str?.Trim();
            return string.IsNullOrEmpty(str) ? null : str;
        }




        public static string Join(this string before, string after)
        {
            return before.Join(Environment.NewLine, after);
        }




        public static string Join(this string before, string separator, string after)
        {
            if (before == null) return after;
            if (after == null) return before;
            return before + separator + after;
        }




        public static int? ToDigit(this char c)
        {
            if (char.IsDigit(c)) return c - '0';
            return null;
        }




        public static string AddConcat(this Dictionary<string, object> dict, string key, object value)
        {
            var newkey = key;
            if (dict.ContainsKey(newkey))
            {
                var currentValue = dict[newkey];
                List<object> list;
                if (currentValue is List<object> currentList)
                {
                    list = currentList;
                    list.Add(value);
                }
                else
                {
                    list = new List<object>();
                    list.Add(currentValue);
                    list.Add(value);
                    dict[newkey] = list;
                }
            }
            else dict.Add(newkey, value);
            return newkey;
        }




        public static string AddRename<T>(this Dictionary<string, T> dict, string key, T value)
        {
            var newkey = RenameKey(key, dict);
            dict.Add(newkey, value);
            return newkey;

            static string RenameKey(string key, Dictionary<string, T> dict)
            {
                var newkey = key;
                int keyCount = 2;
                while (dict.ContainsKey(newkey))
                {
                    if (keyCount == int.MaxValue) { key += "_"; keyCount = 1; }
                    newkey = $"{key}({keyCount++})";
                }
                return newkey;
            }
        }




        public static void Set<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }
    }
}
