using BinaryKits.Zpl.Label.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryKits.Zpl.Label.UnitTest
{
    [TestClass]
    public class ZebraACSCompressionHelperTest
    {
        [TestMethod]
        public void GetZebraCharCount_Simple_Successful()
        {
            var testData = new object[][]
            {
                //new object[] { 1, "G" },
                new object[] { 2, "H" },
                new object[] { 3, "I" },
                new object[] { 4, "J" },
                new object[] { 5, "K" },
                new object[] { 6, "L" },
                new object[] { 7, "M" },
                new object[] { 8, "N" },
                new object[] { 9, "O" },
                new object[] { 10, "P" },
                new object[] { 11, "Q" },
                new object[] { 12, "R" },
                new object[] { 13, "S" },
                new object[] { 14, "T" },
                new object[] { 15, "U" },
                new object[] { 16, "V" },
                new object[] { 17, "W" },
                new object[] { 18, "X" },
                new object[] { 19, "Y" },
                new object[] { 20, "g" },
                new object[] { 40, "h" },
                new object[] { 60, "i" },
                new object[] { 80, "j" },
                new object[] { 100, "k" },
                new object[] { 120, "l" },
                new object[] { 140, "m" },
                new object[] { 160, "n" },
                new object[] { 180, "o" },
                new object[] { 200, "p" },
                new object[] { 220, "q" },
                new object[] { 240, "r" },
                new object[] { 260, "s" },
                new object[] { 280, "t" },
                new object[] { 300, "u" },
                new object[] { 320, "v" },
                new object[] { 340, "w" },
                new object[] { 360, "x" },
                new object[] { 380, "y" },
                new object[] { 400, "z" },
            };

            foreach (var data in testData)
            {
                int charRepeatCount = (int)data[0];
                string compressed = (string)data[1];
                var zebraCharCount = ZebraACSCompressionHelper.GetZebraCharCount(charRepeatCount);
                Assert.AreEqual(compressed, zebraCharCount,
                    string.Format("Failed for charRepeatCount={0}", charRepeatCount));
            }
        }

        [TestMethod]
        public void GetZebraCharCount_Complex_Successful()
        {
            var testData = new object[][]
            {
                new object[] { 21, "gG" },
                new object[] { 22, "gH" },
                new object[] { 23, "gI" },
                new object[] { 24, "gJ" },
                new object[] { 25, "gK" },
                new object[] { 30, "gP" },
                new object[] { 35, "gU" },
                new object[] { 36, "gV" },
                new object[] { 37, "gW" },
                new object[] { 38, "gX" },
                new object[] { 39, "gY" },
                new object[] { 50, "hP" },
                new object[] { 642, "zrH" },
                new object[] { 800, "zz" },
                new object[] { 842, "zzhH" },
            };

            foreach (var data in testData)
            {
                int charRepeatCount = (int)data[0];
                string compressed = (string)data[1];
                var zebraCharCount = ZebraACSCompressionHelper.GetZebraCharCount(charRepeatCount);
                Assert.AreEqual(compressed, zebraCharCount,
                    string.Format("Failed for charRepeatCount={0}", charRepeatCount));
            }
        }

        [TestMethod]
        public void Compress_ValidData1_Successful()
        {
            var compressed = ZebraACSCompressionHelper.Compress("FFFF\nF00F\nFFFF\n", 2);
            Assert.AreEqual("JFFH0FJF", compressed);
        }

        [TestMethod]
        public void Compress_ValidData2_Successful()
        {
            var compressed = ZebraACSCompressionHelper.Compress("FFFFF00FFFFF", 2);
            Assert.AreEqual("JFFH0FJF", compressed);
        }

        [TestMethod]
        public void Compress_ValidData3_Successful()
        {
            var compressed = ZebraACSCompressionHelper.Compress("FFFFFFFFFFFFFFFFFFFF\n8000FFFF0000FFFF0001\n8000FFFF0000FFFF0001\n8000FFFF0000FFFF0001\nFFFF0000FFFF0000FFFF\nFFFF0000FFFF0000FFFF\nFFFF0000FFFF0000FFFF\nFFFFFFFFFFFFFFFFFFFF\n", 10);
            Assert.AreEqual("gF8I0JFJ0JFJ0JF::gF", compressed);
        }

        [TestMethod]
        public void CompressUncompress_Flow_Successful()
        {
            var originalData = "FFFFFFFFFFFFFFFFFFFF\n8000FFFF0000FFFF0001\n8000FFFF0000FFFF0001\n8000FFFF0000FFFF0001\nFFFF0000FFFF0000FFFF\nFFFF0000FFFF0000FFFF\nFFFF0000FFFF0000FFFF\nFFFFFFFFFFFFFFFFFFFF\n";

            var compressed = ZebraACSCompressionHelper.Compress(originalData, 10);
            var uncompressed = ZebraACSCompressionHelper.Uncompress(compressed, 10);
            Assert.AreEqual(originalData, uncompressed);
        }

        [TestMethod]
        public void Uncompress_ValidData1_Successful()
        {
            var compressedData = "gO0E\n";

            var uncompressed = ZebraACSCompressionHelper.Uncompress(compressedData, 10);
            Assert.AreEqual("00000000000000000000000000000E\n", uncompressed);
        }

        [TestMethod]
        public void Uncompress_ValidData2_Successful()
        {
            var compressedData = "gO0GE\n";

            var uncompressed = ZebraACSCompressionHelper.Uncompress(compressedData, 10);
            Assert.AreEqual("00000000000000000000000000000E\n", uncompressed);
        }
    }
}
