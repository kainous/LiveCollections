using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AwaitableDictionary.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public async Task ImmediateDictionaryTest() {
            var dict = new AwaitableDictionary<string, int>();
            dict.AddOrReplace("Hello", "Hello".GetHashCode());
            var s = await dict.GetItem("Hello");
            Assert.AreEqual(s, "Hello".GetHashCode());
        }

        private async Task AsynchronousAddition(AwaitableDictionary<string, int> dictionary) {
            await Task.Delay(1000);
            dictionary.AddOrReplace("World", "World".GetHashCode());
            dictionary.AddOrReplace("Bar", "Bar".GetHashCode());
            await Task.Delay(1000);
            dictionary.AddOrReplace("Foo", "Foo".GetHashCode());
        }

        [TestMethod]
        public async Task DelayedDictionaryTest() {
            var dict = new AwaitableDictionary<string, int>();
            var ignore = AsynchronousAddition(dict);
            var s1 = dict.GetItem("World");
            var s2 = dict.GetItem("Foo");
            var s3 = dict.GetItem("Bar");
            
            Assert.AreEqual(await s1, "World".GetHashCode());
            Assert.AreEqual(await s3, "Bar".GetHashCode());
            Assert.AreEqual(await s2, "Foo".GetHashCode());
        }
    }
}
