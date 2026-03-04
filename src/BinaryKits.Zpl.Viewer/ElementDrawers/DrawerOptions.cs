using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    public class DrawerOptions
    {
        [Obsolete("Use FontManager.FontLoader instead.")]
        public Func<string, Font> FontLoader { get; set; }

        /// <summary>
        /// Gets or sets the image format used when rendering output.
        /// </summary>
        public ImageFormat RenderFormat { get; set; }

        /// <summary>
        /// Gets or sets the quality level used when rendering images in formats that support lossy compression.
        /// </summary>
        public int RenderQuality { get; set; }

        /// <summary>
        /// Applies label over a white background after rendering all elements
        /// </summary>
        public bool OpaqueBackground { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether dashes should be replaced with en dash.
        /// </summary>
        public bool ReplaceDashWithEnDash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether underscores in text should be replaced with en space.
        /// </summary>
        public bool ReplaceUnderscoreWithEnSpace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether antialiasing is enabled.
        /// </summary>
        public bool Antialias { get; set; }

        public FontManager FontManager { get; private set; }

        public DrawerOptions() : this(new FontManager()) { }

        public DrawerOptions(FontManager fontManager)
        {
            this.FontManager = fontManager;
            this.RenderFormat = ImageFormat.Png;
            this.RenderQuality = 80;
            this.OpaqueBackground = false;
            this.ReplaceDashWithEnDash = true;
            this.ReplaceUnderscoreWithEnSpace = false;
            this.Antialias = true;
        }
    }
}
