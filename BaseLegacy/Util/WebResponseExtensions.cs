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
        /// <param name="charset">Converte o stream de bytes para string usando este charset</param>
        public static string GetContentAsString(this WebResponse response, Encoding charset = null) {
            try {
                if (response == null) return null;
                if(charset == null) {
                    try {
                        var encodingStr = (response as HttpWebResponse)?.CharacterSet;
                        charset = string.IsNullOrEmpty(encodingStr) ? null : Encoding.GetEncoding(encodingStr);
                    }
                    catch (Exception ex) {}
                }
                var stream = response?.GetResponseStream();
                if (stream.Position > 0) stream.Position = 0;
                StreamReader reader = charset != null ? new StreamReader(stream, charset) : new StreamReader(stream, true);
                var data = reader.ReadToEnd();
                return data;
            }
            catch (Exception ex) {
                return "Error while reading the data stream from the response";
            }
        }




        public static StdApiException AddJsonContent(this HttpWebRequest req, object data, Encoding charset = null) {
            if (data is string s) {
                return AddContent(req, "application/json", s, charset);
            }
            try {
                JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var str = JsonConvert.SerializeObject(data, settings);
                return AddContent(req, "application/json", str, charset);
            }
            catch (StdApiException) { throw; }
            catch (Exception ex) {
                return StdApiException.CreateFrom(ex, "Ocorreu um erro ao adicionar o conteúdo da requisição");
            }
        }




        public static StdApiException AddStringContent(this HttpWebRequest req, string str, Encoding charset = null) {
            return AddContent(req, "text/plain", str, charset);
        }




        public static StdApiException AddContent(this HttpWebRequest req, string contentType, string str, Encoding charset = null) {
            try {
                charset = charset ?? Encoding.Default;
                if (charset == Encoding.Default) {
                    req.ContentType = contentType;
                    using (var sw = new StreamWriter(req.GetRequestStream())) sw.Write(str);
                }
                else {
                    req.ContentType = $"{contentType}; charset=" + charset.WebName;
                    var bytes = charset.GetBytes(str);
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




        public static StdApiException AddContent(this HttpWebRequest req, string contentType, byte[] bytes, Encoding charset = null) {
            try {
                req.ContentType = charset == null ? contentType : $"{contentType}; charset=" + charset.WebName;
                using (var s = req.GetRequestStream()) s.Write(bytes, 0, bytes.Length);
                return null;
            }
            catch (Exception ex) {
                return StdApiException.CreateFrom(ex, "Ocorreu um erro ao adicionar o conteúdo da requisição");
            }
        }
    }
}
