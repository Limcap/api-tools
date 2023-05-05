using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Limcap.ApiTools {

	public class ApiResult {

		protected ApiResult() { }

		public ApiResult( HttpStatusCode status, object content ) {
			StatusCode = (int)status;
			CompiledResultObject = content;
		}

		public ApiResult( int statusCode, object content ) {
			StatusCode = statusCode;
			CompiledResultObject = content;
		}

		public virtual int StatusCode { get; protected set; }
		public virtual object CompiledResultObject { get; protected set; }
	}
}
