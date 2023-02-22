﻿using Microsoft.AspNetCore.JsonPatch.Helpers;
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

    public partial class StdApiResponse {

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
        //public object CommStatusSource { get; }
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




        /*
        /// <summary>
        /// Retorna o conteúdo de uma WebResponse no formato string.
        /// </summary>
        /// <param name="response">Objeto fonte</param>
        /// <param name="foceEncoding">Força a conversão da stream de bytes para string usando este encoding</param>
        private string GetContentAsString(WebResponse response, Encoding foceEncoding = null) {
            if (response == null) return null;
            var encodingStr = (response as HttpWebResponse)?.ContentEncoding;
            var encoding = encodingStr == null ? null : Encoding.GetEncoding(encodingStr);
            encoding = foceEncoding ?? encoding;
            var rs = response?.GetResponseStream();
            StreamReader sr = encoding != null ? new StreamReader(rs, encoding) : new StreamReader(rs, true);
            var data = sr.ReadToEnd();
            return data;
        }
        */




        //public C ContentAsObject<C>() where C : class {
        //    try { return System.Text.Json.JsonSerializer.Deserialize<C>(ContentAsString); }
        //    catch { return null; }
        //}
        //public S? ContentAsStruct<S>() where S : struct {
        //    try { return System.Text.Json.JsonSerializer.Deserialize<S>(ContentAsString); }
        //    catch { return null; }
        //}
        /// <summary>
        /// Retorna o conteúdo da resposta como o struct <see cref="S"/>
        /// </summary>
        /// <typeparam name="C">Nome do tipo do objeto</typeparam>
        public C ContentAs<C>() where C : class {
            try { return System.Text.Json.JsonSerializer.Deserialize<C>(ContentAsString); }
            catch { return null; }
        }




        /// <summary>
        /// Retorna o conteúdo da resposta como o struct <see cref="S"/>
        /// </summary>
        /// <typeparam name="S">Nome do tipo do struct</typeparam>
        /// <param name="onErrorReturnDefaultValue">Se true, retorna o valor padrão do struct caso não
        /// seja possível desserializar o conteúdo no tipo <see cref="S"/>. Se false, retorna null</param>
        public S? ContentAs<S>(bool onErrorReturnDefaultValue = false) where S : struct {
            try { return System.Text.Json.JsonSerializer.Deserialize<S>(ContentAsString); }
            catch { if (onErrorReturnDefaultValue) return default(S); else return null; }
        }




        public DesserializationResult<C> TryDeserialize<C>( JsonSerializerOptions options = null ) where C : class {
            try {
                options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                var res = JsonSerializer.Deserialize<C>(ContentAsString,options);
                return new DesserializationResult<C>(res);
            }
            catch (Exception ex) {
                return new DesserializationResult<C>(ex);
            }
        }




        public StdApiResponse ThrowOnError(string additionalMessage, object additionalInfo, params StdApiWebException.SpecialCase[] specialCases) {
            if (IsSuccess) return this;
            var ex = StdApiWebException.From(this, additionalMessage, additionalInfo);
            ex.SpecialCases.AddRange(specialCases);
            if (ex == null) return this;
            throw ex;
        }




        public StdApiResponse ThrowOnError(string additionalMessage, object additionalInfo = null) {
            return ThrowOnError(additionalMessage, additionalInfo, null);
        }




        public StdApiResponse ThrowOnError() {
            return ThrowOnError(null, null, null);
        }




        public StdApiWebException ToException(string additionalMessage = null, object additionalInfo = null) {
            if (IsSuccess) return null;
            return StdApiWebException.From(this, additionalMessage, additionalInfo);
        }




        public StdApiResult ToResult() {
            return new StdApiResult(this);
        }




        public StdApiResult ToResult(string message) {
            return new StdApiResult(this, message);
        }




        private static T? TryOrNull<T>(Func<T> get) where T : struct {
            try { return get(); } catch { return null; }
        }




        public struct Blueprint {
            public Exception Exception;
            //public object CommStatusSource;
            public CommunicationStatus CommStatusCode;
            public string CommMessage;
            public HttpStatusCode? HttpStatusCode;
            public string ContentAsString;
            public string ContentEncoding;
            public Uri RequestUri;
            public long ContentLength;
            public string ContentType;
            public Version ProtocolVersion;
            public string CharacterSet;
            public Dictionary<string, string> Headers;
            public bool? IsFromCache;
            public DateTime? LastModified;
            public string Method;
            public string Server;
        }




        public enum CommunicationStatus {
            /// <summary>
            /// Pode representar qualquer um dos outros status (exceto 0, 13 e 14).
            /// É usado quando o erro não foi especificado.
            /// </summary>
            Aborted = -1,
            Success = 0,
            NameResolutionFailure = 1,
            ConnectFailure = 2,
            ReceiveFailure = 3,
            SendFailure = 4,
            PipelineFailure = 5,
            RequestCanceled = 6,
            ProtocolError = 7,
            ConnectionClosed = 8,
            TrustFailure = 9,
            SecureChannelFailure = 10,
            ServerProtocolViolation = 11,
            KeepAliveFailure = 12,
            Pending = 13,
            Timeout = 14,
            ProxyNameResolutionFailure = 15,
            UnknownError = 16,
            MessageLengthLimitExceeded = 17,
            CacheEntryNotFound = 18,
            RequestProhibitedByCachePolicy = 19,
            RequestProhibitedByProxy = 20,
        }




        public struct DesserializationResult<T> {
            public DesserializationResult(T value) {
                Value = value;
                Error = null;
            }
            public DesserializationResult(Exception error) {
                Value = default;
                Error = error;
            }
            public readonly T Value;
            public readonly Exception Error;
            const string _Message = "Não foi possível interpretar o resultado da chamada externa";
            public DesserializationResult<T> ThrowOnError(string message, object data = null) {
                if (Error != null) {
                    throw new StdApiException(HttpStatusCode.Conflict, message ?? _Message, data);
                }
                return this;
            }
            public DesserializationResult<T> ThrowOnError() {
                return ThrowOnError(_Message, null);
            }
        }
    }
}
