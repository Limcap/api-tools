using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Limcap.ApiTools {

	public partial class StdResponse : IMakeApiErrorResult {

		public static async Task<StdResponse> FromAsync( HttpWebRequest req ) {
			try {
				var res = await req.GetResponseAsync();
				return new StdResponse(req, res as HttpWebResponse);
			}
			catch (WebException ex) {
				return new StdResponse(req, ex);
			}
		}




		public static StdResponse From( HttpWebRequest req ) {
			try {
				var res = req.GetResponse();
				return new StdResponse(req, res as HttpWebResponse);
			}
			catch (WebException ex) {
				return new StdResponse(req, ex);
			}
		}




		public WebException Exception { get; }
		public string CommMessage { get; }
		public CommunicationStatus CommStatus { get; }
		public HttpStatusCode? HttpStatus { get; }
		public int StatusCode => HttpStatus.HasValue ? (int)HttpStatus : (int)CommStatus;
		public bool IsSuccess => HttpStatus.HasValue && ((int)HttpStatus) < 300;
		public byte[] ContentBytes { get; }
		public string ContentAsString { get => ContentBytes?.AsString(CharacterSet); }
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




		public StdResponse( HttpWebRequest req, HttpWebResponse resp )
		: this(req, resp, CommunicationStatus.Success, "Communication successfully completed") { }




		private StdResponse( HttpWebRequest req, WebException ex )
		: this(req, ex.Response, ex.Status.ToCommStatus(), ex.Message) {
			Exception = ex;
		}




		private StdResponse(
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
			ContentBytes = resp.GetContentAsBytes();
			resp.Dispose();
		}




		public StdResponse( Blueprint b ) {
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
			ContentBytes = b.ContentBytes;
		}




		private static T? TryOrNull<T>( Func<T> get ) where T : struct {
			try { return get(); } catch { return null; }
		}




		public ApiWebException ToException( CommunicationStatus status ) => ToException((int)status);
		public ApiWebException ToException( HttpStatusCode status ) => ToException((int)status);
		public ApiWebException ToException( int status ) {
			if (status == (int)CommStatus || (HttpStatus.HasValue && status == (int)HttpStatus))
				return ApiWebException.From(this);
			else return null;
		}
		public ApiWebException ToException( params int[] statuses ) {
			if (statuses.Contains((int)CommStatus) || (HttpStatus.HasValue && statuses.Contains((int)HttpStatus)))
				return ApiWebException.From(this);
			else return null;
		}
		public ApiWebException ToException() {
			return ApiWebException.From(this);
		}




		//public StdApiException TryDeserializeContent<T>(out T result, JsonSerializerOptions opt = null)
		public ApiException TryDeserializeContent<T>( out T result ) {
			try {
				//result = JsonUtil.Deserialize<T>(ContentAsString, opt);
				result = JsonUtil.Deserialize<T>(ContentAsString);
				return null;
			}
			catch (Exception ex) {
				result = default;
				var msg = "Não foi possível desserializar o conteúdo";
				return new ApiException(HttpStatusCode.Conflict, msg, ex.Message);
			}
		}




		//public T DeserializeContent<T>(out StdApiException exception, JsonSerializerOptions options = null) {
		//    exception = TryDeserializeContent<T>(out var result, options);
		public T DeserializeContent<T>( out ApiException exception ) {
			exception = TryDeserializeContent<T>(out var result);
			return result;
		}




		public string GetHeader( string key ) {
			if (Headers == null) return null;
			foreach (var item in Headers) {
				if (item.Key.ToLower() == key.ToLower())
					return item.Value;
				//if (item.Value.ToLower() == key.ToLower())
				//    return item.Key;
			}
			return null;
		}
		public string GetHeader_ContentDisposition() {
			return GetHeaderSubvalue("Content-Disposition")?.Trim('"');
		}
		public string GetHeader_ContentDisposition_Filename() {
			return GetHeaderSubvalue("Content-Disposition", "filename=")?.Trim('"');
		}
		public string GetHeader_ContentDisposition_Name() {
			return GetHeaderSubvalue("Content-Disposition", "name=")?.Trim('"');
		}
		public string GetHeader_ContentType() {
			return GetHeaderSubvalue("Content-Type");
		}
		public string GetHeader_ContentType_Charset() {
			return GetHeaderSubvalue("Content-Type", "charset=");
		}
		private string GetHeaderSubvalue( string header, string key = null ) {
			var subitems = GetHeader(header)
						.Split(';')
						.Select(a => a.ToLower().Trim());
			subitems = subitems.Where(b => b.StartsWith(key.ToLower()));
			var selected = subitems.FirstOrDefault();
			if (key == null) return selected;
			if (key.Length >= selected.Length) return null;
			return subitems.FirstOrDefault()?.Substring(key.Length);
		}




		public ApiErrorResult ToResult( string message ) => new ApiErrorResult(this, message);
		public ApiErrorResult ToResult() => new ApiErrorResult(this);
		ApiResult IMakeApiResult.ToResult() => ToResult();
	}
}
