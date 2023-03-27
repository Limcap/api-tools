using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace StandardApiTools {
    public static class WebResponseExtensions {
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




        public static Exception AddJsonContent(this HttpWebRequest req, object data, Encoding encoding = null) {
            if (data is string s) return AddStringContent(req, s, encoding);
            try {
                encoding = encoding ?? Encoding.Default;
                JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var str = JsonConvert.SerializeObject(data, settings);
                return AddContent(req, "application/json", str, encoding);
            }
            catch (Exception ex) {
                //return ex;
                return StdApiException.CreateFrom(ex, "Ocorreu um erro ao adicionar o conteúdo da requisição");
            }
        }




        public static Exception AddStringContent(this HttpWebRequest req, string str, Encoding encoding = null) {
            return AddContent(req, "text/plain", str, encoding);
        }




        public static Exception AddContent(this HttpWebRequest req, string contentType, string str, Encoding encoding = null) {
            try {
                encoding = encoding ?? Encoding.Default;
                if (encoding == Encoding.Default) {
                    req.ContentType = contentType;
                    using (var sw = new StreamWriter(req.GetRequestStream())) sw.Write(str);
                }
                else {
                    req.ContentType = "contentType;charset=" + encoding.WebName;
                    var bytes = encoding.GetBytes(str);
                    req.ContentLength = bytes.Length;
                    using (var s = req.GetRequestStream()) s.Write(bytes, 0, bytes.Length);
                }
                return null;
            }
            catch (Exception ex) {
                //return ex;
                return StdApiException.CreateFrom(ex, "Ocorreu um erro ao adicionar o conteúdo da requisição");
            }
        }
    }
}
