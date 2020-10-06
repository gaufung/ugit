using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ugit
{
    [TestClass]
    public class ExtensionsTests
    {
        [TestMethod]
        public void EncodeTest()
        {
            string foo = "foo";
            byte[] expected = new[] {byte.Parse("102"), byte.Parse("111"), byte.Parse("111")};
            CollectionAssert.AreEqual(expected, foo.Encode());
        }

        [TestMethod]
        public void DecodeTest()
        {
            byte[] arr = new[] {byte.Parse("102"), byte.Parse("111"), byte.Parse("111")};
            string expect = "foo";
            Assert.AreEqual(expect, arr.Decode());
        }

        [TestMethod]
        public void Sha1HexDigestTest()
        {
            string foo = "foo";
            string expect = "0beec7b5ea3f0fdbc95d0dd47f3c5bc275da8a33";
            Assert.AreEqual(expect, foo.Encode().Sha1HexDigest());
        }

        [TestMethod]
        public void TestIndex()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["hello"] = "world";
            var str = JsonSerializer.Serialize(dic);
            Assert.AreEqual("", str);
        }
            
    }
}