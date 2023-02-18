namespace StandardApiTools {
    public partial class StdApiWebException {

        public struct SpecialCase {

            public SpecialCase(int status, C condition, M message, D details) {
                Status = status;
                Condition = condition;
                Message = message;
                Content = details;
            }

            public SpecialCase(int status, M message) : this(status, null, message, null) { }

            public SpecialCase(int status, C condition, M message) : this(status, condition, message, null) { }

            public SpecialCase(int status, M message, D details) : this(status, null, message, details) { }




            public int Status;
            public C Condition;
            //public Func<StdApiResponse, bool> Condition;
            public M Message;
            //public Func<StdApiResponse, string> Message;
            public D Content;
            //public Func<StdApiResponse, object> Details;




            public delegate bool C(StdApiResponse r);
            public delegate string M(StdApiResponse r);
            public delegate object D(StdApiResponse r);




            public static class StaticMethods {

                public static SpecialCase Case(int status, C condition, M message, D details) {
                    return new SpecialCase(status, condition, message, details);
                }

                public static SpecialCase Case(int status, C condition, M message) {
                    return new SpecialCase(status, condition, message, null);
                }

                public static SpecialCase Case(int status, M message, D details) {
                    return new SpecialCase(status, null, message, details);
                }

                public static SpecialCase Case(int status, M message) {
                    return new SpecialCase(status, null, message, null);
                }
            }
        }
    }
}
