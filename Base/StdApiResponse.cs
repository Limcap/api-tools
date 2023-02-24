using Microsoft.AspNetCore.JsonPatch.Helpers;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static StandardApiTools.StdApiResponse;
using static StandardApiTools.StdApiWebException.SpecialCase;

namespace StandardApiTools {

    public partial class StdApiResponse : IProduceStdApiResult {

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




        public Exception Exception { get; }
        public string CommMessage { get; }
        public CommunicationStatus CommStatus { get; }
        public HttpStatusCode? HttpStatus { get; }
        public int StatusCode => HttpStatus.HasValue ? (int)HttpStatus : (int)CommStatus; 
        public bool IsSuccess => HttpStatus.HasValue && ((int)HttpStatus) < 300;
        public string ContentAsString { get; }
        public byte[] ContentAsBytes { get => ContentAsString == null ? null : Encoding.UTF8.GetBytes(ContentAsString); }
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
        :this(req, resp, CommunicationStatus.Success, "Communication successfully completed" ){}




        private StdApiResponse(HttpWebRequest req, WebException ex)
        :this(req, ex.Response, ex.Status.ToCommStatus(), ex.Message) {
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
            Headers = resp.Headers.AllKeys.ToDictionary(k => resp.Headers[k]);
            IsFromCache = resp.IsFromCache;
            var hr = resp as HttpWebResponse;
            HttpStatus = TryOrNull(() => hr.StatusCode);
            ProtocolVersion = hr.ProtocolVersion;
            CharacterSet = hr.CharacterSet;
            LastModified = hr.LastModified;
            Method = hr.Method;
            Server = hr.Server;
            ContentEncoding = hr.ContentEncoding;
            ContentAsString = resp.GetContentAsString();
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
            ContentAsString = b.ContentAsString;
        }




        private static T? TryOrNull<T>(Func<T> get) where T : struct {
            try { return get(); } catch { return null; }
        }




        public StdApiWebException ToException(string additionalMessage = null) {
            return StdApiWebException.From(this, additionalMessage);
        }
        //public StdApiWebException ToException(string additionalMessage = null, params KeyValuePair<string,object>[] customData) {
        //    return StdApiWebException.From(this, additionalMessage, customData);
        //}




        public StdApiResult ToResult() {
            return new StdApiResult(this);
        }




        public StdApiResult ToResult(string message) {
            return new StdApiResult(this, message);
        }




        public DesserializationResult<T> TryDeserialize<T>(JsonSerializerOptions options = null) {
            try {
                options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                var res = JsonSerializer.Deserialize<T>(ContentAsString, options);
                return new DesserializationResult<T>(res);
            }
            catch (Exception ex) {
                return new DesserializationResult<T>(ex);
            }
        }
    }
}
