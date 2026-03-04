using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Label.Helpers;

using System.Drawing;
using System.IO;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Graphic Field elements
    /// </summary>
    public class GraphicFieldElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplGraphicField;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplGraphicField graphicField)
            {
                byte[] imageData = ByteHelper.HexToBytes(graphicField.Data);

                float x = graphicField.PositionX;
                float y = graphicField.PositionY;

                if (graphicField.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                Bitmap image;
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    image = new Bitmap(ms);
                }

                bool useFieldTypeset = graphicField.FieldTypeset != null;
                if (useFieldTypeset)
                {
                    y -= image.Height;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                this.graphics.DrawImage(image, x, y);
                PointF result = this.CalculateNextDefaultPosition(x, y, image.Width, image.Height, graphicField.FieldOrigin != null, Label.FieldOrientation.Normal, currentPosition);
                image.Dispose();
                return result;
            }

            return currentPosition;
        }
    }
}
