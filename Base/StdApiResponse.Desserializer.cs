using System;
using System.Net;
using System.Text.Json;

namespace StandardApiTools {

    public partial class StdApiResponse {
        public static class Desserializer {

            public struct ClassResult<C> where C: class {
                public ClassResult(C obj) { Object = obj; Error = null; }
                public ClassResult(Exception ex) { Object = null;  Error = ex; }
                public readonly C Object;
                public readonly Exception Error;
                public StdApiException ToException(string message, object content = null) {
                    if (Error == null) return null;
                    return new StdApiException(HttpStatusCode.Conflict, message ?? DefaultMessage, content);
                }
                public StdApiException ToException() {
                    return ToException(DefaultMessage);
                }
            }




            public struct StructResult<S> where S : struct {
                public StructResult(S obj) { Object = obj; Error = null; }
                public StructResult(Exception ex) { Object = default; Error = ex; }
                public readonly S Object;
                public readonly Exception Error;
                public StdApiException ToException(string message, object content = null) {
                    if (Error == null) return null;
                    return new StdApiException(HttpStatusCode.Conflict, message ?? DefaultMessage, content);
                }
                public StdApiException ToException() {
                    return ToException(DefaultMessage);
                }
            }




            //public Desserializer(string content, JsonSerializerOptions options = null) {
            //    try {
            //        options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            //        var res = JsonSerializer.Deserialize<T>(content, options);
            //        return new Desserializer<T>(res);
            //    }
            //    catch (Exception ex) {
            //        return new Desserializer<T>(ex);
            //    }
            //}

            //public Desserializer<T> From<T>(string content, JsonSerializerOptions options = null) where T : class {
            //    try {
            //        options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            //        var res = JsonSerializer.Deserialize<T>(content, options);
            //        return new Desserializer<T>(res);
            //    }
            //    catch (Exception ex) {
            //        return new Desserializer<T>(ex);
            //    }
            //}
            public static ClassResult<T> Deserialize<T>(string content, JsonSerializerOptions options = null) where T : class {
                try {
                    options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    var res = JsonSerializer.Deserialize<T>(content, options);
                    return new ClassResult<T>(res);
                }
                catch (Exception ex) {
                    return new ClassResult<T>(ex);
                }
            }




            public static StructResult<T> Deserialize<T>(string content, bool useDefaultInsteadOfNull = false, JsonSerializerOptions options = null) where T : struct {
                try {
                    options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    var res = JsonSerializer.Deserialize<T>(content, options);
                    return new StructResult<T>(res);
                }
                catch (Exception ex) {
                    return new StructResult<T>(ex);
                }
            }



            const string DefaultMessage = "Não foi possível interpretar o resultado da chamada externa";




            //public DesserializationResult<T> ThrowOnError(string message, object data = null) {
            //    if (Error != null) {
            //        throw new StdApiException(HttpStatusCode.Conflict, message ?? _Message, data);
            //    }
            //    return this;
            //}
            //public DesserializationResult<T> ThrowOnError() {
            //    return ThrowOnError(_Message, null);
            //}
            //public StdApiException ToException(string message, object content = null) {
            //    if (Error == null) return null;
            //    return new StdApiException(HttpStatusCode.Conflict, message ?? DefaultMessage, content);
            //}




            //public StdApiException ToException() {
            //    return ToException(DefaultMessage);
            //}
        }
    }
}
