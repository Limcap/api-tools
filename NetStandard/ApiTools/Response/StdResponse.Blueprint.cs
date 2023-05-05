using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Limcap.ApiTools {

	public partial class StdResponse {
		public struct Blueprint {
			public Exception Exception;
			public CommunicationStatus CommStatusCode;
			public string CommMessage;
			public HttpStatusCode? HttpStatusCode;
			public byte[] ContentBytes;
			public string ContentAsString {
				get => ContentBytes?.AsString(CharacterSet);
				set {
					ContentBytes = Encoding.UTF8.GetBytes(value);
					ContentEncoding = Encoding.UTF8.WebName;
				}
			}
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




			public static Blueprint From( StdResponse resp ) {
				var blueprint = new Blueprint() {
					Exception = resp.Exception,
					CommStatusCode = resp.CommStatus,
					CommMessage = resp.CommMessage,
					HttpStatusCode = resp.HttpStatus,
					ContentBytes = resp.ContentBytes,
					ContentLength = resp.ContentLength,
					ContentType = resp.ContentType,
					ContentEncoding = resp.ContentEncoding,
					CharacterSet = resp.CharacterSet,
					Server = resp.Server,
					RequestUri = resp.RequestUri,
					Method = resp.Method,
					Headers = resp.Headers,
					ProtocolVersion = resp.ProtocolVersion,
					IsFromCache = resp.IsFromCache,
					LastModified = resp.LastModified,
				};
				return blueprint;
			}
		}




		public static StdResponse Clone( StdResponse resp ) {
			return new StdResponse(Blueprint.From(resp));
		}
	}
}
