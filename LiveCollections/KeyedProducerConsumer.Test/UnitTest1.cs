using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KeyedProducerConsumer.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public async Task ImmediateDictionaryTest() {
            var dict = new KeyedProducerConsumer<string, int>();
            dict.AddOrReplace("Hello", "Hello".GetHashCode());
            var s = await dict.GetItem("Hello");
            Assert.AreEqual(s, "Hello".GetHashCode());
        }

        private async Task AsynchronousAddition(KeyedProducerConsumer<string, int> dictionary) {
            await Task.Delay(1000);
            dictionary.AddOrReplace("World", "World".GetHashCode());
            dictionary.AddOrReplace("Bar", "Bar".GetHashCode());
            await Task.Delay(1000);
            dictionary.AddOrReplace("Foo", "Foo".GetHashCode());
        }

        [TestMethod]
        public async Task DelayedDictionaryTest() {
            var dict = new KeyedProducerConsumer<string, int>();
            var ignore = AsynchronousAddition(dict);
            var s1 = dict.GetItem("World");
            var s2 = dict.GetItem("Foo");
            var s3 = dict.GetItem("Bar");
            
            Assert.AreEqual(await s1, "World".GetHashCode());
            Assert.AreEqual(await s3, "Bar".GetHashCode());
            Assert.AreEqual(await s2, "Foo".GetHashCode());
        }

        private Task AsynchronousAddition2(KeyedProducerConsumer<string, int> dictionary) {
            dictionary.AddOrReplace("World", "World".GetHashCode());
            dictionary.AddOrReplace("Bar", "Bar".GetHashCode());
            dictionary.AddOrReplace("Foo", "Foo".GetHashCode());
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task RaceTest() {
            for (var i = 0; i < 5000000; i++) {

                var dict = new KeyedProducerConsumer<string, int>();
                var ignore = AsynchronousAddition2(dict);
                var s1 = dict.GetItem("World");
                var s2 = dict.GetItem("Foo");
                var s3 = dict.GetItem("Bar");

                Assert.AreEqual(await s1, "World".GetHashCode());
                Assert.AreEqual(await s3, "Bar".GetHashCode());
                Assert.AreEqual(await s2, "Foo".GetHashCode());
            }
        }
    }
}
