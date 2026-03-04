using BinaryKits.Zpl.Label.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace BinaryKits.Zpl.Label.Elements
{
    /// <summary>
    /// Download Graphics / Native TrueType or OpenType Font
    /// The ~DY command downloads to the printer graphic objects or fonts in any supported format.
    /// This command can be used in place of ~DG for more saving and loading options.
    /// ~DY is the preferred command to download TrueType fonts on printers with firmware greater than X.13.
    /// It is faster than ~DU.
    /// </summary>
    /// <remarks>
    /// Format:~DYd:f,b,x,t,w,data
    /// d = file location
    /// f = file name
    /// b = format downloaded in data field
    /// x = extension of stored file
    /// t = total number of bytes in file
    /// w = total number of bytes per row
    /// data = data
    /// </remarks>
    public class ZplDownloadObjects : ZplDownload
    {
        public string ObjectName { get; private set; }
        public byte[] ImageData { get; private set; }

        public ZplDownloadObjects(char storageDevice, string imageName, byte[] imageData)
            : base(storageDevice)
        {
            ObjectName = imageName;
            ImageData = imageData;
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
                    int scaleWidth = (int)Math.Round(image.Width * context.ScaleFactor);
                    int scaleHeight = (int)Math.Round(image.Height * context.ScaleFactor);

                    using (Bitmap resized = new Bitmap(scaleWidth, scaleHeight))
                    {
                        using (Graphics g = Graphics.FromImage(resized))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(image, 0, 0, scaleWidth, scaleHeight);
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

            string hexString = ByteHelper.BytesToHex(objectData);

            char formatDownloadedInDataField = 'P'; //portable network graphic (.PNG) - ZB64 encoded
            char extensionOfStoredFile = 'P'; //store as compressed (.PNG)

            List<string> result = new List<string>();
            result.Add(string.Format("~DY{0}:{1},{2},{3},{4},,{5}", StorageDevice, ObjectName, formatDownloadedInDataField, extensionOfStoredFile, objectData.Length, hexString));
            return result;
        }
    }
}
