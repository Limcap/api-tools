using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace StandardApiTools {
    //public class StdApiResult<T>: StdApiErrorResult where T : class {

    //    public StdApiResult(int status, string message, T data) : base(status, message, data) { }

    //    public StdApiResult(StdApiResponse response, string message) {
    //        base.Status = response.StatusCode;
    //        base.Content = Desserialize(response.ContentAsString);
    //        base.Message = ComposeMessage(message, Content);
    //    }

    //    public new T Content => (T)base.Content;

    //    public static T Desserialize(string json, JsonSerializerOptions options = null) {
    //        options ??= new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    //        try { return JsonSerializer.Deserialize<T>(json, options); }
    //        catch { return null; }
    //    }

    //    public static string ComposeMessage(string message, T content) {
    //        if (content != null) return message;
    //        return message
    //        + Environment.NewLine
    //        + "\nO conteúdo não pode ser desserializado em um objeto do tipo " + typeof(T).Name;
    //    }
    //}
}
