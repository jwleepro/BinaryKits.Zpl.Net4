using BinaryKits.Zpl.Label.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryKits.Zpl.Label.UnitTest
{
    [TestClass]
    public class ZebraZ64CompressionHelperTest
    {
        [TestMethod]
        public void CompressUncompress_Flow_Successful()
        {
            var originalData = "FFFFFFFFFFFFFFFFFFFF8000FFFF0000FFFF00018000FFFF0000FFFF00018000FFFF0000FFFF0001FFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFFFFFFFFFFFFFFFFFF";

            string compressed = ZebraZ64CompressionHelper.Compress(originalData);
            string uncompressed = ZebraZ64CompressionHelper.Uncompress(compressed).ToHexFromBytes();
            Assert.AreEqual(originalData, uncompressed);
        }
    }
}
