using System;
using System.Collections.Generic;

namespace StandardApiTools {
    public abstract class StdApiExceptionBase: Exception, IProduceStdApiErrorResult {

        protected StdApiExceptionBase(string message, Exception ex = null) : base(message, ex) {
            MessageParts = new List<string>(3);
            if (message != null) MessageParts.Add(message);
        }




        protected int statusCode;
        protected object content;
        protected object info;
        public readonly List<string> MessageParts;




        public virtual int StatusCode { get => statusCode; } //protected set => statusCode = value;
        public override string Message { get => string.Join(Environment.NewLine, MessageParts); }
        public virtual object Content { get => content; set => content = value; }
        public virtual object Info { get => info; set => info = value; }




        public virtual void AddMessage(string value) => MessageParts.Add(value.Trim());




        public void Throw() => throw this;
        public virtual StdApiErrorResult ToResult() => new StdApiErrorResult(StatusCode, Message, Content, Info);
        StdApiResult IProduceStdApiResult.ToResult() => ToResult();



        public Func<StdApiExceptionBase, StdApiResult> ResultConverter { get; set; }
    }
}
