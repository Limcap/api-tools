using System.IO;
using System.Net;
using System.Text;
using System;
using RestSharp;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace StandardResponseTools {
    public struct CachedResponse {

        private HttpWebResponse r;
        public HttpStatusCode? Status { get; }
        //public string ContentAsString { get => Encoding.GetEncoding(ContentEncoding).GetString(ContentAsBytes); }
        //public byte[] ContentAsBytes { get; }
        public string ContentAsString { get; }
        public byte[] ContentAsBytes { get => Encoding.UTF8.GetBytes(ContentAsString); }
        public string ContentEncoding { get; }
        public string Uri { get; }
        public long ContentLength { get; }
        public string ContentType { get; }
        public Version ProtocolVersion { get; }
        public string CharacterSet { get; }
        //public WebHeaderCollection Headers { get; }
        public Dictionary<string, string> Headers { get; }
        public bool IsFromCache { get; }
        public DateTime? LastModified { get; }
        public string Method { get; }
        public string Server { get; }






        public CachedResponse(WebResponse response) {
            ContentLength = response.ContentLength;
            ContentType = response.ContentType;
            Headers = response.Headers.AllKeys.ToDictionary(k => response.Headers[k]);
            IsFromCache = response.IsFromCache;
            r = response as HttpWebResponse;
            ProtocolVersion = r?.ProtocolVersion;
            CharacterSet = r?.CharacterSet;
            LastModified = r?.LastModified;
            Method = r?.Method;
            Server = r?.Server;
            Status = r?.StatusCode;
            ContentEncoding = r?.ContentEncoding;
            try {
                Uri = r?.ResponseUri.AbsoluteUri ?? "(URI não informada)";
                ContentAsString = r.GetContentAsString();
                //ContentAsBytes = r.GetContentAsBytes();
            }
            catch (Exception ex) {
                Uri = "[erro]";
                ContentAsString = null;
            }
            response.Dispose();
        }





        public CachedResponse(IRestResponse r) {
            this.r = null;
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
            Status = r.StatusCode;
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
    }
}
