using System;
using System.Net;

namespace StandardApiTools {
    public class StdApiException: Exception, IProduceStdApiErrorResult, IAddInfo {

        public StdApiException(Exception innerException, string message = null)
        : base(message ?? innerException.Message, innerException) {
            StatusCode = 500;
            Content = innerException.ToString();
        }




        public StdApiException(string message, object content = null)
        : base(message) {
            StatusCode = 500;
            Content = content;
        }




        public StdApiException(HttpStatusCode code, string message = null, object content = null)
        : base(message ?? code.ToString()) {
            StatusCode = (int)code;
            Content = content;
        }




        public readonly int StatusCode;
        public readonly object Content;
        public StdApiDataCollection Info = new StdApiDataCollection();




        public StdApiErrorResult ToResult() {
            return new StdApiErrorResult(StatusCode, Message, Content, Info);
        }




        public void Throw() {
            throw this;
        }




        void IAddInfo.AddInfo(string key, object value) => AddInfo(key, value);
        public StdApiException AddInfo(string key, object value) {
            Info.Add(key, value);
            return this;
        }
    }
}
