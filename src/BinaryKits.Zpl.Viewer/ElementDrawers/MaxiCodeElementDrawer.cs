using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;
using BinaryKits.Zpl.Viewer.Symologies;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for MaxiCode Barcode Elements.
    /// </summary>
    public class MaxiCodeElementDrawer : BarcodeDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplMaxiCode;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplMaxiCode maxiCode)
            {
                float x = maxiCode.PositionX;
                float y = maxiCode.PositionY;

                if (maxiCode.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                string content = maxiCode.Content;
                if (maxiCode.HexadecimalIndicator is char hexIndicator)
                {
                    content = content.ReplaceHexEscapes(hexIndicator, internationalFont);
                }

                bool[] data = MaxiCodeSymbology.Encode(content, maxiCode.Mode);

                using (Bitmap image = DrawMaxiCode(data, printDensityDpmm))
                {
                    byte[] png;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Png);
                        png = ms.ToArray();
                    }

                    this.DrawBarcode(png, x, y, image.Width, image.Height, maxiCode.FieldOrigin != null, maxiCode.FieldOrientation);
                    return this.CalculateNextDefaultPosition(x, y, image.Width, image.Height, maxiCode.FieldOrigin != null, maxiCode.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match documentation.", Scope = "member")]
        private static Bitmap DrawMaxiCode(bool[] data, int dpmm)
        {
            // ISO/IEC 16023:2000 pp. 16, 38-40
            // fundamental dimensions
            float L;
            float H;
            float W;
            float V;
            float X;
            float Y;

            // gutters
            float gX;
            float gV;

            // dark hex pattern (relative line segments)
            PointF[] pattern;
            float xoff;
            float yoff;

            if (dpmm == 8)
            {
                W = 7;
                V = 8;
                X = W;
                Y = 6;

                gX = 1;
                gV = 1;

                xoff = 4f;
                yoff = 2f;
                pattern = new PointF[]
                {
                    new PointF(0f, 3f),
                    new PointF(2f, 2f),
                    new PointF(1.5f, 0f),
                    new PointF(2f, -2f),
                    new PointF(0f, -3f),
                    new PointF(-2f, -2f),
                    new PointF(-1.5f, 0f)
                };

                L = 29 * W;
                H = 32 * Y;
            }
            else if (dpmm == 12)
            {
                W = 10;
                V = 12;
                X = W;
                Y = 9;

                gX = 2;
                gV = 2;

                xoff = 5f;
                yoff = 3f;
                pattern = new PointF[]
                {
                    new PointF(0f, 4f),
                    new PointF(3f, 3f),
                    new PointF(1.5f, 0f),
                    new PointF(3f, -3f),
                    new PointF(0f, -4f),
                    new PointF(-3f, -3f),
                    new PointF(-1.5f, 0f)
                };

                L = 29 * W;
                H = 32 * Y;
            }
            else
            {
                L = 25.50f * dpmm;

                W = L / 29;
                V = 1.1547f * W; // (2/Math.Sqrt(3)) * W
                X = W;
                Y = 0.866f * W; // (Math.Sqrt(3)/2) * W

                H = 32 * Y;

                gX = dpmm / 6f;
                gV = 1.1547f * gX;

                xoff = W / 2;
                yoff = (V - gV) / 4;

                // drawn hexagon dimensions
                float hexW = (X - gX) / 2; // half width
                float hexH = (V - gV) / 4; // quarter height

                pattern = new PointF[]
                {
                    new PointF(0f, hexH * 2),
                    new PointF(hexW, hexH),
                    new PointF(hexW, -hexH),
                    new PointF(0f, -hexH * 2),
                    new PointF(-hexW, -hexH)
                };
            }

            // finder radii
            float R1 = 0.51f * dpmm;
            float R2 = 1.18f * dpmm;
            float R3 = 1.86f * dpmm;
            float R4 = 2.53f * dpmm;
            float R5 = 3.20f * dpmm;
            float R6 = 3.87f * dpmm;

            Bitmap image = new Bitmap((int)Math.Ceiling(L + X - gX), (int)Math.Ceiling(H + V - gV), PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.Clear(Color.Transparent);

                using (GraphicsPath path = new GraphicsPath(FillMode.Alternate))
                {
                    int dataIndex = 0;
                    for (int j = 0; j < 33; j++)
                    {
                        int columns = 30 - j % 2;
                        for (int i = 0; i < columns; i++)
                        {
                            if (dataIndex >= data.Length)
                            {
                                break;
                            }

                            if (data[dataIndex])
                            {
                                float startX = i * W + j % 2 * xoff;
                                float startY = j * Y + yoff;
                                path.AddPolygon(CreatePolygon(startX, startY, pattern));
                            }

                            dataIndex++;
                        }
                    }

                    float finderX = 14 * W + (X - gX) / 2;
                    float finderY = 16 * Y + (V - gV) / 2;

                    AddFinderRing(path, finderX, finderY, R2, R1);
                    AddFinderRing(path, finderX, finderY, R4, R3);
                    AddFinderRing(path, finderX, finderY, R6, R5);

                    using (SolidBrush brush = new SolidBrush(Color.Black))
                    {
                        graphics.FillPath(brush, path);
                    }
                }
            }

            return image;
        }

        private static PointF[] CreatePolygon(float startX, float startY, PointF[] relativeSegments)
        {
            List<PointF> points = new List<PointF>();
            float currentX = startX;
            float currentY = startY;
            points.Add(new PointF(currentX, currentY));

            for (int i = 0; i < relativeSegments.Length; i++)
            {
                currentX += relativeSegments[i].X;
                currentY += relativeSegments[i].Y;
                points.Add(new PointF(currentX, currentY));
            }

            return points.ToArray();
        }

        private static void AddFinderRing(GraphicsPath path, float centerX, float centerY, float outerRadius, float innerRadius)
        {
            path.AddEllipse(centerX - outerRadius, centerY - outerRadius, outerRadius * 2, outerRadius * 2);
            path.AddEllipse(centerX - innerRadius, centerY - innerRadius, innerRadius * 2, innerRadius * 2);
        }
    }
}
