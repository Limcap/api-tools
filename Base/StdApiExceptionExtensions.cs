using System;
using System.Text.Json;

namespace StandardApiTools {
    public static class StdApiExceptionExtensions {

        public static E SetMessage<E>(this E ex, string message) where E : StdApiExceptionBase {
            ex.MessageParts.Clear();
            ex.MessageParts.Add(message.TrimToNull());
            return ex;
        }




        public static E AddMessage<E>(this E ex, string value) where E : StdApiExceptionBase {
            ex.MessageParts.Add(value.Trim());
            return ex;
        }




        public static E InsertMessage<E>(this E ex, string value) where E : StdApiExceptionBase {
            ex.MessageParts.Insert(0, value.Trim());
            return ex;
        }




        public static E AddInfo<E>(this E ex, string key, object value) where E : StdApiException {
            ex.Info.Add(key, value);
            return ex;
        }




        //public static E SetContentType<E, T>(this E ex, JsonSerializerOptions opt = null) where E : StdApiException {
        //    if (ex.Content == null) return ex;
        //    try {
        //        var json = ex.Content as string ?? JsonSerializer.Serialize(ex.Content, opt);
        //        ex.Content = JsonSerializer.Deserialize<T>(json, opt);
        //    }
        //    catch (Exception e) {
        //        ex.Info.Add(
        //            "Erro de desserialização",
        //            "O conteúdo está apresentado no formato cru, pois não foi possível " +
        //            "desserializá-lo. " + Environment.NewLine + e.Message
        //        );
        //        //try { ex.Content = JsonSerializer.Deserialize<object>(str, opt); }
        //        //catch { }
        //    }
        //    return ex;
        //}
    }
}
