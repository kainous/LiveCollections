using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AwaitableDictionary.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public async Task TestMethod1() {
            var dict = new AwaitableDictionary<string, int>();
            dict.AddOrReplace("Hello", "Hello".GetHashCode());
            var s = await dict.GetItem("Hello");
        }

        [TestMethod]
        public async Task DelayedDictionaryTest() {
            var dict = new AwaitableDictionary<string, int>();
            Task.Run(async () => {
                await Task.Delay(5000);
                dict.AddOrReplace("World", "World".GetHashCode());
            });
            var s = await dict.GetItem("World");
        }
    }
}
