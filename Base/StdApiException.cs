using System;
using System.Collections.Generic;
using System.Net;

namespace StandardApiTools {

    public class StdApiException: StdApiExceptionBase, IAddInfo {

        protected StdApiException(WebException innerException, string message = null)
        : base(message, innerException) {
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public StdApiException(Exception innerException, string message = null)
        : base(message ?? innerException.Message, innerException) {
            this.statusCode = 500;
            this.content = innerException.ToString();
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public StdApiException(string message, object content = null)
        : base(message) {
            this.statusCode = 500;
            this.content = content;
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public StdApiException(HttpStatusCode code, string message = null, object content = null)
        : base(message ?? code.ToString()) {
            this.statusCode = (int)code;
            this.content = content;
            this.info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        public new StdApiException AddMessage(string value) {
            MessageParts.Add(value.Trim());
            return this;
        }




        public new StdApiDataCollection Info { get => info as StdApiDataCollection; }
        



        IAddInfo IAddInfo.AddInfo(string key, object value) => AddInfo(key, value);
        public virtual StdApiException AddInfo(string key, object value) {
            Info.Add(key, value);
            return this;
        }
    }
}
