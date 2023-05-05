using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Limcap.ApiTools {

	public static class JsonUtil {

		public static string Serialize( object obj ) {
			return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
		}
		public static T Deserialize<T>( string json ) {
			return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
		}




		public static Exception TryDeserialize( this string str, out object result ) {
			var error = str.TryDeserialize<object>(out var r);
			result = r;
			return error;
		}




		public static Exception TryDeserialize<T>( this string str, out T result ) {
			try { result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str); return null; }
			catch (Exception ex) { result = default; return ex; }
		}
	}
}
