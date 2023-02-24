using System;
using System.Collections.Generic;
using System.Net;

namespace StandardApiTools {

    public partial class StdApiResponse {
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
    }
}
