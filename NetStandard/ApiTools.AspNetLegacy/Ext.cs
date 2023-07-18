using Limcap.ApiTools;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Limcap.ApiTools.AspNetLegacy {
	public static class Ext {
		public static HttpResponseMessage ToHttpResponseMessage( this ApiResult result ) {
			HttpContent content;
			var c = result.GetCompiledResultObject();
			if (c is HttpContent ht) content = ht;
			else if (c is string str) content = new StringContent(str, Encoding.UTF8);
			else content = new StringContent(JsonUtil.Serialize(c), Encoding.UTF8);
			return new HttpResponseMessage((HttpStatusCode)result.GetStatusCode()) {
				StatusCode = (HttpStatusCode)result.GetStatusCode(),
				Content = content,
			};
		}
	}
}
