using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Ugit
{
    [TestClass]
    public class ExtensionTest
    {
        [TestMethod]
        public void Sha1HexDigestTest()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello World");
            string expected = "0a4d55a8d778e5022fab701977c5d840bbc486d0";
            Assert.AreEqual(expected, data.Sha1HexDigest());
        }
    }
}
