using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace StandardApiTools {

    public partial class StdApiResponse {
        public struct Blueprint {
            public Exception Exception;
            public CommunicationStatus CommStatusCode;
            public string CommMessage;
            public HttpStatusCode? HttpStatusCode;
            public byte[] ContentBytes;
            public string ContentAsString {
                get => ContentBytes?.ToEncodedString(ContentEncoding);
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
        }
    }
}
