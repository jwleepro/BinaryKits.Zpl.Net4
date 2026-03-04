using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Graphic Symbol elements
    /// </summary>
    public class GraphicSymbolElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element.GetType() == typeof(ZplGraphicSymbol);
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplGraphicSymbol graphicSymbol)
            {
                float x = graphicSymbol.PositionX;
                float y = graphicSymbol.PositionY;
                FieldJustification fieldJustification = FieldJustification.None;

                if (graphicSymbol.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                (float fontSize, float scaleX) = FontScale.GetFontScaling("GS", graphicSymbol.Height, graphicSymbol.Width, printDensityDpmm);

                // remove incorrect scaling
                fontSize /= 1.1f;

                Font loadedFont = options.FontManager.FontGS;
                using (Font textFont = new Font(loadedFont.FontFamily, fontSize * 1.25f, loadedFont.Style))
                {
                    string displayText = string.Format("{0}", (char?)graphicSymbol.Character);
                    SizeF textBounds = this.graphics.MeasureString(displayText, textFont);
                    float totalWidth = textBounds.Width * scaleX;

                    if (graphicSymbol.FieldOrigin != null)
                    {
                        fieldJustification = graphicSymbol.FieldOrigin.FieldJustification;
                    }
                    else if (graphicSymbol.FieldTypeset != null)
                    {
                        fieldJustification = graphicSymbol.FieldTypeset.FieldJustification;
                    }

                    GraphicsState savedState = this.graphics.Save();
                    try
                    {
                        bool useFieldOrigin = graphicSymbol.FieldOrigin != null;
                        Matrix rotationMatrix = GetRotationMatrix(x, y, textBounds.Width, textBounds.Height, useFieldOrigin, graphicSymbol.FieldOrientation);
                        if (rotationMatrix != null)
                        {
                            this.graphics.MultiplyTransform(rotationMatrix);
                            rotationMatrix.Dispose();
                        }

                        if (Math.Abs(scaleX - 1f) > 0.0001f)
                        {
                            Matrix scaleMatrix = new Matrix();
                            scaleMatrix.Translate(x, y, MatrixOrder.Append);
                            scaleMatrix.Scale(scaleX, 1f, MatrixOrder.Append);
                            scaleMatrix.Translate(-x, -y, MatrixOrder.Append);
                            this.graphics.MultiplyTransform(scaleMatrix);
                            scaleMatrix.Dispose();
                        }

                        float drawX = x;
                        if (fieldJustification == FieldJustification.Right)
                        {
                            drawX = x - textBounds.Width;
                        }

                        using (SolidBrush brush = new SolidBrush(Color.Black))
                        {
                            this.graphics.DrawString(displayText, textFont, brush, drawX, y);
                        }
                    }
                    finally
                    {
                        this.graphics.Restore(savedState);
                    }

                    // Update the next default field position after rendering
                    return this.CalculateNextDefaultPosition(x, y, totalWidth, textBounds.Height, false, graphicSymbol.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        private static Matrix GetRotationMatrix(float x, float y, float width, float height, bool useFieldOrigin, FieldOrientation fieldOrientation)
        {
            Matrix matrix = null;
            if (useFieldOrigin)
            {
                switch (fieldOrientation)
                {
                    case FieldOrientation.Rotated90:
                        matrix = new Matrix();
                        matrix.RotateAt(90, new PointF(x + height / 2f, y + height / 2f));
                        break;
                    case FieldOrientation.Rotated180:
                        matrix = new Matrix();
                        matrix.RotateAt(180, new PointF(x + width / 2f, y + height / 2f));
                        break;
                    case FieldOrientation.Rotated270:
                        matrix = new Matrix();
                        matrix.RotateAt(270, new PointF(x + width / 2f, y + width / 2f));
                        break;
                    case FieldOrientation.Normal:
                        break;
                }
            }
            else
            {
                switch (fieldOrientation)
                {
                    case FieldOrientation.Rotated90:
                        matrix = new Matrix();
                        matrix.RotateAt(90, new PointF(x, y));
                        break;
                    case FieldOrientation.Rotated180:
                        matrix = new Matrix();
                        matrix.RotateAt(180, new PointF(x, y));
                        break;
                    case FieldOrientation.Rotated270:
                        matrix = new Matrix();
                        matrix.RotateAt(270, new PointF(x, y));
                        break;
                    case FieldOrientation.Normal:
                        break;
                }
            }

            return matrix;
        }
    }
}
