using System;
using static StandardResponseTools.ExternalServiceException;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StandardResponseTools {

    public class SRException: Exception, ISRReady {

        public SRException(int status, string message, object details=null)
        : base(message) {
            Status = status;
            Details = details;
        }





        public SRException(Exception originalException, string message = null, object details = null)
        : base(message??originalException.Message, originalException) {
            Status = 500;
            Details = details ?? originalException.ToString();
        }






        public int Status { get; private set; }
        public object Details { get; private set; }






        public static R Wrap<R>(
            Func<R> func,
            params (Type type ,Func<Exception,SRException> converter)[] handlers
        ) {
            try {
                return func();
            }
            catch (Exception ex) {
                foreach(var c in handlers) {
                    if (ex.GetType() == c.type) throw c.converter(ex);
                }
                throw new SRException(ex);
            }
        }
    }
}