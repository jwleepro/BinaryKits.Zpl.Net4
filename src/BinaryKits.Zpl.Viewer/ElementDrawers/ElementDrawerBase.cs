using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

using System.Drawing;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    public abstract class ElementDrawerBase : IElementDrawer
    {
        internal IPrinterStorage printerStorage;
        internal Graphics graphics;

        ///<inheritdoc/>
        public void Prepare(
            IPrinterStorage printerStorage,
            Graphics graphics)
        {
            this.printerStorage = printerStorage;
            this.graphics = graphics;
        }

        ///<inheritdoc/>
        public abstract bool CanDraw(ZplElementBase element);

        ///<inheritdoc/>
        public virtual bool IsReverseDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual bool IsWhiteDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual bool ForceBitmapDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition)
        {
            return currentPosition;
        }

        ///<inheritdoc/>
        public virtual PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont)
        {
            return this.Draw(element, options, currentPosition);
        }

        ///<inheritdoc/>
        public virtual PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            return this.Draw(element, options, currentPosition, internationalFont);
        }

        protected virtual PointF CalculateNextDefaultPosition(float x, float y, float elementWidth, float elementHeight, bool useFieldOrigin, Label.FieldOrientation fieldOrientation, PointF currentPosition)
        {
            if (useFieldOrigin)
            {
                switch (fieldOrientation)
                {
                    case Label.FieldOrientation.Normal:
                        return new PointF(x + elementWidth, y + elementHeight);
                    case Label.FieldOrientation.Rotated90:
                        return new PointF(x, y + elementHeight);
                    case Label.FieldOrientation.Rotated180:
                        return new PointF(x - elementWidth, y);
                    case Label.FieldOrientation.Rotated270:
                        return new PointF(x, y - elementHeight);
                }
            }
            else
            {
                switch (fieldOrientation)
                {
                    case Label.FieldOrientation.Normal:
                        return new PointF(x + elementWidth, y);
                    case Label.FieldOrientation.Rotated90:
                        return new PointF(x, y + elementWidth);
                    case Label.FieldOrientation.Rotated180:
                        return new PointF(x - elementWidth, y);
                    case Label.FieldOrientation.Rotated270:
                        return new PointF(x, y - elementWidth);
                }
            }

            return currentPosition;
        }

    }
}
