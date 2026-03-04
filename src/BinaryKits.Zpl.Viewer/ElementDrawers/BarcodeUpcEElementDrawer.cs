using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using ZXing.OneD;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    public class BarcodeUpcEElementDrawer : BarcodeDrawerBase
    {
        private static readonly bool[] guards = new bool[51];

        static BarcodeUpcEElementDrawer()
        {
            foreach (int idx in new int[] { 0, 2, 46, 48, 50 })
            {
                guards[idx] = true;
            }
        }

        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplBarcodeUpcE;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplBarcodeUpcE barcode)
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

                // [S]DDDDDD[C]
                if (content.Length < 7)
                {
                    // number system 0
                    content = content.PadLeft(7, '0');
                }
                else if (content.Length <= 8)
                {
                    // ignore user provided checksum
                    content = content.Substring(0, 7);
                }
                else
                {
                    // UPC-A to UPC-E
                    string numberSystem = "0";
                    content = content.PadRight(10, '0');
                    if (content.Length > 10)
                    {
                        numberSystem = content.Substring(0, 1);
                        content = content.Substring(1, 10);
                    }

                    int manufacturer = int.Parse(content.Substring(0, 5));
                    int product = int.Parse(content.Substring(5, 5));

                    if (manufacturer % 100 == 0)
                    {
                        int trail = manufacturer / 100 % 10;
                        if (trail <= 2)
                        {
                            content = $"{numberSystem}{manufacturer / 1000:D2}{product % 1000:D3}{trail}";
                        }
                        else
                        {
                            content = $"{numberSystem}{manufacturer / 100:D3}{product % 100:D2}{3}";
                        }
                    }
                    else if (manufacturer % 10 == 0)
                    {
                        content = $"{numberSystem}{manufacturer / 10:D4}{product % 10:D1}{4}";
                    }
                    else
                    {
                        content = $"{numberSystem}{manufacturer:D5}{Math.Max(product % 10, 5):D1}";
                    }
                }

                string interpretation = content;

                if (barcode.PrintCheckDigit)
                {
                    string expanded = UPCEReader.convertUPCEtoUPCA(content);
                    int checksum = 0;
                    for (int i = 0; i < 11; i++)
                    {
                        checksum += (expanded[i] - 48) * (i % 2 * 2 + 7);
                    }

                    interpretation = string.Format("{0}{1}", interpretation, checksum % 10);
                }

                UPCEWriter writer = new UPCEWriter();
                bool[] result = writer.encode(content);
                using (Bitmap resizedImage = BoolArrayToBitmap(result, barcode.Height, barcode.ModuleWidth))
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
                        Font labelTypeFace = options.FontManager.FontLoader("A");
                        using (Font labelFont = new Font(labelTypeFace.FontFamily, labelFontSize, labelTypeFace.Style))
                        {
                            if (barcode.PrintInterpretationLineAboveCode)
                            {
                                this.DrawInterpretationLine(interpretation, labelFont, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, true, options);
                            }
                            else
                            {
                                this.DrawUpcEInterpretationLine(result, interpretation, labelFont, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, barcode.ModuleWidth, options);
                            }
                        }
                    }

                    return this.CalculateNextDefaultPosition(x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        private void DrawUpcEInterpretationLine(
            bool[] data,
            string interpretation,
            Font font,
            float x,
            float y,
            int barcodeWidth,
            int barcodeHeight,
            bool useFieldOrigin,
            FieldOrientation fieldOrientation,
            int moduleWidth,
            DrawerOptions options)
        {
            GraphicsState savedState = this.graphics.Save();
            try
            {
                Matrix matrix = GetRotationMatrix(x, y, barcodeWidth, barcodeHeight, useFieldOrigin, fieldOrientation);
                if (matrix != null)
                {
                    this.graphics.MultiplyTransform(matrix);
                    matrix.Dispose();
                }

                SizeF textSize = this.graphics.MeasureString(interpretation, font);
                float textHeight = textSize.Height;

                if (!useFieldOrigin)
                {
                    y -= barcodeHeight;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                float margin = Math.Max((font.GetHeight(this.graphics) - textHeight) / 2, MIN_LABEL_MARGIN);
                int spacing = moduleWidth * 7;

                using (Bitmap guardImage = BoolArrayToBitmap(guards, (int)(margin + textHeight / 2), moduleWidth))
                {
                    this.graphics.DrawImage(guardImage, x, y + barcodeHeight);
                }

                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    for (int i = 0; i < interpretation.Length; i++)
                    {
                        string digit = interpretation[i].ToString();
                        SizeF digitSize = this.graphics.MeasureString(digit, font);
                        this.graphics.DrawString(digit, font, brush, x - (spacing + digitSize.Width) / 2 - moduleWidth, y + barcodeHeight + textHeight + margin);
                        x += spacing;

                        if (i == 0)
                        {
                            x += moduleWidth * 4;
                        }
                        else if (i == 6)
                        {
                            x += moduleWidth * 6;
                        }
                    }
                }
            }
            finally
            {
                this.graphics.Restore(savedState);
            }
        }

    }
}
