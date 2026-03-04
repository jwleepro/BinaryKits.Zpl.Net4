using BinaryKits.Zpl.Label.Helpers;
using BinaryKits.Zpl.Label.ImageConverters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace BinaryKits.Zpl.Label.Elements
{
    /// <summary>
    /// Download Graphics<br/>
    /// The ~DG command downloads an ASCII Hex representation of a graphic image.
    /// If .GRF is not the specified file extension, .GRF is automatically appended.
    /// </summary>
    /// <remarks>
    /// Format:~DGd:o.x,t,w,data
    /// d = device to store image
    /// o = image name
    /// x = extension
    /// t = total number of bytes in graphic
    /// w = number of bytes per row
    /// data = ASCII hexadecimal string defining image
    /// </remarks>
    public class ZplDownloadGraphics : ZplDownload
    {
        public string ImageName { get; private set; }
        private string _extension { get; set; }
        public byte[] ImageData { get; private set; }

        private readonly IImageConverter _imageConverter;
        readonly ZplCompressionScheme _compressionScheme;

        /// <summary>
        /// Zpl Download Graphics
        /// </summary>
        /// <param name="storageDevice"></param>
        /// <param name="imageName"></param>
        /// <param name="imageData"></param>
        /// <param name="imageConverter"></param>
        /// <param name="compressionScheme"></param>
        public ZplDownloadGraphics(
            char storageDevice,
            string imageName,
            byte[] imageData,
            ZplCompressionScheme compressionScheme = ZplCompressionScheme.ACS,
            IImageConverter imageConverter = default)
            : base(storageDevice)
        {
            if (imageName.Length > 8)
            {
                throw new ArgumentException("maximum length of 8 characters exceeded", "imageName");
            }

            _extension = "GRF"; //Fixed

            ImageName = imageName;
            ImageData = imageData;

            if (imageConverter == default)
            {
                imageConverter = new ImageSharpImageConverter();
            }
            _imageConverter = imageConverter;
            _compressionScheme = compressionScheme;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            byte[] objectData;
            using (MemoryStream imageStream = new MemoryStream(ImageData))
            using (Bitmap image = new Bitmap(imageStream))
            {
                if (context.ScaleFactor != 1)
                {
                    int newWidth = image.Width / 2;
                    int newHeight = image.Height / 2;
                    using (Bitmap resized = new Bitmap(newWidth, newHeight))
                    {
                        using (Graphics g = Graphics.FromImage(resized))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(image, 0, 0, newWidth, newHeight);
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            resized.Save(ms, ImageFormat.Png);
                            objectData = ms.ToArray();
                        }
                    }
                }
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Png);
                        objectData = ms.ToArray();
                    }
                }
            }

            ImageResult imageResult = _imageConverter.ConvertImage(objectData);
            string zplData = string.Empty;

            switch (_compressionScheme)
            {
                case ZplCompressionScheme.None:
                    zplData = imageResult.RawData.ToHexFromBytes();
                    break;
                case ZplCompressionScheme.ACS:
                    zplData = ZebraACSCompressionHelper.Compress(imageResult.RawData.ToHexFromBytes(), imageResult.BytesPerRow);
                    break;
                case ZplCompressionScheme.Z64:
                    zplData = ZebraZ64CompressionHelper.Compress(imageResult.RawData);
                    break;
                case ZplCompressionScheme.B64:
                    zplData = ZebraB64CompressionHelper.Compress(imageResult.RawData);
                    break;
            }

            List<string> result = new List<string>();
            result.Add(string.Format("~DG{0}:{1}.{2},{3},{4},", StorageDevice, ImageName, _extension, imageResult.BinaryByteCount, imageResult.BytesPerRow));
            result.Add(zplData);
            return result;
        }
    }
}
