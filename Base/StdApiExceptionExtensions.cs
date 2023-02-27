using System;
using System.Text.Json;

namespace StandardApiTools {
    public static class StdApiExceptionExtensions {
        public static E SetCustomResultType<E, T>(this E ex, T resultObject)
        where E : StdApiExceptionBase where T : IErrorToResultConverter<E> {
            resultObject.SetSourceException(ex);
            return ex;
        }
    }
}
