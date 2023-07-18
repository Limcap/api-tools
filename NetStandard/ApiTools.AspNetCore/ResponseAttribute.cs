using Microsoft.AspNetCore.Mvc;
using System;

namespace Limcap.ApiTools.AspNetCore {
	public class ResponseAttribute : ProducesResponseTypeAttribute {
		public ResponseAttribute(int statusCode, Type type) : base(type, statusCode) { }
		public ResponseAttribute(int statusCode) : base(statusCode) { }
	}
}
