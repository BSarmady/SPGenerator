using System;

namespace SPGenerator {

    public class gen_exception: Exception {
        public gen_exception() : base() { }
        public gen_exception(string message) : base(message) { }
        public gen_exception(string message, Exception innerException) : base(message, innerException) { }
    }

}