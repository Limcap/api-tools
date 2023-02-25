using System;
using System.Collections.Generic;

namespace StandardApiTools {
    public abstract class StdApiExceptionBase: Exception, IProduceStdApiErrorResult, IAddInfo {

        protected StdApiExceptionBase(string message, Exception ex = null) : base(message, ex) {
            MessageParts = new List<string>(3);
            if (message != null) MessageParts.Add(message);
            Info = new StdApiDataCollection(new Dictionary<string, object>(3));
        }




        protected int statusCode;
        protected object content;
        public readonly List<string> MessageParts;




        public virtual int StatusCode { get => statusCode; } //protected set => statusCode = value;
        public override string Message { get => string.Join(Environment.NewLine, MessageParts); }
        public virtual object Content { get => content; set => content = value; }
        public virtual StdApiDataCollection Info { get; }




        public virtual void AddMessage(string value) {
            MessageParts.Add(value.Trim());
        }




        void IAddInfo.AddInfo(string key, object value) {
            AddInfo(key, value);
        }




        public virtual StdApiExceptionBase AddInfo(string key, object value) {
            Info.Add(key, value);
            return this;
        }




        public virtual StdApiErrorResult ToResult() {
            return new StdApiErrorResult(StatusCode, Message, Content, Info);
        }




        public void Throw() {
            throw this;
        }
    }
}
