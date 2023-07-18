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

		public ApiResult(HttpStatusCode status, object content) {
			_StatusCode = (int)status;
			_CompiledResultObject = content;
		}

		public ApiResult(int statusCode, object content) {
			_StatusCode = statusCode;
			_CompiledResultObject = content;
		}

		protected int _StatusCode;
		public virtual int GetStatusCode() => _StatusCode;
		protected object _CompiledResultObject;
		public virtual object GetCompiledResultObject() => _CompiledResultObject;
	}
}
