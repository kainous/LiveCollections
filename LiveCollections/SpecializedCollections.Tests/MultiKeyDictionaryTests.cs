using System.Linq;
using System.Threading.Tasks;
using Halliburton.SpecializedCollections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Halliburton.IC.SpecializedCollections.Tests {
    [TestClass]
    public class MultiKeyDictionaryTests {
        private readonly (string, int, string)[] _items = new[] {
            ("Test1", 3, "Alice"),
            ("Test1", 4, "Bob"),
            ("Test2", 3, "Charles")
        };

        [TestMethod]
        public async Task TestAddItems() {
            var dict = new ReadWriteLockedMultiKeyDictionary<string, int, string>();
            await dict.AddOrUpdateMany(_items);
            var result1 = await dict.TryGetValue("Test1", 3);
            Assert.IsTrue(result1.Any("Alice"));
            Assert.IsFalse(await dict.TryAdd("Test1", 4, "Dave"));
        }

        [TestMethod]
        public async Task TestGetGrouping() {
            var dict = new ReadWriteLockedMultiKeyDictionary<string, int, string>();
            await dict.AddOrUpdateMany(_items);
            var group1 = await dict.GetGrouping("Test1");
            var group2 = await dict.GetGrouping("Test2");
            Assert.AreEqual(2, group1.Count);
            Assert.AreEqual(1, group2.Count);
            Assert.AreEqual("Alice", group1[3]);
            Assert.AreEqual("Bob", group1[4]);
            Assert.AreEqual("Charles", group2[3]);
        }
    }
}
