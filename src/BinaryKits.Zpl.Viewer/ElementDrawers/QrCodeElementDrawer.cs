using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for QR Code Barcode elements
    /// </summary>
    public class QrCodeElementDrawer : BarcodeDrawerBase
    {
        private static readonly Regex gs1Regex = new Regex(@"^>;>8(.+)$", RegexOptions.Compiled);

        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplQrCode;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplQrCode qrcode)
            {
                float x = qrcode.PositionX;
                float y = qrcode.PositionY;

                if (qrcode.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                string content = qrcode.Content;
                if (qrcode.HexadecimalIndicator is char hexIndicator)
                {
                    content = content.ReplaceHexEscapes(hexIndicator, internationalFont);
                }

                // support hand-rolled GS1
                bool gs1Mode = false;
                Match gs1Match = gs1Regex.Match(content);
                if (gs1Match.Success)
                {
                    content = gs1Match.Groups[1].Value;
                    gs1Mode = true;
                }

                int verticalQuietZone = qrcode.VerticalQuietZone;

                QRCodeWriter writer = new QRCodeWriter();
                QrCodeEncodingOptions encodingOptions = new QrCodeEncodingOptions()
                {
                    ErrorCorrection = ConvertErrorCorrection(qrcode.ErrorCorrectionLevel),
                    CharacterSet = "UTF-8",
                    Margin = 0,
                    GS1Format = gs1Mode
                };
                BitMatrix result = writer.encode(content, BarcodeFormat.QR_CODE, 0, 0, encodingOptions.Hints);

                using (Bitmap resizedImage = BitMatrixToBitmap(result, qrcode.MagnificationFactor))
                {
                    byte[] png;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resizedImage.Save(ms, ImageFormat.Png);
                        png = ms.ToArray();
                    }

                    this.DrawBarcode(png, x, y + verticalQuietZone, resizedImage.Width, resizedImage.Height + 2 * verticalQuietZone, qrcode.FieldOrigin != null, qrcode.FieldOrientation);
                    return this.CalculateNextDefaultPosition(x, y, resizedImage.Width, resizedImage.Height + 2 * verticalQuietZone, qrcode.FieldOrigin != null, qrcode.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        private static ZXing.QrCode.Internal.ErrorCorrectionLevel ConvertErrorCorrection(ErrorCorrectionLevel errorCorrectionLevel)
        {
            switch (errorCorrectionLevel)
            {
                case ErrorCorrectionLevel.UltraHighReliability:
                    return ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
                case ErrorCorrectionLevel.HighReliability:
                    return ZXing.QrCode.Internal.ErrorCorrectionLevel.Q;
                case ErrorCorrectionLevel.Standard:
                    return ZXing.QrCode.Internal.ErrorCorrectionLevel.M;
                case ErrorCorrectionLevel.HighDensity:
                    return ZXing.QrCode.Internal.ErrorCorrectionLevel.L;
                default:
                    return ZXing.QrCode.Internal.ErrorCorrectionLevel.M;
            }
        }
    }
}
