using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;

namespace StandardApiTools {
    public class StdApiResult<T>: StdApiResult {

        protected StdApiResult() { }

        public StdApiResult(HttpStatusCode status, T content) {
            StatusCode = (int)status;
            CompiledResultObject = content;
        }

        public new T CompiledResultObject { get; set; }
    }
}
