namespace Limcap.ApiTools {
	public static class ApiExceptionExtensions {

		public static E SetMessage<E>( this E ex, string message ) where E : ApiExceptionBase {
			ex.MessageParts.Clear();
			ex.MessageParts.Add(message.TrimToNull());
			return ex;
		}




		public static E AddMessage<E>( this E ex, string value ) where E : ApiExceptionBase {
			ex.MessageParts.Add(value?.Trim());
			return ex;
		}




		public static E InsertMessage<E>( this E ex, string value ) where E : ApiExceptionBase {
			ex.MessageParts.Insert(0, value.Trim());
			return ex;
		}




		public static E ClearMessage<E>( this E ex, string value ) where E : ApiExceptionBase {
			ex.MessageParts.Clear();
			return ex;
		}




		public static E AddInfo<E>( this E ex, string key, object value ) where E : ApiException {
			ex.Info.Add(key, value);
			return ex;
		}
	}
}
