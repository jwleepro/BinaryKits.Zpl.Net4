using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using ZXing.Common;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Base clase for Barcode element drawers
    /// </summary>
    public abstract class BarcodeDrawerBase : ElementDrawerBase
    {
        /// <summary>
        /// Minimum acceptable magin between a barcode and its interpretation line, in pixels
        /// </summary>
        protected const float MIN_LABEL_MARGIN = 5f;

        protected void DrawBarcode(byte[] barcodeImageData, float x, float y, int barcodeWidth, int barcodeHeight, bool useFieldOrigin, Label.FieldOrientation fieldOrientation)
        {
            GraphicsState savedState = this.graphics.Save();
            try
            {
                Matrix matrix = GetRotationMatrix(x, y, barcodeWidth, barcodeHeight, useFieldOrigin, fieldOrientation);
                if (!useFieldOrigin)
                {
                    y -= barcodeHeight;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                if (matrix != null)
                {
                    this.graphics.MultiplyTransform(matrix);
                    matrix.Dispose();
                }

                using (MemoryStream ms = new MemoryStream(barcodeImageData))
                {
                    using (Bitmap bmp = new Bitmap(ms))
                    {
                        this.graphics.DrawImage(bmp, x, y);
                    }
                }
            }
            finally
            {
                this.graphics.Restore(savedState);
            }
        }

        protected void DrawInterpretationLine(string interpretation, Font font, float x, float y, int barcodeWidth, int barcodeHeight, bool useFieldOrigin, Label.FieldOrientation fieldOrientation, bool printInterpretationLineAboveCode, DrawerOptions options)
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
                float textWidth = textSize.Width;
                float textHeight = textSize.Height;

                x += (barcodeWidth - textWidth) / 2;
                if (!useFieldOrigin)
                {
                    y -= barcodeHeight;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                float margin = Math.Max((font.GetHeight(this.graphics) - textHeight) / 2, MIN_LABEL_MARGIN);
                using (SolidBrush brush = new SolidBrush(Color.Black))
                {
                    if (printInterpretationLineAboveCode)
                    {
                        this.graphics.DrawString(interpretation, font, brush, x, y - margin - textHeight);
                    }
                    else
                    {
                        this.graphics.DrawString(interpretation, font, brush, x, y + barcodeHeight + margin);
                    }
                }
            }
            finally
            {
                this.graphics.Restore(savedState);
            }
        }

        protected static Matrix GetRotationMatrix(float x, float y, int width, int height, bool useFieldOrigin, Label.FieldOrientation fieldOrientation)
        {
            Matrix matrix = null;
            if (useFieldOrigin)
            {
                switch (fieldOrientation)
                {
                    case Label.FieldOrientation.Rotated90:
                        matrix = new Matrix();
                        matrix.RotateAt(90, new PointF(x + height / 2f, y + height / 2f));
                        break;
                    case Label.FieldOrientation.Rotated180:
                        matrix = new Matrix();
                        matrix.RotateAt(180, new PointF(x + width / 2f, y + height / 2f));
                        break;
                    case Label.FieldOrientation.Rotated270:
                        matrix = new Matrix();
                        matrix.RotateAt(270, new PointF(x + width / 2f, y + width / 2f));
                        break;
                    case Label.FieldOrientation.Normal:
                        break;
                }
            }
            else
            {
                switch (fieldOrientation)
                {
                    case Label.FieldOrientation.Rotated90:
                        matrix = new Matrix();
                        matrix.RotateAt(90, new PointF(x, y));
                        break;
                    case Label.FieldOrientation.Rotated180:
                        matrix = new Matrix();
                        matrix.RotateAt(180, new PointF(x, y));
                        break;
                    case Label.FieldOrientation.Rotated270:
                        matrix = new Matrix();
                        matrix.RotateAt(270, new PointF(x, y));
                        break;
                    case Label.FieldOrientation.Normal:
                        break;
                }
            }

            return matrix;
        }

        protected static Bitmap BoolArrayToBitmap(bool[] array, int height, int moduleWidth = 1)
        {
            using (Bitmap image = new Bitmap(array.Length, 1, PixelFormat.Format32bppArgb))
            {
                for (int col = 0; col < array.Length; col++)
                {
                    Color color = array[col] ? Color.Black : Color.Transparent;
                    image.SetPixel(col, 0, color);
                }

                return ResizeBitmapNearestNeighbor(image, image.Width * moduleWidth, height);
            }
        }

        protected static Bitmap BoolArrayWithMaskToBitmap(bool[] array, bool[] mask, int height, int moduleWidth = 1)
        {
            using (Bitmap image = new Bitmap(array.Length, 1, PixelFormat.Format32bppArgb))
            {
                for (int col = 0; col < array.Length; col++)
                {
                    Color color = array[col] && mask[col] ? Color.Black : Color.Transparent;
                    image.SetPixel(col, 0, color);
                }

                return ResizeBitmapNearestNeighbor(image, image.Width * moduleWidth, height);
            }
        }

        protected static Bitmap BitMatrixToBitmap(BitMatrix matrix, int pixelScale)
        {
            using (Bitmap image = new Bitmap(matrix.Width, matrix.Height, PixelFormat.Format32bppArgb))
            {
                for (int row = 0; row < matrix.Height; row++)
                {
                    for (int col = 0; col < matrix.Width; col++)
                    {
                        Color color = matrix[col, row] ? Color.Black : Color.Transparent;
                        image.SetPixel(col, row, color);
                    }
                }

                return ResizeBitmapNearestNeighbor(image, image.Width * pixelScale, image.Height * pixelScale);
            }
        }

        protected static Bitmap ResizeBitmapNearestNeighbor(Bitmap source, int newWidth, int newHeight)
        {
            Bitmap result = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(source, 0, 0, newWidth, newHeight);
            }
            return result;
        }

        protected static bool[] AdjustWidths(bool[] array, int wide, int narrow)
        {
            List<bool> result = new List<bool>();
            bool last = true;
            int count = 0;
            foreach (bool current in array)
            {
                if (current != last)
                {
                    result.AddRange(Enumerable.Repeat(last, count == 1 ? narrow : wide));
                    last = current;
                    count = 0;
                }

                count += 1;
            }

            result.AddRange(Enumerable.Repeat(last, narrow));
            return result.ToArray();
        }
    }
}
