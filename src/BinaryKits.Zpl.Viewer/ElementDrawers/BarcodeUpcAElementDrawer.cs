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
    public class BarcodeUpcAElementDrawer : BarcodeDrawerBase
    {
        private static readonly bool[] guards = new bool[95];

        static BarcodeUpcAElementDrawer()
        {
            int[] guardIndicies = new int[]
            {
                0, 2,
                4, 5, 6, 7, 8, 9,
                46, 48,
                85, 86, 87, 88, 89, 90,
                92, 94
            };

            foreach (int idx in guardIndicies)
            {
                guards[idx] = true;
            }
        }

        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplBarcodeUpcA;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplBarcodeUpcA barcode)
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

                content = content.PadLeft(11, '0').Substring(0, 11);
                string interpretation = content;

                if (barcode.PrintCheckDigit)
                {
                    int checksum = 0;
                    for (int i = 0; i < 11; i++)
                    {
                        checksum += (content[i] - 48) * (i % 2 * 2 + 7);
                    }

                    interpretation = string.Format("{0}{1}", interpretation, checksum % 10);
                }


                EAN13Writer writer = new EAN13Writer();
                bool[] result = writer.encode(content.PadLeft(12, '0'));
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
                                this.DrawUpcAInterpretationLine(result, interpretation, labelFont, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, barcode.ModuleWidth, options);
                            }
                        }
                    }

                    return this.CalculateNextDefaultPosition(x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        private void DrawUpcAInterpretationLine(
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

                using (Bitmap guardImage = BoolArrayWithMaskToBitmap(data, guards, (int)(margin + textHeight / 2), moduleWidth))
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

                        if (i == 0 || i == 10)
                        {
                            x += moduleWidth * 11;
                        }
                        else if (i == 5)
                        {
                            x += moduleWidth * 4;
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
