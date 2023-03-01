using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StandardApiTools {

    public static class JsonUtil {

        public static string Serialize(object obj, JsonSerializerOptions opt = null) {
            return JsonSerializer.Serialize(obj, opt ?? DefaultOptions);
        }
        public static T Deserialize<T>(string json, JsonSerializerOptions opt = null) {
            return JsonSerializer.Deserialize<T>(json, opt ?? DefaultOptions);
        }




        private static JsonSerializerOptions DefaultOptions {
            get {
                var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                opt.Converters.Add(new StringConverter());
                opt.Converters.Add(new TimeSpanConverter());
                opt.Converters.Add(new ObjectConverter());
                return opt;
            }
        }




        private class StringConverter: JsonConverter<string> {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                if (reader.TokenType == JsonTokenType.Number) {
                    return reader.GetInt32().ToString();
                }
                else if (reader.TokenType == JsonTokenType.String) {
                    var str = reader.GetString();
                    return str;
                }
                else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False) {
                    return reader.GetBoolean().ToString();
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) {
                writer.WriteStringValue(value);
            }
        }




        private class TimeSpanConverter: JsonConverter<TimeSpan> {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                var converter = new System.ComponentModel.TimeSpanConverter();
                if (reader.TokenType == JsonTokenType.String) {
                    var str = reader.GetString();
                    var ts1 = converter.ConvertFrom(str);
                    TimeSpan.TryParse(str, out var ts);
                    return ts;
                }
                throw new JsonException("Value can't be converted to System.TimeSpan");
            }

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) {
                writer.WriteStringValue(value.ToString());
            }
        }




        private class ObjectConverter: JsonConverter<object> {
            public override Object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                if (reader.TokenType == JsonTokenType.String) {
                    var str = reader.GetString();
                    if (str.StartsWith("{") && str.EndsWith("}")) {
                        var obj = Deserialize<ExpandoObject>(str);
                        return obj;
                    }
                    else return str;
                }
                else if(reader.TokenType == JsonTokenType.StartObject) {
                    var sb = new StringBuilder();
                    var eo = new ExpandoObject();
                    var eoc = (ICollection<KeyValuePair<string, object>>)eo;
                    sb.Append('{');
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject) {
                        string key = null;
                        object value = null;
                        if (reader.TokenType == JsonTokenType.PropertyName) key = reader.GetString();
                        reader.Read();
                        if (reader.TokenType == JsonTokenType.StartArray)
                            do { reader.Read(); }
                            while (reader.TokenType != JsonTokenType.EndArray);
                        if (reader.TokenType == JsonTokenType.Number) { reader.TryGetInt64(out var val); value = val; }
                        else if (reader.TokenType == JsonTokenType.String) { value = reader.GetString(); }
                        var kv = new KeyValuePair<string, object>(key, value);
                        eoc.Add(kv);
                    }
                    if (reader.TokenType == JsonTokenType.EndObject) return eo;
                }
                throw new JsonException("Value can't be converted to System.Object");
            }

            public override void Write(Utf8JsonWriter writer, Object value, JsonSerializerOptions options) {
                writer.WriteStringValue(value.ToString());
            }
        }










        /// <summary>
        /// Tenta desserializar uma string para um objeto anonimo.
        /// Se não conseguir, retorna a própria string.
        /// </summary>
        public static object DeserializeOr(this string str, JsonSerializerOptions opt = null) {
            return str.DeserializeOr<object>(str, opt ?? JsonUtil.DefaultOptions);
        }




        /// <summary>
        /// Tenta desserializar uma string para um objeto anonimo.
        /// Se não conseguir, retorna o valor informado, que pode ser null.
        /// </summary>
        public static object DeserializeOr(this string str, object orValue, JsonSerializerOptions opt = null) {
            return str.DeserializeOr<object>(orValue, opt ?? JsonUtil.DefaultOptions);
        }




        /// <summary>
        /// Tenta desserializar uma string para um objeto anonimo.
        /// Se não conseguir, retorna o valor informado.
        /// </summary>
        public static T DeserializeOr<T>(this string str, T orValue, JsonSerializerOptions opt = null) where T : class {
            var ok = str.TryDeserialize<T>(out var r, opt ?? JsonUtil.DefaultOptions);
            return ok ? r : orValue;
        }




        public static S? DeserializeOr<S>(this string str, S? orValue = null, JsonSerializerOptions opt = null) where S : struct {
            var ok = str.TryDeserialize<S>(out var r, opt ?? JsonUtil.DefaultOptions);
            return ok ? r : orValue;
        }



        public static bool TryDeserialize(this string str, out object result, JsonSerializerOptions opt = null) {
            var ok = str.TryDeserialize<object>(out var r, opt ?? JsonUtil.DefaultOptions);
            result = r;
            return ok;
        }




        public static bool TryDeserialize<T>(this string str, out T result, JsonSerializerOptions opt = null) {
            // A idéia era usar o System.Text.Json, para não ter que usar o Newtonsoft.Json, porém
            // por enquanto terá que ser o Newtonsoft pq os objetos desserializados com o System.Text.Json
            // não são exibidos corretamente em algumas implementações do Swagger. Por exemplo, fica:
            // { "ValueKind" : 1 } ao invés de { "chave" : valor }.
            try { result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str); return true; }
            catch { result = default; return false; }
            //try { result = JsonSerializer.Deserialize<T>(str, opt ?? JsonUtil.DefaultOptions); return true; }
            //catch (Exception ex) { result = default; return false; }
        }
    }
}
