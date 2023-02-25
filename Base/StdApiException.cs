using System;
using System.Collections;
using System.Net;

namespace StandardApiTools {
    public class StdApiException: Exception, IProduceStdApiErrorResult, IAddInfo {

        protected StdApiException(WebException innerException, string message = null)
        : base(message, innerException) { }




        public StdApiException(Exception innerException, string message = null)
        : base(message ?? innerException.Message, innerException) {
            StatusCode = 500;
            Content = innerException.ToString();
            Info = new StdApiDataCollection();
        }




        public StdApiException(string message, object content = null)
        : base(message) {
            StatusCode = 500;
            Content = content;
            Info = new StdApiDataCollection();
        }




        public StdApiException(HttpStatusCode code, string message = null, object content = null)
        : base(message ?? code.ToString()) {
            StatusCode = (int)code;
            Content = content;
            Info = new StdApiDataCollection();
        }




        public virtual int StatusCode { get; }
        //public override string Message { get => base.Message; }
        public virtual object Content { get; }
        public virtual StdApiDataCollection Info { get; protected set; }
        private new IDictionary Data { get; }




        public virtual StdApiErrorResult ToResult() {
            return new StdApiErrorResult(StatusCode, Message, Content, Info);
        }




        public void Throw() {
            throw this;
        }




        void IAddInfo.AddInfo(string key, object value) => AddInfo(key, value);
        public virtual StdApiException AddInfo(string key, object value) {
            Info.Add(key, value);
            return this;
        }
    }
}
