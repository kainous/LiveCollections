using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Monadic;

namespace SpecializedCollections.Tests {
    [TestClass]
    public class OptionTests {
        private readonly Dictionary<string, int> _dictionary = new Dictionary<string, int> {
            { "Test1", 5 },
            { "Test2", 6 }
        };

        [TestMethod]
        public void TestTryGet() {
            var result = _dictionary.TryGetValue("Test1");
            Assert.IsInstanceOfType(result, typeof(Some<int>));
            switch (result) {
                case Some<int> some:
                    Assert.AreEqual(5, some.Value);
                    break;
                default:
                    Assert.Fail();
                    break;
            }
        }

        [TestMethod]
        public void TestOptionSelectManySome() {
            var result = from first in _dictionary.TryGetValue("Test1")
                         from second in _dictionary.TryGetValue("Test2")
                         select first + second;

            result.ForEach(a => Assert.AreEqual(11, a));
            Assert.IsTrue(result.Contains(11));
        }

        [TestMethod]
        public void TestOptionSelectManyNone1() {
            var result = from first in _dictionary.TryGetValue("a")
                         from second in _dictionary.TryGetValue("Test2")
                         select first + second;

            result.ForEach(a => Assert.Fail());
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void TestOptionSelectManyNone2() {
            var result = from first in _dictionary.TryGetValue("Test1")
                         from second in _dictionary.TryGetValue("b")
                         select first + second;

            result.ForEach(a => Assert.AreEqual(11, a));
            Assert.IsFalse(result.Contains(11));
        }
    }
}
