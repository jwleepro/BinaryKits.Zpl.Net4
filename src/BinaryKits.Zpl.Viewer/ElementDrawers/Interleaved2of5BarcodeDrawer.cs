using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using ZXing.OneD;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Interleaved 2 of 5 Barcode elements
    /// </summary>
    public class Interleaved2of5BarcodeDrawer : BarcodeDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplBarcodeInterleaved2of5;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplBarcodeInterleaved2of5 barcode)
            {
                float x = barcode.PositionX;
                float y = barcode.PositionY;

                if (barcode.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                string content = barcode.Content;
                if (barcode.HexadecimalIndicator is char hexIndicator)
                {
                    content = content.ReplaceHexEscapes(hexIndicator, internationalFont);
                }

                if (barcode.Mod10CheckDigit)
                {
                    int sum = 0;

                    for (int i = 0; i < content.Length; i++)
                    {
                        if (!char.IsDigit(content[i]))
                        {
                            return currentPosition;
                        }

                        int digit = content[i] - '0';
                        int weight = ((content.Length - i) % 2 == 0) ? 3 : 1;
                        sum += digit * weight;
                    }

                    int checkDigit = (10 - (sum % 10)) % 10;
                    content = $"{content}{checkDigit}";
                }

                if (content.Length % 2 != 0)
                {
                    content = $"0{content}";
                }

                ITFWriter writer = new ITFWriter();
                bool[] result = writer.encode(content);
                int narrow = barcode.ModuleWidth;
                int wide = (int)Math.Floor(barcode.WideBarToNarrowBarWidthRatio * narrow);
                result = AdjustWidths(result, wide, narrow);
                using (Bitmap resizedImage = BoolArrayToBitmap(result, barcode.Height))
                {
                    byte[] png;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        resizedImage.Save(ms, ImageFormat.Png);
                        png = ms.ToArray();
                    }

                    this.DrawBarcode(png, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation);

                    if (barcode.PrintInterpretationLine)
                    {
                        float labelFontSize = FontScale.GetBitmappedFontSize("A", Math.Min(barcode.ModuleWidth, 10), printDensityDpmm).Value;
                        Font labelTypeface = options.FontManager.FontLoader("A");
                        using (Font labelFont = new Font(labelTypeface.FontFamily, labelFontSize, labelTypeface.Style))
                        {
                            this.DrawInterpretationLine(content, labelFont, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, barcode.PrintInterpretationLineAboveCode, options);
                        }
                    }

                    return this.CalculateNextDefaultPosition(x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }
    }
}
