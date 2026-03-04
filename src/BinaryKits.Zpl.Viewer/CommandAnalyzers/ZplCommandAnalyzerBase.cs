using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

namespace BinaryKits.Zpl.Viewer.CommandAnalyzers
{
    public abstract class ZplCommandAnalyzerBase : IZplCommandAnalyzer
    {
        public string PrinterCommandPrefix { get; private set; }

        public ZplCommandAnalyzerBase(string prefix)
        {
            this.PrinterCommandPrefix = prefix;
        }

        ///<inheritdoc/>
        public bool CanAnalyze(string zplLine)
        {
            return zplLine.StartsWith(this.PrinterCommandPrefix);
        }

        ///<inheritdoc/>
        public abstract ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage);

        protected string[] SplitCommand(string zplCommand, int dataStartIndex = 0)
        {
            string zplCommandData = zplCommand.Substring(this.PrinterCommandPrefix.Length + dataStartIndex);
            return zplCommandData.Trim().Split(',');
        }

        protected FieldOrientation ConvertFieldOrientation(string fieldOrientation, VirtualPrinter virtualPrinter)
        {
            switch (fieldOrientation)
            {
                case "N":
                    return FieldOrientation.Normal;
                case "R":
                    return FieldOrientation.Rotated90;
                case "I":
                    return FieldOrientation.Rotated180;
                case "B":
                    return FieldOrientation.Rotated270;
                default:
                    return virtualPrinter.FieldOrientation;
            }
        }

        protected QualityLevel ConvertQualityLevel(string qualityLevel)
        {
            switch (qualityLevel)
            {
                case "0":
                    return QualityLevel.ECC0;
                case "50":
                    return QualityLevel.ECC50;
                case "80":
                    return QualityLevel.ECC80;
                case "100":
                    return QualityLevel.ECC100;
                case "140":
                    return QualityLevel.ECC140;
                case "200":
                    return QualityLevel.ECC200;
                default:
                    return QualityLevel.ECC0;
            }
        }

        protected FieldJustification ConvertFieldJustification(string fieldJustification,VirtualPrinter virtualPrinter)
        {
            switch (fieldJustification)
            {
                case "0":
                    return FieldJustification.Left;
                case "1":
                    return FieldJustification.Right;
                case "2":
                    return FieldJustification.Auto;
                default:
                    return virtualPrinter.FieldJustification;
            }
        }

        protected ErrorCorrectionLevel ConvertErrorCorrectionLevel(string errorCorrection)
        {
            switch (errorCorrection)
            {
                case "H":
                    return ErrorCorrectionLevel.UltraHighReliability;
                case "Q":
                    return ErrorCorrectionLevel.HighReliability;
                case "M":
                    return ErrorCorrectionLevel.Standard;
                case "L":
                    return ErrorCorrectionLevel.HighDensity;
                default:
                    return ErrorCorrectionLevel.Standard;
            }
        }

        protected bool ConvertBoolean(string yesOrNo, string defaultValue = "N")
        {
            return (!string.IsNullOrEmpty(yesOrNo) ? yesOrNo : defaultValue) == "Y";
        }

        protected int IndexOfNthCharacter(string input, int occurranceToFind, char charToFind)
        {
            int index = -1;
            for (int i = 0; i < occurranceToFind; i++)
            {
                index = input.IndexOf(charToFind, index + 1);

                if (index == -1)
                {
                    break;
                }
            }

            return index;
        }
    }
}
