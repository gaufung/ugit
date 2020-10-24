using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ugit
{
    [TestClass]
    public class DiffTests
    {

        private IDiff diff;

        [TestInitialize]
        public void Init()
        {
            diff = new Diff();
        }

        [TestMethod]
        public void CompareTreesTest()
        {
            Dictionary<string, string> fromTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Dictionary<string, string> toTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo1" },
                { "world.txt", "bar" },
            };

            (string, IEnumerable<string>)[] expected = new (string, IEnumerable<string>)[]
            {
                ("hello.txt", new string[2]{"foo", "foo1"}),
                ("ugit.txt", new string[2]{"baz", null}),
                ("world.txt", new string[2]{"bar", "bar"}),
            };
            (string, IEnumerable<string>)[] actual = diff.CompareTrees(fromTree, toTree).OrderBy(i=>i.Item1).ToArray();
            for(int i = 0; i < 2; i++)
            {
                Assert.AreEqual(expected[i].Item1, actual[i].Item1);
                CollectionAssert.AreEqual(expected[i].Item2.ToArray(), actual[i].Item2.ToArray());
            }
        }

        [TestMethod]
        public void DiffTreeTest()
        {
            Dictionary<string, string> fromTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Dictionary<string, string> toTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo1" },
                { "world.txt", "bar" },
            };

            string exepcted = string.Join("\n", new string[]
            {
                "changed: hello.txt",
                "changed: ugit.txt",
            });
            Assert.AreEqual(exepcted, diff.DiffTree(fromTree, toTree));
        }
    }
}
