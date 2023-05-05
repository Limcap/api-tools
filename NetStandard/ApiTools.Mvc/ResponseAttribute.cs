using Microsoft.AspNetCore.Mvc;
using System;

namespace Limcap.ApiTools.Mvc {
	public class ResponseAttribute : ProducesResponseTypeAttribute {
		public ResponseAttribute( int statusCode, Type type ) : base(type, statusCode) { }
	}
}
