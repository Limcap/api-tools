using Microsoft.AspNetCore.Mvc;

namespace Limcap.ApiTools.AspNetCore {
	public static class Ext {
		public static IActionResult ToIActionResult( this ApiResult result ) {
			return new ActionResult {
				StatusCode = result.StatusCode,
				CompiledResultObject = result.CompiledResultObject
			};
		}
	}
}
