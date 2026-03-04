using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("BinaryKits.Zpl.Label.UnitTest")]

namespace BinaryKits.Zpl.Label.Helpers
{
    /// <summary>
    /// Z64 Data Compression Scheme for ~DG and ~DB Commands First compresses the data using the LZ77 algorithm to
    /// reduce its size, then compressed data is then encoded using Base64 A CRC is calculated across the Base64-encoded
    /// data. If the CRC-check fails or the download is aborted, the object can be invalidated by the printer. reduces
    /// the actual number of data bytes and the amount of time required to download graphic images and bitmapped fonts
    /// with the ~DG and ~DB commands
    /// </summary>
    public static class ZebraZ64CompressionHelper
    {
        private static Regex _z64Regex = new Regex(":(Z64):(\\S+):([0-9a-fA-F]+)", RegexOptions.Compiled);

        public static string Compress(string hexData)
        {
            var cleanedHexData = hexData.Replace("\n", string.Empty).Replace("\r", string.Empty);
            return Compress(cleanedHexData.ToBytesFromHex());
        }

        public static string Compress(byte[] bytes)
        {
            var data = Deflate(bytes);
            var base64 = data.ToBase64();
            return ":Z64:" + base64 + ":" + Crc16.ComputeHex(base64.EncodeBytes());
        }

        public static byte[] Uncompress(string hexData)
        {
            var match = _z64Regex.Match(hexData);
            if (match.Success)
            {
                var imageBase64 = match.Groups[2].Value;
                var bytes = imageBase64.FromBase64();
                return Inflate(bytes);
            }
            else
            {
                throw new FormatException("Hex string not in Z64 format");
            }
        }

        /// <summary>
        /// Decompress graphics data with ZLib headers. .NET 4.0 has no ZlibStream implementation. Need to use
        /// DeflateStream and write header and checksum.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static byte[] Inflate(byte[] data)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var inputStream = new MemoryStream())
                {
                    //skip first 2 bytes of headers and last 4 bytes of checksum.
                    inputStream.Write(data, 2, data.Length - 6);
                    inputStream.Position = 0;
                    using (var decompressor = new DeflateStream(inputStream, CompressionMode.Decompress, true))
                    {
                        CopyStream(decompressor, outputStream);
                        return outputStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Compress graphics data with ZLib headers. .NET 4.0 has no ZlibStream implementation.
        /// Need to use DeflateStream and write header and checksum.
        /// Cleaned up implementation based on https://yal.cc/cs-deflatestream-zlib/
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static byte[] Deflate(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                // write header:
                ms.WriteByte(0x78);
                // compression level header (Optimal)
                ms.WriteByte(0x9C);

                // write compressed data (with Deflate headers):
                // .NET 4.0 DeflateStream only supports CompressionMode, not CompressionLevel
                var compressor = new DeflateStream(ms, CompressionMode.Compress, true);
                compressor.Write(data, 0, data.Length);
                compressor.Close();

                // compute Adler-32 checksum
                uint a1 = 1, a2 = 0;
                foreach (byte b in data)
                {
                    a1 = (a1 + b) % 65521;
                    a2 = (a2 + a1) % 65521;
                }

                //append checksum
                ms.WriteByte((byte)(a2 >> 8));
                ms.WriteByte((byte)a2);
                ms.WriteByte((byte)(a1 >> 8));
                ms.WriteByte((byte)a1);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Stream.CopyTo is not available in .NET 4.0, so we implement our own.
        /// </summary>
        private static void CopyStream(Stream source, Stream destination)
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
            }
        }
    }
}
