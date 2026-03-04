using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

using System.Drawing;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Public interface for element drawers
    /// </summary>
    public interface IElementDrawer
    {
        /// <summary>
        /// Prepare the drawer
        /// </summary>
        /// <param name="printerStorage"></param>
        /// <param name="graphics"></param>
        void Prepare(
            IPrinterStorage printerStorage,
            Graphics graphics);

        /// <summary>
        /// Check if the drawer can draw this element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        bool CanDraw(ZplElementBase element);

        /// <summary>
        /// Element requires reverse draw
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        bool IsReverseDraw(ZplElementBase element);

        /// <summary>
        /// Element is white
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        bool IsWhiteDraw(ZplElementBase element);

        /// <summary>
        /// Element needs to be drawn in bitmap mode
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        bool ForceBitmapDraw(ZplElementBase element);

        /// <summary>
        /// Draw the element
        /// </summary>
        PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition);

        /// <summary>
        /// Draw the element with extra context information
        /// </summary>
        PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont);

        /// <summary>
        /// Draw the element with extra context information
        /// </summary>
        PointF Draw(ZplElementBase element, DrawerOptions options, PointF currentPosition, InternationalFont internationalFont, int printDensityDpmm);
    }
}
