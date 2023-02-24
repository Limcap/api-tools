using System;

namespace StandardApiTools {

        public struct Safe<T> {
            public static Safe<T> Do(Func<T> action) {
                Safe<T> s;
                try { var result = action(); s = new Safe<T>(result); }
                catch (Exception ex) { s = new Safe<T>(ex); }
                return s;
            }
            public Safe(T value) {
                Value = value;
                Error = null;
            }
            public Safe(Exception error) {
                Value = default;
                Error = error;
            }
            public readonly T Value;
            public readonly Exception Error;
        }

}
