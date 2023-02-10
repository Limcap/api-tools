using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace StandardApiTools {
    public class StdApiResponse {

        public static StdApiResponse FromExternalService(HttpWebRequest req) {
            try {
                var res = req.GetResponse();
                return new StdApiResponse(req, res as HttpWebResponse);
            }
            catch (WebException ex) {
                return new StdApiResponse(req, ex);
            }
        }






        public static StdApiResponse FromExternalService(IRestRequest req, IRestClient client) {
            var resp = client.Execute(req);
            return new StdApiResponse(client, resp);
        }






        public static StdApiResponse FromExternalService<T>(IRestClient client, IRestRequest req) {
            var resp = client.Execute(req);
            return new StdApiResponse(client, resp);
        }





        public readonly Exception Exception;
        public readonly object CommStatusSource;
        public readonly int CommStatusCode;
        public readonly string CommMessage;
        public readonly HttpStatusCode? HttpStatusCode;
        public bool IsSuccess => HttpStatusCode.HasValue && ((int)HttpStatusCode) < 300;
        public readonly string ContentAsString;
        public byte[] ContentAsBytes { get => ContentAsString == null ? null : Encoding.UTF8.GetBytes(ContentAsString); }
        public readonly string ContentEncoding;
        public readonly Uri RequestUri;
        public readonly long ContentLength;
        public readonly string ContentType;
        public readonly Version ProtocolVersion;
        public readonly string CharacterSet;
        public readonly Dictionary<string, string> Headers;
        public readonly bool? IsFromCache;
        public readonly DateTime? LastModified;
        public readonly string Method;
        public readonly string Server;






        public StdApiResponse(HttpWebRequest req, HttpWebResponse resp)
        :this(req, resp, 0, null, "Sucess" ){}






        private StdApiResponse(HttpWebRequest req, WebException ex)
        :this(req, ex.Response, (int)ex.Status, ex.Status, ex.Message) {
            Exception = ex;
        }





        private StdApiResponse(
            HttpWebRequest req, WebResponse resp,
            int comStatusCode, object comStatusSource, string comStatusMessage
            ) {
            var CommStatusCode = comStatusCode;
            var CommStatusSource = comStatusSource;
            var CommMessage = comStatusMessage;
            RequestUri = req.RequestUri;
            if (resp is null) return;
            ContentLength = resp.ContentLength;
            ContentType = resp.ContentType;
            Headers = resp.Headers.AllKeys.ToDictionary(k => resp.Headers[k]);
            IsFromCache = resp.IsFromCache;
            var hr = resp as HttpWebResponse;
            HttpStatusCode = TryOrNull(() => hr.StatusCode);
            ProtocolVersion = hr.ProtocolVersion;
            CharacterSet = hr.CharacterSet;
            LastModified = hr.LastModified;
            Method = hr.Method;
            Server = hr.Server;
            HttpStatusCode = hr.StatusCode;
            ContentEncoding = hr.ContentEncoding;
            ContentAsString = GetContentAsString(resp);
            resp.Dispose();
        }






        private StdApiResponse(IRestClient client, IRestResponse resp) {
            Exception = resp.ErrorException;
            CommStatusCode = (int)resp.ResponseStatus;
            CommStatusSource = resp.ResponseStatus;
            CommMessage = resp.ErrorMessage;
            CharacterSet = null;
            ContentAsString = resp.Content;
            ContentEncoding = resp.ContentEncoding;
            ContentType = resp.ContentType;
            Headers = resp.Headers.ToDictionary(p => p.Name, p => p.Value.ToString());
            Method = resp.Request.Method.ToString();
            ProtocolVersion = resp.ProtocolVersion;
            Server = resp.Server;
            RequestUri = client.BaseUrl;
            LastModified = null;
            IsFromCache = false;
            ContentLength = resp.ContentLength;
            HttpStatusCode = resp.StatusCode;
        }






        /// <summary>
        /// Retorna o conteúdo de uma WebResponse no formato string.
        /// </summary>
        /// <param name="response">Objeto fonte</param>
        /// <param name="foceEncoding">Força a conversão da stream de bytes para string usando este encoding</param>
        public string GetContentAsString(WebResponse response, Encoding foceEncoding = null) {
            if (response == null) return null;
            var encodingStr = (response as HttpWebResponse)?.ContentEncoding;
            var encoding = encodingStr == null ? null : Encoding.GetEncoding(encodingStr);
            encoding = foceEncoding ?? encoding;
            var rs = response?.GetResponseStream();
            StreamReader sr = encoding != null ? new StreamReader(rs, encoding) : new StreamReader(rs, true);
            var data = sr.ReadToEnd();
            return data;
        }






        private static T? TryOrNull<T>(Func<T> get) where T : struct {
            try { return get(); } catch { return null; }
        }






        public StdApiResponse ThrowOnError(params StdApiDependencyException.SpecialCase[] customCases) {
            if (IsSuccess) return this;
            var ex = StdApiDependencyException.From(this);
            if (ex == null) return this;
            throw ex;
        }
    }
}
