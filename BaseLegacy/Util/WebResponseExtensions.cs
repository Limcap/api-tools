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
            req.Proxy = new WebProxy(WebRequest.DefaultWebProxy.GetProxy(new Uri(uri)), true);
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




        public static Exception AddJsonBody(this HttpWebRequest req, object data) {
            try {
                using (var sw = new StreamWriter(req.GetRequestStream())) {
                    var opt = new JsonSerializerSettings {
                        NullValueHandling = NullValueHandling.Ignore,
                    };
                    string json = JsonConvert.SerializeObject(data,opt);
                    sw.Write(json);
                }
                return null;
            }
            catch (Exception ex) {
                return ex;
            }
        }
    }
}
