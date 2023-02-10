using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System;
using static StandardResponseTools.ExternalServiceException;
using Newtonsoft.Json;

namespace StandardResponseTools {

    public static class Extensions {

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
        /// Transforma uma <see cref="WebException"/> em uma <see cref="ExternalServiceException"/>,
        /// incluindo opcionais casos especiais.
        /// </summary>
        /// <param name="ex">Objeto fonte</param>
        /// <param name="casos">Casos especiais</param>
        public static ExternalServiceException AsServicoExternoException(this WebException ex, params Action<CustomCase>[] casos) {
            var parsedCasos = CriarCasos(casos);
            return new ExternalServiceException(ex, parsedCasos);

            static CustomCase[] CriarCasos(Action<CustomCase>[] actions) {
                var casos = new List<CustomCase>(actions.Length);
                foreach (var action in actions) {
                    var caso = new CustomCase();
                    action(caso);
                    casos.Add(caso);
                }
                return casos.ToArray();
            }
        }






        /// <summary>
        /// Obtém o conteúdo de uma WebResponse em bytes crus
        /// </summary>
        /// <param name="response">Objeto fonte</param>
        public static byte[] GetContentAsBytes(this WebResponse response) {
            //using var rs = response.GetResponseStream();
            //using var ms = new MemoryStream();
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
            //using var rs = response.GetResponseStream();
            //using var sr = new StreamReader(rs);
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
        public static T Pop<T>(this AggregateException ex) where T : Exception {
            return ex.InnerExceptions[0] is T ex2 ? ex2 : null;
        }






        /// <summary>
        /// Retorna a primeira exceção interna caso o objeto seja uma <see cref="AggregateException"/>
        /// ou então retorna o próprio objeto.
        /// </summary>
        public static Exception Pop(this Exception ex) {
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
            catch(Exception ex) {
                return ex;
            }

        }
    }
}
