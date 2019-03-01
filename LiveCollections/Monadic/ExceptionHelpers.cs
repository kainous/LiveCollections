using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace System {
    public static class ExceptionHelpers {
        public static Exception Rethrow(this Exception exception) {
            ExceptionDispatchInfo.Capture(exception).Throw();

            // This next line is never called
            return exception;
        }

        public static T Rethrow<T>(this Exception exception) {
            ExceptionDispatchInfo.Capture(exception).Throw();

            // This next line is never called
            return default;
        }
    }
}
