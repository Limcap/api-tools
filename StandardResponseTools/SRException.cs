using System;

namespace StandardResponseTools {

    public class SRException: Exception, ISRReady {

        public SRException(int status, string message, object details = null)
        : base(message) {
            Result = new SRResult(status, message, details);
        }





        public SRException(Exception originalException, string message = null, object details = null)
        : base(null, originalException) {
            Result = new SRResult(500, message ?? originalException.Message, details ?? originalException.ToString());
        }






        public readonly SRResult Result;
        public int Status { get => Result.Status; }
        public object Details { get => Result.Data; }
        public override string Message => Result.Message;






        public static R Wrap<R>(
            Func<R> func,
            params (Type type, Func<Exception, SRException> converter)[] handlers
        ) {
            try {
                return func();
            }
            catch (Exception ex) {
                foreach (var c in handlers) {
                    if (ex.GetType() == c.type) throw c.converter(ex);
                }
                throw new SRException(ex);
            }
        }
    }
}