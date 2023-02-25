using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StandardApiTools {

    public static class StdApiExtensions {

        /// <summary>
        /// Altera uma <see cref="HttpWebRequest"/> para ignorar o proxy em caso da URI
        /// apontar para localhost.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="uri"></param>
        public static void NoProxyForLocal(this HttpWebRequest req, string uri) {
            req.Proxy = new WebProxy(HttpClient.DefaultProxy.GetProxy(new Uri(uri)), true);
        }




        /// <summary>
        /// Obtém o conteúdo de uma WebResponse em bytes crus
        /// </summary>
        /// <param name="response">Objeto fonte</param>
        public static byte[] GetContentAsBytes(this WebResponse response) {
            var rs = response.GetResponseStream();
            var ms = new MemoryStream();
            rs.CopyTo(ms);
            return ms.ToArray();
        }




        /// <summary>
        /// Retorna o conteúdo de uma WebResponse no formato string.
        /// </summary>
        /// <param name="response">Objeto fonte</param>
        /// <param name="foceEncoding">Força a conversão da stream de bytes para string usando este encoding</param>
        public static string GetContentAsString(this WebResponse response, Encoding foceEncoding = null) {
            if (response == null) return null;
            var encodingStr = (response as HttpWebResponse)?.ContentEncoding;
            var encoding = encodingStr == null ? null : Encoding.GetEncoding(encodingStr);
            encoding = foceEncoding ?? encoding;
            var rs = response?.GetResponseStream();
            StreamReader sr = encoding != null ? new StreamReader(rs, encoding) : new StreamReader(rs, true);
            var data = sr.ReadToEnd();
            return data;
        }




        /// <summary>
        /// Retorna somente a primeira exceção de uma <see cref="AggregateException"/>
        /// caso ela seja do tipo <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Tipo da exceção desejada</typeparam>
        public static T Deaggregate<T>(this AggregateException ex) where T : Exception {
            return ex.InnerExceptions[0] is T ex2 ? ex2 : null;
        }




        /// <summary>
        /// Retorna a primeira exceção interna caso o objeto seja uma <see cref="AggregateException"/>
        /// ou então retorna o próprio objeto.
        /// </summary>
        public static Exception Deaggregate(this Exception ex) {
            return ex is AggregateException agg && agg.InnerExceptions.Count == 1 ? agg.InnerExceptions[0] : ex;
        }




        /// <summary>
        /// Verificar se uma string ou conjunto de characters é uma sequencia hexadecimal.
        /// </summary>
        public static bool IsHex(this IEnumerable<char> chars) {
            foreach (var c in chars) {
                var isHex = ((c == '-') ||
                    (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F')
                );
                return isHex;
            }
            return false;
        }




        public static Exception AddJsonBody(this HttpWebRequest req, object data) {
            try {
                using (var sw = new StreamWriter(req.GetRequestStream())) {
                    string json = JsonConvert.SerializeObject(data);
                    sw.Write(json);
                }
                return null;
            }
            catch (Exception ex) {
                return ex;
            }
        }




        public static string TrimToNull(this string str) {
            str = str?.Trim();
            return string.IsNullOrEmpty(str) ? null : str;
        }




        public static string Join(this string before, string after) {
            return Join(before, Environment.NewLine, after);
        }




        public static string Join(this string before, string separator, string after) {
            if (before == null) return after;
            if (after == null) return before;
            return before + separator + after;
        }




        public static Task<StdApiResponse> GetStdApiResponseAsync(this HttpWebRequest req) {
            return StdApiResponse.FromAsync(req);
        }




        public static StdApiResponse GetStdApiResponse(this HttpWebRequest req) {
            return StdApiResponse.From(req);
        }




        //public static T ApplySpecialCase<T>(this Func<T> func, StdApiWebException.SpecialCase[] cases) {
        //    return StdApiWebException.Handle(func, cases);
        //}




        public static StdApiResponse.CommunicationStatus ToCommStatus(this WebExceptionStatus wss) {
            return (StdApiResponse.CommunicationStatus)(int)wss;
        }




        public static int? ToDigit(this char c) {
            if (char.IsDigit(c)) return (c - '0');
            return null;
        }




        public static string AddConcat(this Dictionary<string, object> dict, string key, object value) {
            var newkey = key;
            if (dict.ContainsKey(newkey)) {
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




        public static string AddRename<T>(this Dictionary<string, T> dict, string key, T value) {
            var newkey = RenameKey(key, dict);
            dict.Add(newkey, value);
            return newkey;

            static string RenameKey(string key, Dictionary<string, T> dict) {
                var newkey = key;
                int keyCount = 2;
                //while (dict.ContainsKey(newkey) || ReservedKeys.Contains(newkey)) {
                while (dict.ContainsKey(newkey)) {
                    if (keyCount == int.MaxValue) { key += "_"; keyCount = 1; }
                    newkey = $"{key}({keyCount++})";
                }
                return newkey;
            }
        }




        public static void Set<K, V>(this Dictionary<K, V> dict, K key, V value) {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }
    }
}
