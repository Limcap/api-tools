namespace StandardApiTools {
    public static class StdApiExceptionExtensions {

        public static E SetMessage<E>(this E ex, string message) where E : StdApiExceptionBase {
            ex.MessageParts.Clear();
            ex.MessageParts.Add(message.TrimToNull());
            return ex;
        }




        public static E AddMessage<E>(this E ex, string value) where E : StdApiExceptionBase {
            ex.MessageParts.Add(value?.Trim());
            return ex;
        }




        public static E InsertMessage<E>(this E ex, string value) where E : StdApiExceptionBase {
            ex.MessageParts.Insert(0, value.Trim());
            return ex;
        }




        public static E ClearMessage<E>(this E ex, string value) where E : StdApiExceptionBase {
            ex.MessageParts.Clear();
            return ex;
        }




        public static E AddInfo<E>(this E ex, string key, object value) where E : StdApiException {
            ex.Info.Add(key, value);
            return ex;
        }
    }
}
