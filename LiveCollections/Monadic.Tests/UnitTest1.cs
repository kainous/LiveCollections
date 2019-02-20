using System.Threading.Tasks;
using CSharp.Collections.Monadic;
using CSharp.Collections.Monadic.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Monadic.Tests {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {
            var a = new Result<string, int>("Hello");
            var b = new Result<string, int>(3);
            var c = Option.Some(3);
            var d = Option.None<string>();
        }

        [TestMethod]
        public Option<T>
    }

    internal static class Helper {
        public static async Task<T> AsTask<T>(this Option<T> option) {
            return await option;
        }
    }
}
