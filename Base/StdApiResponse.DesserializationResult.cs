using System;
using System.Net;

namespace StandardApiTools {

    public partial class StdApiResponse {

        public struct DesserializationResult<T> {

            public DesserializationResult(T obj) { Object = obj; Error = null; }

            public DesserializationResult(Exception ex) { Object = default; Error = ex; }

            public readonly T Object;
            public readonly Exception Error;
            const string DefaultDesserializationErrorMessage = "Não foi possível interpretar o resultado da chamada externa";

            public StdApiException2 ToException(string message, object content = null) {
                if (Error == null) return null;
                return new StdApiException2(HttpStatusCode.Conflict, message ?? DefaultDesserializationErrorMessage, content);
            }

            public StdApiException2 ToException() {
                return ToException(DefaultDesserializationErrorMessage);
            }
        }
    }
}
