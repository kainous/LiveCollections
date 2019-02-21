using CSharp.Collections.Monadic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharp.Collections.Monadic.Tasks;

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

        public async Option<string> Concatenate(Option<string> a, Option<string> b) {
            var x = await a;
            var y = await b;
            return x + y;
        }

        [TestMethod]
        public void TestMethod2() {
            var result = Concatenate(Option.Some("Hello "), Option.Some("World"));
            Assert.AreEqual(result.GetValue(string.Empty), "Hello World");
            result = Concatenate(Option.None<string>(), Option.Some("Try"));
            Assert.AreEqual(result.GetValue("Fail"), "Fail");
        }
    }

    internal static class Helper {
        //public static async Task<T> AsTask<T>(this Option<T> option) {
        //     await option;
        //}
    }
}
