using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace StandardApiTools {

    public partial class StdApiResponse: IProduceStdApiErrorResult {

        public static async Task<StdApiResponse> FromAsync(HttpWebRequest req) {
            try {
                var res = await req.GetResponseAsync();
                return new StdApiResponse(req, res as HttpWebResponse);
            }
            catch (WebException ex) {
                return new StdApiResponse(req, ex);
            }
        }




        public static StdApiResponse From(HttpWebRequest req) {
            try {
                var res = req.GetResponse();
                return new StdApiResponse(req, res as HttpWebResponse);
            }
            catch (WebException ex) {
                return new StdApiResponse(req, ex);
            }
        }




        public WebException Exception { get; }
        public string CommMessage { get; }
        public CommunicationStatus CommStatus { get; }
        public HttpStatusCode? HttpStatus { get; }
        public int StatusCode => HttpStatus.HasValue ? (int)HttpStatus : (int)CommStatus;
        public bool IsSuccess => HttpStatus.HasValue && ((int)HttpStatus) < 300;
        public byte[] ContentAsBytes { get; }
        public string ContentAsString { get => ContentAsBytes?.ToEncodedString(ContentEncoding); }
        public string ContentEncoding { get; }
        public Uri RequestUri { get; }
        public long ContentLength { get; }
        public string ContentType { get; }
        public Version ProtocolVersion { get; }
        public string CharacterSet { get; }
        public Dictionary<string, string> Headers { get; }
        public bool? IsFromCache { get; }
        public DateTime? LastModified { get; }
        public string Method { get; }
        public string Server { get; }




        public StdApiResponse(HttpWebRequest req, HttpWebResponse resp)
        : this(req, resp, CommunicationStatus.Success, "Communication successfully completed") { }




        private StdApiResponse(HttpWebRequest req, WebException ex)
        : this(req, ex.Response, ex.Status.ToCommStatus(), ex.Message) {
            Exception = ex;
        }




        private StdApiResponse(
            HttpWebRequest req, WebResponse resp,
            CommunicationStatus commStatus, string comStatusMessage
            ) {
            CommStatus = commStatus;
            CommMessage = comStatusMessage;
            RequestUri = req.RequestUri;
            if (resp is null) return;
            ContentLength = resp.ContentLength;
            ContentType = resp.ContentType;
            Headers = resp.Headers.AllKeys.ToDictionary(k => k, k => resp.Headers[k]);
            IsFromCache = resp.IsFromCache;
            var hr = resp as HttpWebResponse;
            HttpStatus = TryOrNull(() => hr.StatusCode);
            ProtocolVersion = hr.ProtocolVersion;
            CharacterSet = hr.CharacterSet;
            LastModified = hr.LastModified;
            Method = hr.Method;
            Server = hr.Server;
            ContentEncoding = hr.ContentEncoding;
            //ContentEncoding = hr.ContentEncoding != null ? Encoding.GetEncoding(hr.ContentEncoding) : null;
            //ContentAsString = resp.GetContentAsString();
            ContentAsBytes = resp.GetContentAsBytes();
            resp.Dispose();
        }




        public StdApiResponse(Blueprint b) {
            CommStatus = b.CommStatusCode;
            CommMessage = b.CommMessage;
            RequestUri = b.RequestUri;
            ContentLength = b.ContentLength;
            ContentType = b.ContentType;
            Headers = b.Headers;
            IsFromCache = b.IsFromCache;
            HttpStatus = b.HttpStatusCode;
            ProtocolVersion = b.ProtocolVersion;
            CharacterSet = b.CharacterSet;
            LastModified = b.LastModified;
            Method = b.Method;
            Server = b.Server;
            ContentEncoding = b.ContentEncoding;
            ContentAsBytes = b.ContentBytes;
        }




        private static T? TryOrNull<T>(Func<T> get) where T : struct {
            try { return get(); } catch { return null; }
        }




        public StdApiWebException ToException(CommunicationStatus status) => ToException((int)status);
        public StdApiWebException ToException(HttpStatusCode status) => ToException((int)status);
        public StdApiWebException ToException(int status) {
            if (status == (int)HttpStatus || status == (int)CommStatus)
                return StdApiWebException.From(this);
            else return null;
        }
        public StdApiWebException ToException(params int[] statuses) {
            if (statuses.Contains((int)HttpStatus) || statuses.Contains((int)CommStatus))
                return StdApiWebException.From(this);
            else return null;
        }
        public StdApiWebException ToException() {
            return StdApiWebException.From(this);
        }




        public StdApiException TryDeserializeContent<T>(out T result, JsonSerializerOptions options = null) {
            try {
                options ??= StdApiUtil.DefaultJsonSerializerOptions;
                result = JsonSerializer.Deserialize<T>(ContentAsString, options);
                return null;
            }
            catch (Exception ex) {
                result = default;
                var msg = "Não foi possível desserializar o conteúdo";
                return new StdApiException(HttpStatusCode.Conflict, msg, ex.Message);
            }
        }




        public T DeserializeContent<T>(out StdApiException exception, JsonSerializerOptions options = null) {
            exception = TryDeserializeContent<T>(out var result, options);
            return result;
        }




        public string GetHeaderValue(string key) {
            if (Headers == null) return null;
            foreach (var item in Headers) {
                if (item.Key == key)
                    return item.Value;
                if (item.Value == key)
                    return item.Key;
            }
            return null;
        }




        public StdApiErrorResult ToResult(string message) => new StdApiErrorResult(this, message);
        public StdApiErrorResult ToResult() => new StdApiErrorResult(this);
        StdApiResult IProduceStdApiResult.ToResult() => ToResult();
    }
}
