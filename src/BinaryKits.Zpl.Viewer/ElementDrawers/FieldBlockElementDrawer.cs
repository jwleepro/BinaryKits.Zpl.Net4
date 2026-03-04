using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.Helpers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Field Block elements
    /// </summary>
    public class FieldBlockElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplFieldBlock;
        }

        ///<inheritdoc/>
        public override bool IsReverseDraw(ZplElementBase element)
        {
            if (element is ZplFieldBlock fieldBlock)
            {
                return fieldBlock.ReversePrint;
            }

            return false;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplFieldBlock fieldBlock)
            {
                ZplFont font = fieldBlock.Font;
                (float fontSize, float scaleX) = FontScale.GetFontScaling(font.FontName, font.FontHeight, font.FontWidth, printDensityDpmm);

                Font loadedFont = options.FontManager.FontLoader(font.FontName);
                using (Font textFont = new Font(loadedFont.FontFamily, fontSize, loadedFont.Style))
                {
                    string text = fieldBlock.Text;
                    if (fieldBlock.HexadecimalIndicator is char hexIndicator)
                    {
                        text = text.ReplaceHexEscapes(hexIndicator, internationalFont);
                    }

                    if (font.FontName == "0")
                    {
                        if (options.ReplaceDashWithEnDash)
                        {
                            text = text.Replace("-", " \u2013 ");
                        }

                        if (options.ReplaceUnderscoreWithEnSpace)
                        {
                            text = text.Replace('_', '\u2002');
                        }
                    }

                    float x = fieldBlock.PositionX;
                    float y = fieldBlock.PositionY;

                    if (fieldBlock.UseDefaultPosition)
                    {
                        x = currentPosition.X;
                        y = currentPosition.Y;
                    }

                    List<string> textLines = WordWrap(text, this.graphics, textFont, fieldBlock.Width).Take(fieldBlock.MaxLineCount).ToList();
                    int hangingIndent = 0;
                    float lineHeight = fontSize + fieldBlock.LineSpace;

                    // actual ZPL printer does not include trailing line spacing in total height
                    float totalHeight = lineHeight * fieldBlock.MaxLineCount - fieldBlock.LineSpace;

                    if (fieldBlock.FieldTypeset != null)
                    {
                        totalHeight = lineHeight * (fieldBlock.MaxLineCount - 1) + textFont.GetHeight(this.graphics);
                        y -= totalHeight;
                    }

                    GraphicsState savedState = this.graphics.Save();
                    try
                    {
                        bool useFieldOrigin = fieldBlock.FieldOrigin != null;
                        Matrix matrix = GetRotationMatrix(fieldBlock.PositionX, fieldBlock.PositionY, fieldBlock.Width, totalHeight, useFieldOrigin, fieldBlock.Font.FieldOrientation);
                        if (matrix != null)
                        {
                            this.graphics.MultiplyTransform(matrix);
                            matrix.Dispose();
                        }

                        if (Math.Abs(scaleX - 1f) > 0.0001f)
                        {
                            Matrix scaleMatrix = new Matrix();
                            scaleMatrix.Translate(fieldBlock.PositionX, y, MatrixOrder.Append);
                            scaleMatrix.Scale(scaleX, 1f, MatrixOrder.Append);
                            scaleMatrix.Translate(-fieldBlock.PositionX, -y, MatrixOrder.Append);
                            this.graphics.MultiplyTransform(scaleMatrix);
                            scaleMatrix.Dispose();
                        }

                        foreach (string textLine in textLines)
                        {
                            float drawX = fieldBlock.PositionX + hangingIndent;
                            SizeF textBounds = this.graphics.MeasureString(textLine, textFont);
                            float diff = fieldBlock.Width - textBounds.Width;

                            switch (fieldBlock.TextJustification)
                            {
                                case TextJustification.Center:
                                    drawX += diff / 2f;
                                    break;
                                case TextJustification.Right:
                                    drawX += diff;
                                    hangingIndent = -fieldBlock.HangingIndent;
                                    break;
                                case TextJustification.Left:
                                case TextJustification.Justified:
                                default:
                                    hangingIndent = fieldBlock.HangingIndent;
                                    break;
                            }

                            using (SolidBrush brush = new SolidBrush(Color.Black))
                            {
                                this.graphics.DrawString(textLine, textFont, brush, drawX, y);
                            }

                            y += lineHeight;
                        }
                    }
                    finally
                    {
                        this.graphics.Restore(savedState);
                    }

                    return this.CalculateNextDefaultPosition(fieldBlock.PositionX, fieldBlock.PositionY, fieldBlock.Width, totalHeight, fieldBlock.FieldOrigin != null, fieldBlock.Font.FieldOrientation, currentPosition);
                }
            }

            return currentPosition;
        }

        private static List<string> WordWrap(string text, Graphics graphics, Font font, int maxWidth)
        {
            float spaceWidth = graphics.MeasureString(" ", font).Width;
            List<string> lines = new List<string>();

            Stack<string> words = new Stack<string>(text.Split(new char[] { ' ' }, StringSplitOptions.None).AsEnumerable().Reverse());
            StringBuilder line = new StringBuilder();
            float width = 0;

            while (words.Count != 0)
            {
                string word = words.Pop();
                if (word.Contains(@"\&"))
                {
                    string[] subwords = word.Split(new string[] { @"\&" }, 2, StringSplitOptions.None);
                    word = subwords[0];
                    words.Push(subwords[1]);

                    float wordWidth = graphics.MeasureString(word, font).Width;
                    if (width + wordWidth <= maxWidth)
                    {
                        line.Append(word);
                        lines.Add(line.ToString());
                        line = new StringBuilder();
                        width = 0;
                    }
                    else
                    {
                        if (line.Length > 0)
                        {
                            lines.Add(line.ToString().Trim());
                        }

                        lines.Add(word.ToString());
                        line = new StringBuilder();
                        width = 0;
                    }
                }
                else
                {
                    float wordWidth = graphics.MeasureString(word, font).Width;
                    if (width + wordWidth <= maxWidth)
                    {
                        line.Append(word + " ");
                        width += wordWidth + spaceWidth;
                    }
                    else
                    {
                        if (line.Length > 0)
                        {
                            lines.Add(line.ToString().Trim());
                        }

                        line = new StringBuilder(word + " ");
                        width = wordWidth + spaceWidth;
                    }
                }
            }

            if (line.Length > 0 || lines.Count == 0)
            {
                lines.Add(line.ToString().Trim());
            }

            return lines;
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
