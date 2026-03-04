using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Text Field elements
    /// </summary>
    public class TextFieldElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element.GetType() == typeof(ZplTextField);
        }

        ///<inheritdoc/>
        public override bool IsReverseDraw(ZplElementBase element)
        {
            if (element is ZplTextField textField)
            {
                return textField.ReversePrint;
            }

            return false;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplTextField textField)
            {
                float x = textField.PositionX;
                float y = textField.PositionY;
                FieldJustification fieldJustification = FieldJustification.None;

                if (textField.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                if (textField.FieldOrigin != null)
                {
                    fieldJustification = textField.FieldOrigin.FieldJustification;
                }
                else if (textField.FieldTypeset != null)
                {
                    fieldJustification = textField.FieldTypeset.FieldJustification;
                }

                ZplFont font = textField.Font;
                (float fontSize, float scaleX) = FontScale.GetFontScaling(font.FontName, font.FontHeight, font.FontWidth, printDensityDpmm);

                Font loadedFont = options.FontManager.FontLoader(font.FontName);
                using (Font textFont = new Font(loadedFont.FontFamily, fontSize, loadedFont.Style))
                {
                    string displayText = textField.Text;
                    if (textField.HexadecimalIndicator is char hexIndicator)
                    {
                        displayText = displayText.ReplaceHexEscapes(hexIndicator, internationalFont);
                    }

                    if (font.FontName == "0")
                    {
                        if (options.ReplaceDashWithEnDash)
                        {
                            displayText = displayText.Replace("-", " \u2013 ");
                        }

                        if (options.ReplaceUnderscoreWithEnSpace)
                        {
                            displayText = displayText.Replace('_', '\u2002');
                        }
                    }

                    SizeF textBounds = this.graphics.MeasureString(displayText, textFont);
                    float totalWidth = textBounds.Width * scaleX;

                    GraphicsState savedState = this.graphics.Save();
                    try
                    {
                        bool useFieldOrigin = textField.FieldOrigin != null;
                        Matrix rotationMatrix = GetRotationMatrix(x, y, textBounds.Width, textBounds.Height, useFieldOrigin, textField.Font.FieldOrientation);
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
                        if (fieldJustification == FieldJustification.Right ||
                            (fieldJustification == FieldJustification.Auto && IsRightToLeftText(displayText)))
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
                    return this.CalculateNextDefaultPosition(x, y, totalWidth, textBounds.Height, false, textField.Font.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        private static bool IsRightToLeftText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                int codePoint = text[i];
                if ((codePoint >= 0x0590 && codePoint <= 0x08FF) ||
                    (codePoint >= 0xFB1D && codePoint <= 0xFEFC))
                {
                    return true;
                }
            }

            return false;
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
