using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

using System.Drawing;
using System.IO;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Image Move elements
    /// </summary>
    public class ImageMoveElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplImageMove;
        }

        ///<inheritdoc/>
        public override PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplImageMove imageMove)
            {
                byte[] imageData = this.printerStorage.GetFile(imageMove.StorageDevice, imageMove.ObjectName);

                if (imageData.Length == 0)
                {
                    return currentPosition;
                }

                Bitmap image;
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    image = new Bitmap(ms);
                }

                float x = imageMove.PositionX;
                float y = imageMove.PositionY;

                if (imageMove.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                bool useFieldTypeset = imageMove.FieldTypeset != null;
                if (useFieldTypeset)
                {
                    y -= image.Height;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                this.graphics.DrawImage(image, x, y);

                PointF result = this.CalculateNextDefaultPosition(x, y, image.Width, image.Height, imageMove.FieldOrigin != null, Label.FieldOrientation.Normal, currentPosition);
                image.Dispose();
                return result;
            }

            return currentPosition;
        }
    }
}
