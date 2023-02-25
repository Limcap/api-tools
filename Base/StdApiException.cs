using System;
using System.Net;

namespace StandardApiTools {

    public class StdApiException: StdApiExceptionBase {

        protected StdApiException(WebException innerException, string message = null)
        : base(message, innerException) { }




        public StdApiException(Exception innerException, string message = null)
        : base(message ?? innerException.Message, innerException) {
            this.statusCode = 500;
            this.content = innerException.ToString();
        }




        public StdApiException(string message, object content = null)
        : base(message) {
            this.statusCode = 500;
            this.content = content;
        }




        public StdApiException(HttpStatusCode code, string message = null, object content = null)
        : base(message ?? code.ToString()) {
            this.statusCode = (int)code;
            this.content = content;
        }




        public new StdApiException AddMessage(string value) {
            MessageParts.Add(value.Trim());
            return this;
        }




        public new StdApiException AddInfo(string key, object value) {
            Info.Add(key, value);
            return this;
        }
    }
}
