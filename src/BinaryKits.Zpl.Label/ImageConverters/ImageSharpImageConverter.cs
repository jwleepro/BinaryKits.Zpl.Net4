using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace BinaryKits.Zpl.Label.ImageConverters
{
    public class ImageSharpImageConverter : IImageConverter
    {
        /// <summary>
        /// Convert image to bitonal image (grf)
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public ImageResult ConvertImage(byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData.Length))
            {
                using (MemoryStream imageStream = new MemoryStream(imageData))
                using (Bitmap image = new Bitmap(imageStream))
                {
                    int bytesPerRow = image.Width % 8 > 0
                        ? image.Width / 8 + 1
                        : image.Width / 8;

                    int binaryByteCount = image.Height * bytesPerRow;

                    int colorBits = 0;
                    int j = 0;

                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            Color pixel = image.GetPixel(x, y);

                            bool isBlackPixel = ((pixel.R + pixel.G + pixel.B) / 3) < 128;
                            if (isBlackPixel)
                            {
                                colorBits |= 1 << (7 - j);
                            }

                            j++;

                            if (j == 8 || x == (image.Width - 1))
                            {
                                ms.WriteByte((byte)colorBits);
                                colorBits = 0;
                                j = 0;
                            }
                        }
                    }

                    return new ImageResult
                    {
                        RawData = ms.ToArray(),
                        BinaryByteCount = binaryByteCount,
                        BytesPerRow = bytesPerRow
                    };
                }
            }
        }

        private byte Reverse(byte b)
        {
            int reverse = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & (1 << i)) != 0)
                {
                    reverse |= 1 << (7 - i);
                }
            }
            return (byte)reverse;
        }

        /// <summary>
        /// Convert from bitonal image (grf) to png image
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="bytesPerRow"></param>
        /// <returns></returns>
        public byte[] ConvertImage(byte[] imageData, int bytesPerRow)
        {
            imageData = imageData.Select(b => Reverse(b)).ToArray();

            int imageHeight = imageData.Length / bytesPerRow;
            int imageWidth = bytesPerRow * 8;

            using (Bitmap image = new Bitmap(imageWidth, imageHeight))
            {
                for (int y = 0; y < image.Height; y++)
                {
                    BitArray bits = new BitArray(imageData.Skip(bytesPerRow * y).Take(bytesPerRow).ToArray());

                    for (int x = 0; x < image.Width; x++)
                    {
                        if (bits[x])
                        {
                            image.SetPixel(x, y, Color.Black);
                        }
                    }
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, ImageFormat.Png);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
