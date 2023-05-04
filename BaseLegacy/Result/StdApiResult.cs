using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using System.Threading;
using System.Text;

namespace StandardApiTools {

    public class StdApiResult: HttpResponseMessage {

        protected StdApiResult() { }

        public StdApiResult(HttpStatusCode status, object content) {
            StatusCode = status;
            CompiledResultObject = content;
        }

        //public int StatusCode;
        protected object CompiledResultObject {
            get {
                return Content;
            }
            set {
                if (value is HttpContent ht) Content = ht;
                else if (value is string str) Content = new StringContent(str, Encoding.UTF8);
                //if (CompiledResultObject is string str) Content = new StringContent(str);
                else Content = new StringContent(JsonUtil.Serialize(value), Encoding.UTF8);
            }
        }
        //public new HttpContent Content 


        public static StdApiResult From(Exception ex) {
            ex = ex.Deaggregate();
            var result = ex is IProduceStdApiErrorResult pr
            ? pr.ToResult()
            : new StdApiResult(HttpStatusCode.InternalServerError, ex.ToString());
            return result;
        }


        //public virtual Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken) {
        //    if (cancellationToken.IsCancellationRequested) throw new Exception();
        //    return Task.FromResult(GetResponseMessage());
        //}


        //public HttpResponseMessage GetResponseMessage() {
        //    var content = CompiledResultObject is string
        //        ? CompiledResultObject.ToString()
        //        : JsonUtil.Serialize(CompiledResultObject);
        //    return new HttpResponseMessage() {
        //        StatusCode = (HttpStatusCode)this.StatusCode,
        //        Content = new StringContent(content),
        //    };
        //}
    }
}
