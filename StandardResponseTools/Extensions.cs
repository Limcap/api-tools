using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System;
using static StandardResponseTools.ExternalServiceException;
using System.Runtime.CompilerServices;

namespace StandardResponseTools {

    public static class Extensions {

        public static void NoProxyForLocal(this HttpWebRequest req, string uri) {
            req.Proxy = new WebProxy(HttpClient.DefaultProxy.GetProxy(new Uri(uri)), true);
        }






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






        public static byte[] GetContentAsBytes(this WebResponse response) {
            //using var rs = response.GetResponseStream();
            //using var ms = new MemoryStream();
            var rs = response.GetResponseStream();
            var ms = new MemoryStream();
            rs.CopyTo(ms);
            return ms.ToArray();
        }






        public static string GetContentAsString(this WebResponse response, Encoding focerEncoding = null) {
            //using var rs = response.GetResponseStream();
            //using var sr = new StreamReader(rs);
            var encodingStr = (response as HttpWebResponse)?.ContentEncoding;
            var encoding = encodingStr == null ? null : Encoding.GetEncoding(encodingStr);
            encoding = focerEncoding ?? encoding;
            var rs = response.GetResponseStream();
            StreamReader sr = encoding != null ? new StreamReader(rs, encoding) : new StreamReader(rs, true);
            var data = sr.ReadToEnd();
            return data;
        }






        public static T Pop<T>(this AggregateException ex) where T : Exception {
            return ex.InnerExceptions[0] is T ex2 ? ex2 : null;
        }
        public static Exception Pop(this Exception ex) {
            return ex is AggregateException agg && agg.InnerExceptions.Count == 1 ? agg.InnerExceptions[0] : ex;
        }
    }
}
