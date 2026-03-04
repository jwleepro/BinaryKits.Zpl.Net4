using System;
using System.Collections.Generic;
using System.Drawing;

namespace BinaryKits.Zpl.Viewer
{
    public class FontManager
    {
        /// <summary>
        /// Gets or sets the list of font family names used as the primary font stack for rendering text.
        /// </summary>
        public List<string> FontStack0 { get; set; }

        /// <summary>
        /// Gets or sets the list of font family names used as the primary monospace font stack.
        /// </summary>
        public List<string> FontStackA { get; set; }

        /// <summary>
        /// Gets or sets the list of font family names used for graphic symbol rendering.
        /// </summary>
        public List<string> FontStackGS { get; set; }

        /// <summary>
        /// Gets or sets the delegate used to load a font by name and return a Font instance.
        /// </summary>
        public Func<string, Font> FontLoader { get; set; }

        /// <summary>
        /// Gets the default font for graphic symbols.
        /// </summary>
        public Font FontGS
        {
            get { return GetDefaultFontGS(); }
        }

        private Font defaultFont0;
        private Font defaultFontA;
        private Font defaultFontGS;

        public FontManager()
        {
            this.FontStack0 = new List<string>
            {
                "Swis721 Cn BT",
                "TeX Gyre Heros Cn",
                "Nimbus Sans Narrow",
                "Roboto Condensed",
                "Helvetica",
                "Helvetica Neue",
                "Arial",
            };

            this.FontStackA = new List<string>
            {
                "DejaVu Sans Mono",
                "Lucida Console",
                "Andale Mono",
                "Droid Sans Mono",
                "Courier New",
            };

            this.FontStackGS = new List<string>
            {
                "Segoe UI Symbol",
                "Arial Unicode MS",
                "Arial",
            };

            this.FontLoader = (fontName) =>
            {
                if (fontName == "0")
                {
                    return GetDefaultFont0();
                }

                if (fontName == "GS")
                {
                    return GetDefaultFontGS();
                }

                return GetDefaultFontA();
            };
        }

        private Font GetDefaultFont0()
        {
            if (this.defaultFont0 != null)
            {
                return this.defaultFont0;
            }

            foreach (string fontFamily in this.FontStack0)
            {
                Font font = TryCreateFont(fontFamily, 10f, FontStyle.Bold);
                if (font != null)
                {
                    this.defaultFont0 = font;
                    return font;
                }
            }

            this.defaultFont0 = new Font("Arial", 10f, FontStyle.Bold);
            return this.defaultFont0;
        }

        private Font GetDefaultFontA()
        {
            if (this.defaultFontA != null)
            {
                return this.defaultFontA;
            }

            foreach (string fontFamily in this.FontStackA)
            {
                Font font = TryCreateFont(fontFamily, 10f, FontStyle.Regular);
                if (font != null)
                {
                    this.defaultFontA = font;
                    return font;
                }
            }

            this.defaultFontA = new Font("Courier New", 10f, FontStyle.Regular);
            return this.defaultFontA;
        }

        private Font GetDefaultFontGS()
        {
            if (this.defaultFontGS != null)
            {
                return this.defaultFontGS;
            }

            foreach (string fontFamily in this.FontStackGS)
            {
                Font font = TryCreateFont(fontFamily, 10f, FontStyle.Regular);
                if (font != null)
                {
                    this.defaultFontGS = font;
                    return font;
                }
            }

            this.defaultFontGS = new Font("Arial", 10f, FontStyle.Regular);
            return this.defaultFontGS;
        }

        private static Font TryCreateFont(string familyName, float size, FontStyle style)
        {
            try
            {
                FontFamily family = new FontFamily(familyName);
                if (family.IsStyleAvailable(style))
                {
                    return new Font(family, size, style);
                }

                return new Font(family, size);
            }
            catch
            {
                return null;
            }
        }
    }
}
