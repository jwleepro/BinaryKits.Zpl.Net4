using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

using System.Drawing;
using System.Drawing.Drawing2D;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    public class GraphicDiagonalLineElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplGraphicDiagonalLine;
        }

        public override bool IsReverseDraw(ZplElementBase element)
        {
            if (element is ZplGraphicDiagonalLine graphicLine)
            {
                return graphicLine.ReversePrint;
            }

            return false;
        }

        public override bool IsWhiteDraw(ZplElementBase element)
        {
            if (element is ZplGraphicDiagonalLine graphicLine)
            {
                return graphicLine.LineColor == LineColor.White;
            }

            return false;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplGraphicDiagonalLine graphicLine)
            {
                int border = graphicLine.BorderThickness;
                int width = graphicLine.Width;
                int height = graphicLine.Height;

                float x = graphicLine.PositionX;
                float y = graphicLine.PositionY;

                if (graphicLine.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                if (graphicLine.FieldTypeset != null)
                {
                    y -= height;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                PointF pointLL = new PointF(x, y + height);
                PointF pointUL = new PointF(x, y);

                Color drawColor = graphicLine.LineColor == LineColor.White ? Color.White : Color.Black;

                using (SolidBrush brush = new SolidBrush(drawColor))
                {
                    if (graphicLine.RightLeaningDiagonal)
                    {
                        PointF[] points = new PointF[]
                        {
                            pointLL,
                            new PointF(pointLL.X + border, pointLL.Y),
                            new PointF(pointLL.X + border + width, pointLL.Y - height),
                            new PointF(pointLL.X + width, pointLL.Y - height)
                        };
                        this.graphics.FillPolygon(brush, points);
                    }
                    else
                    {
                        PointF[] points = new PointF[]
                        {
                            pointUL,
                            new PointF(pointUL.X + border, pointUL.Y),
                            new PointF(pointUL.X + border + width, pointUL.Y + height),
                            new PointF(pointUL.X + width, pointUL.Y + height)
                        };
                        this.graphics.FillPolygon(brush, points);
                    }
                }

                // Calculate next position based on box dimensions
                return this.CalculateNextDefaultPosition(x, y, width, height, graphicLine.FieldOrigin != null, FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
