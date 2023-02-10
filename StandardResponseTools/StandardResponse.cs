using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace StandardResponseTools {
    public class StandardResponse {

        public static StandardResponse FromExternalService(HttpWebRequest req) {
            try {
                var res = req.GetResponse();
                return new StandardResponse(req, res as HttpWebResponse);
            }
            catch (WebException ex) {
                return new StandardResponse(req, ex);
            }
        }






        public static StandardResponse FromExternalService(IRestRequest req, IRestClient client) {
            var resp = client.Execute(req);
            return new StandardResponse(client, resp);
        }






        public static StandardResponse FromExternalService<T>(IRestClient client, IRestRequest req) {
            var resp = client.Execute(req);
            return new StandardResponse(client, resp);
        }






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






        public StandardResponse(HttpWebRequest req, HttpWebResponse resp)
        :this(req, resp, 0, null, "Sucess" ){}






        private StandardResponse(HttpWebRequest req, WebException ex)
        :this(req, ex.Response, (int)ex.Status, ex.Status, ex.Message) {}





        private StandardResponse(
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






        private StandardResponse(IRestClient client, IRestResponse resp) {
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






        public StandardResponse ThrowOnError(params SpecialCase[] customCases) {
            if (IsSuccess) return this;
            var r = GetErrorResult(customCases);
            if (r == null) return this;
            throw new SRException(r.Status, r.Message, r.Data);
        }






        private SRResult GetErrorResult(params SpecialCase[] cases) {
            if (this.IsSuccess) return null;
            int status = this.HttpStatusCode == null ? (int)this.HttpStatusCode : this.CommStatusCode;
            string description = this.HttpStatusCode == null ? this.HttpStatusCode.ToString() : this.CommStatusSource?.ToString();
            SpecialCase? c = FindCase(cases);
            string message = c != null ? c?.Message?.Invoke(this) : ExternalErrorMessage;
            object details = c != null ? c?.Details?.Invoke(this) : new {
                Status = status,
                Description = description,
                Message = this.CommMessage,
                Data = this.ContentAsString,
                Uri = this.RequestUri
            };
            return new SRResult(status, message, details);
        }






        private SpecialCase? FindCase(SpecialCase[] casos) {
            if (casos.Length == 0) return null;
            foreach (var caso in casos) {
                var isMatch = caso.Status == this.CommStatusCode || caso.Status == (int?)this.HttpStatusCode;
                var isConditionSatisfied = caso.Condition?.Invoke(this) ?? true;
                if (isMatch && isConditionSatisfied) return caso;
            }
            return null;
        }






        public struct SpecialCase {
            public int Status;
            public Func<StandardResponse, bool> Condition;
            public Func<StandardResponse, string> Message;
            public Func<StandardResponse, object> Details;
        }






        const string ExternalErrorMessage = "A chamada para um serviço externo falhou.";
    }
}
