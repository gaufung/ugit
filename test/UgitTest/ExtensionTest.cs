using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Ugit
{
    [TestClass]
    public class ExtensionTest
    {
        [TestMethod]
        public void Sha1HexDigestTest()
        {
            byte[] data = "Hello World".Encode();
            string expected = "0a4d55a8d778e5022fab701977c5d840bbc486d0";
            Assert.AreEqual(expected, data.Sha1HexDigest());
        }

        [TestMethod]
        public void EncodeTest()
        {
            string str = "Hello World";
            byte[] expected = new byte[] { 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100 };
            CollectionAssert.AreEqual(expected, str.Encode());
        }

        [TestMethod]
        public void DecodeTest()
        {
            byte[] data = new byte[] { 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100 };
            string expected = "Hello World";
            Assert.AreEqual(expected, data.Decode());
        }
    }
}
