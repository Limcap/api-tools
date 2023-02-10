using System.IO;
using System.Net;
using System.Text;
using System;
using RestSharp;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace StandardResponseTools {
    public struct CachedResponse {
        //public string MethodName { get; }
        public object CommStatusSource { get; }
        public int CommStatusCode { get; }
        public string CommMessage { get; }
        public HttpStatusCode? HttpStatusCode { get; }
        public bool IsSuccess => HttpStatusCode.HasValue && ((int)HttpStatusCode) < 200;
        //public string ContentAsString { get => Encoding.GetEncoding(ContentEncoding).GetString(ContentAsBytes); }
        //public byte[] ContentAsBytes { get; }
        public string ContentAsString { get; }
        public byte[] ContentAsBytes { get => ContentAsString == null ? null : Encoding.UTF8.GetBytes(ContentAsString); }
        public string ContentEncoding { get; }
        public string Uri { get; }
        public long ContentLength { get; }
        public string ContentType { get; }
        public Version ProtocolVersion { get; }
        public string CharacterSet { get; }
        //public WebHeaderCollection Headers { get; }
        public Dictionary<string, string> Headers { get; }
        public bool? IsFromCache { get; }
        public DateTime? LastModified { get; }
        public string Method { get; }
        public string Server { get; }
        private HttpWebResponse r;





        public CachedResponse(WebResponse response) {
            CommStatusCode = -1;
            CommStatusSource = null;
            CommMessage = "Erro de comunicação não especificado: objeto criado a partir da WebResponse sem um WebException.";
            ContentLength = response?.ContentLength ?? 0;
            ContentType = response?.ContentType ?? null;
            Headers = response?.Headers.AllKeys.ToDictionary(k => response.Headers[k]);
            IsFromCache = response?.IsFromCache;
            var r = response as HttpWebResponse;
            this.r = r;
            HttpStatusCode = tryget(() => r.StatusCode);
            ProtocolVersion = r?.ProtocolVersion;
            CharacterSet = r?.CharacterSet;
            LastModified = r?.LastModified;
            Method = r?.Method;
            Server = r?.Server;
            HttpStatusCode = r?.StatusCode;
            ContentEncoding = r?.ContentEncoding;
            try {
                Uri = r?.ResponseUri.AbsoluteUri ?? "(URI não informada)";
                ContentAsString = r.GetContentAsString();
                //ContentAsBytes = r.GetContentAsBytes();
            }
            catch (Exception ex) {
                Uri = ex.TargetSite.Name; //"[erro]";
                ContentAsString = null;
            }
            response?.Dispose();

            T? tryget<T>(Func<T> get) where T : struct {
                try { return get(); } catch { return null; }
            }
        }






        public CachedResponse(WebException ex) {
            var resp = ex.Response;
            CommStatusCode = (int)ex.Status;
            CommStatusSource = ex.Status;
            CommMessage = ex.Message;
            ContentLength = resp?.ContentLength ?? 0;
            ContentType = resp?.ContentType ?? null;
            Headers = resp?.Headers.AllKeys.ToDictionary(k => resp.Headers[k]);
            IsFromCache = resp?.IsFromCache;
            r = resp as HttpWebResponse;
            HttpStatusCode = r?.StatusCode;
            ProtocolVersion = r?.ProtocolVersion;
            CharacterSet = r?.CharacterSet;
            LastModified = r?.LastModified;
            Method = r?.Method;
            Server = r?.Server;
            ContentEncoding = r?.ContentEncoding;
            try {
                Uri = r?.ResponseUri?.AbsoluteUri ?? "(URI não informada)";
                ContentAsString = r?.GetContentAsString();
                //ContentAsBytes = r.GetContentAsBytes();
            }
            catch {
                Uri = "[erro]";
                ContentAsString = null;
            }
            resp?.Dispose();
        }






        public CachedResponse(IRestResponse r) {
            this.r = null;
            CommStatusCode = (int)r.ResponseStatus;
            CommStatusSource = r.ResponseStatus;
            CommMessage = r.ErrorMessage;
            CharacterSet = null;
            ContentAsString = r.Content;
            ContentEncoding = r.ContentEncoding;
            ContentType = r.ContentType;
            Headers = r.Headers.ToDictionary(p => p.Name, p => p.Value.ToString());
            Method = r.Request.Method.ToString();
            ProtocolVersion = r.ProtocolVersion;
            Server = r.Server;
            Uri = r.ResponseUri?.AbsoluteUri ?? "(URI não informada)";
            LastModified = null;
            IsFromCache = false;
            ContentLength = r.ContentLength;
            HttpStatusCode = r.StatusCode;
        }






        private static string GetResponseMessage(HttpWebResponse response) {
            if (response == null) return null;
            try {
                using var descrption = response.GetResponseStream();
                using var readStream = new StreamReader(descrption);
                var responseContents = readStream.ReadToEnd();
                return responseContents;
            }
            catch {
                return null;
            }
        }


        public enum CommStatus {
            Success = 0,
            Error = 2,
            HttpError = 7,
            TimedOut = 14
        }
    }
}
