using Microsoft.AspNetCore.Mvc;
using System;

namespace StandardApiTools {
    public class ResponseAttribute: ProducesResponseTypeAttribute {
        public ResponseAttribute(int statusCode, Type type) : base(type, statusCode) {}
    }
}
