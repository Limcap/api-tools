using System;
using System.Collections.Generic;

namespace Limcap.ApiTools {
	public abstract class ApiExceptionBase : Exception, IMakeApiErrorResult {

		protected ApiExceptionBase( string message, Exception ex = null ) : base(message, ex) {
			MessageParts = new List<string>(3);
			if (message != null) MessageParts.Add(message);
		}




		protected int statusCode;
		protected object details;
		protected object info;
		public readonly List<string> MessageParts;




		public virtual int StatusCode { get => statusCode; }
		public override string Message { get => string.Join(Environment.NewLine, MessageParts); }
		public virtual object Details { get => details; }
		public virtual object Info { get => info; }




		public void Throw() => throw this;

		public ApiErrorResult ToResult() => new ApiErrorResult(StatusCode, Message, Details, Info);

		ApiResult IMakeApiResult.ToResult() => ToResult();
	}
}
