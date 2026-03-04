using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
using BinaryKits.Zpl.Viewer.ElementDrawers;
using BinaryKits.Zpl.Viewer.Helpers;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace BinaryKits.Zpl.Viewer
{
    public class ZplElementDrawer
    {
        /// <summary>
        /// The array of <see cref="IElementDrawer"/> to draw <see cref="ZplElementBase"/>
        /// </summary>
        public static IElementDrawer[] ElementDrawers { get; private set; }

        static ZplElementDrawer()
        {
            ElementDrawers = new IElementDrawer[]
            {
                new AztecBarcodeElementDrawer(),
                new Barcode128ElementDrawer(),
                new Barcode39ElementDrawer(),
                new Barcode93ElementDrawer(),
                new BarcodeEAN13ElementDrawer(),
                new BarcodeUpcAElementDrawer(),
                new BarcodeUpcEElementDrawer(),
                new BarcodeUpcExtensionElementDrawer(),
                new DataMatrixElementDrawer(),
                new FieldBlockElementDrawer(),
                new GraphicBoxElementDrawer(),
                new GraphicCircleElementDrawer(),
                new GraphicDiagonalLineElementDrawer(),
                new GraphicEllipseElementDrawer(),
                new GraphicFieldElementDrawer(),
                new GraphicSymbolElementDrawer(),
                new ImageMoveElementDrawer(),
                new Interleaved2of5BarcodeDrawer(),
                new MaxiCodeElementDrawer(),
                new Pdf417ElementDrawer(),
                new QrCodeElementDrawer(),
                new RecallGraphicElementDrawer(),
                new TextFieldElementDrawer(),
                new BarcodeAnsiCodabarElementDrawer(),
            };
        }

        private readonly DrawerOptions drawerOptions;
        private readonly IPrinterStorage printerStorage;

        public ZplElementDrawer(IPrinterStorage printerStorage, DrawerOptions drawerOptions = null)
        {
            this.drawerOptions = drawerOptions ?? new DrawerOptions();
            this.printerStorage = printerStorage;
        }

        /// <summary>
        /// Draw the label
        /// </summary>
        /// <param name="elements">Zpl elements</param>
        /// <param name="labelWidth">Label width in millimeter</param>
        /// <param name="labelHeight">Label height in millimeter</param>
        /// <param name="printDensityDpmm">Dots per millimeter</param>
        /// <returns></returns>
        public byte[] Draw(
            IEnumerable<ZplElementBase> elements,
            double labelWidth = 101.6,
            double labelHeight = 152.4,
            int printDensityDpmm = 8)
        {
            int labelImageWidth = Convert.ToInt32(labelWidth * printDensityDpmm);
            int labelImageHeight = Convert.ToInt32(labelHeight * printDensityDpmm);

            using (Bitmap bitmap = new Bitmap(labelImageWidth, labelImageHeight, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    if (this.drawerOptions.Antialias)
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    }

                    // Start with transparent background
                    g.Clear(Color.Transparent);

                    InternationalFont internationalFont = InternationalFont.ZCP850;
                    PointF currentDefaultPosition = PointF.Empty;

                    foreach (ZplElementBase element in elements)
                    {
                        if (element is ZplChangeInternationalFont changeFont)
                        {
                            internationalFont = changeFont.InternationalFont;
                            continue;
                        }

                        IElementDrawer drawer = ElementDrawers.SingleOrDefault(o => o.CanDraw(element));
                        if (drawer == null)
                        {
                            continue;
                        }

                        try
                        {
                            if (drawer.IsReverseDraw(element))
                            {
                                using (Bitmap invertBitmap = new Bitmap(labelImageWidth, labelImageHeight, PixelFormat.Format32bppArgb))
                                {
                                    using (Graphics gInvert = Graphics.FromImage(invertBitmap))
                                    {
                                        if (this.drawerOptions.Antialias)
                                        {
                                            gInvert.SmoothingMode = SmoothingMode.AntiAlias;
                                        }
                                        gInvert.Clear(Color.Transparent);

                                        drawer.Prepare(this.printerStorage, gInvert);
                                        currentDefaultPosition = drawer.Draw(element, this.drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);
                                    }

                                    // XOR blend for reverse draw
                                    XorBlend(bitmap, invertBitmap);
                                }
                                continue;
                            }

                            drawer.Prepare(this.printerStorage, g);
                            currentDefaultPosition = drawer.Draw(element, this.drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);
                        }
                        catch (Exception ex)
                        {
                            if (element is ZplBarcode barcodeElement)
                            {
                                throw new Exception(string.Format("Error on zpl element \"{0}\": {1}", barcodeElement.Content, ex.Message), ex);
                            }
                            else if (element is ZplDataMatrix dataMatrixElement)
                            {
                                throw new Exception(string.Format("Error on zpl element \"{0}\": {1}", dataMatrixElement.Content, ex.Message), ex);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    // Apply white background if needed
                    if (this.drawerOptions.OpaqueBackground)
                    {
                        using (Bitmap whiteBg = new Bitmap(labelImageWidth, labelImageHeight, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics gWhite = Graphics.FromImage(whiteBg))
                            {
                                gWhite.Clear(Color.White);
                                gWhite.DrawImage(bitmap, 0, 0);
                            }

                            using (MemoryStream ms = new MemoryStream())
                            {
                                whiteBg.Save(ms, this.drawerOptions.RenderFormat);
                                return ms.ToArray();
                            }
                        }
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, this.drawerOptions.RenderFormat);
                        return ms.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Simple XOR blend of two bitmaps
        /// </summary>
        private static void XorBlend(Bitmap baseBitmap, Bitmap overlayBitmap)
        {
            int width = Math.Min(baseBitmap.Width, overlayBitmap.Width);
            int height = Math.Min(baseBitmap.Height, overlayBitmap.Height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color overlayPixel = overlayBitmap.GetPixel(x, y);
                    if (overlayPixel.A > 0)
                    {
                        Color basePixel = baseBitmap.GetPixel(x, y);
                        if (basePixel.A > 0)
                        {
                            // XOR: if both have color, make transparent
                            baseBitmap.SetPixel(x, y, Color.Transparent);
                        }
                        else
                        {
                            // XOR: if only overlay has color, draw it
                            baseBitmap.SetPixel(x, y, overlayPixel);
                        }
                    }
                }
            }
        }
    }
}
