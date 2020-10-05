using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ugit
{
    [TestClass]
    public class DataTests
    {
        private Data data;
        
        public void Initialize()
        {
            data = new Data();
        }
        
        [TestMethod]
        public void HashObjectTest()
        {
            string foo = "foo";
            string expected = string.Empty;
        }
    }
}