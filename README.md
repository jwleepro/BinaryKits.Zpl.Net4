# BinaryKits.Zpl.Net4

.NET Framework 4.0 port of [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl).

This repository keeps the original BinaryKits.Zpl development model while targeting legacy .NET Framework environments that cannot move to modern .NET.

## Modules

| Module | Description |
| --- | --- |
| `BinaryKits.Zpl.Label` | Core ZPL element model and renderer (`ZplEngine`) |
| `BinaryKits.Zpl.Labelary` | Labelary API client for preview image generation |
| `BinaryKits.Zpl.Viewer` | Local ZPL analyzer/drawer (`System.Drawing` + `ZXing.Net`) |
| `BinaryKits.Zpl.TestConsole` | Console sample app with practical rendering examples |
| `BinaryKits.Zpl.Label.UnitTest` | MSTest v1 test project for core label logic |

`BinaryKits.Zpl.Protocol` is not included in this .NET 4.0 port.

## Port Notes

| Area | Upstream | This Port |
| --- | --- | --- |
| Target framework | .NET (modern SDK style) | .NET Framework 4.0 |
| Project format | SDK-style `.csproj` | Legacy `ToolsVersion="4.0"` `.csproj` |
| Language level | Latest C# | C# 7.3 (`LangVersion` 7.3) |
| Networking APIs | Async patterns | Synchronous APIs |
| Image/rendering deps | Cross-platform stack in upstream | `System.Drawing` + `ZXing.Net` |
| Tests | Modern MSTest usage | MSTest v1 style |

## Build

### Prerequisites

- Windows
- Visual Studio (or Build Tools) with .NET Framework 4.0 targeting pack
- `nuget.exe` (included in repository root)

### Build Commands

```powershell
# 1) Restore packages (for packages.config projects)
.\nuget.exe restore .\BinaryKits.Zpl.Net4.sln

# 2) Build solution (Developer Command Prompt)
msbuild .\BinaryKits.Zpl.Net4.sln /p:Configuration=Release
```

If `msbuild` is not in PATH:

```powershell
& "C:\Program Files\Microsoft Visual Studio\<Version>\Community\MSBuild\Current\Bin\MSBuild.exe" .\BinaryKits.Zpl.Net4.sln /p:Configuration=Release
```

## Supported Elements (Label Module)

| Category | Supported |
| --- | --- |
| 1D Barcodes | ANSI Codabar, Code 39, Code 93, Code 128, EAN-13, Interleaved 2 of 5, UPC-A, UPC-E, UPC Extension |
| 2D Barcodes | QR Code, Data Matrix, PDF417, Aztec, MaxiCode |
| Text | `ZplTextField`, `ZplSingleLineFieldBlock`, `ZplFieldBlock`, `ZplTextBlock` |
| Graphics | `ZplGraphicBox`, `ZplGraphicCircle`, `ZplGraphicDiagonalLine`, `ZplGraphicEllipse`, `ZplGraphicSymbol` |
| Images | `ZplDownloadObjects`, `ZplDownloadGraphics`, `ZplImageMove`, `ZplRecallGraphic` |
| Other | `ZplRaw`, `ZplChangeInternationalFont`, `ZplReferenceGrid`, format/field-number elements |

## Usage Examples

### Basic Setup

```csharp
using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;
```

### Single Element

```csharp
var output = new ZplGraphicBox(100, 100, 100, 100).ToZplString();
Console.WriteLine(output);
```

### Barcode

```csharp
var output = new ZplBarcode128("123ABC", 10, 50).ToZplString();
Console.WriteLine(output);
```

### Whole Label

```csharp
var sampleText = "[_~^][LineBreak\n][The quick fox jumps over the lazy dog.]";
var font = new ZplFont(fontWidth: 50, fontHeight: 50);

var elements = new ZplElementBase[]
{
    new ZplTextField(sampleText, 50, 100, font),
    new ZplGraphicBox(400, 700, 100, 100, 5),
    new ZplGraphicBox(450, 750, 100, 100, 50, LineColor.White),
    new ZplGraphicCircle(400, 700, 100, 5),
    new ZplGraphicDiagonalLine(400, 700, 100, 50, 5),
    new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 600, 600, 50, 50),
    new ZplRaw("^FO200, 200^GB300, 200, 10 ^FS")
};

var zpl = new ZplEngine(elements).ToZplString(new ZplRenderOptions
{
    SourcePrintDpi = 203,
    TargetPrintDpi = 203
});

Console.WriteLine(zpl);
```

### Download Graphics (~DG / ^XG)

```csharp
var elements = new ZplElementBase[]
{
    new ZplDownloadGraphics('R', "SAMPLE", File.ReadAllBytes("sample.png"), ZplCompressionScheme.Z64),
    new ZplRecallGraphic(100, 100, 'R', "SAMPLE")
};

var zpl = new ZplEngine(elements).ToZplString(new ZplRenderOptions
{
    SourcePrintDpi = 200,
    TargetPrintDpi = 600
});
```

### Labelary Preview

```csharp
using BinaryKits.Zpl.Labelary;

string zplData = "^XA^FT10,60^APN,30,30^FDSAMPLE TEXT^FS^XZ";

using (var client = new LabelaryClient())
{
    var previewData = client.GetPreview(zplData, PrintDensity.PD8dpmm, new LabelSize(6, 8, Measure.Inch));
    if (previewData.Length > 0)
    {
        File.WriteAllBytes("preview.png", previewData);
    }
}
```

### Local Preview (Viewer)

```csharp
using BinaryKits.Zpl.Viewer;

string zplData = "^XA^FT100,100^A0N,67,0^FDTestLabel^FS^XZ";

IPrinterStorage printerStorage = new PrinterStorage();
var analyzer = new ZplAnalyzer(printerStorage);
var drawer = new ZplElementDrawer(printerStorage);

var analyzeInfo = analyzer.Analyze(zplData);
foreach (var labelInfo in analyzeInfo.LabelInfos)
{
    var imageData = drawer.Draw(labelInfo.ZplElements);
    File.WriteAllBytes("label.png", imageData);
}
```

### Send ZPL to Printer (TCP 9100)

```csharp
var zplData = "^XA^FT10,60^APN,30,30^FDSAMPLE TEXT^FS^XZ";

using (var tcpClient = new System.Net.Sockets.TcpClient())
{
    tcpClient.Connect("10.10.5.85", 9100);

    using (var writer = new System.IO.StreamWriter(tcpClient.GetStream()))
    {
        writer.Write(zplData);
        writer.Flush();
    }
}
```

## Project Structure

```text
BinaryKits.Zpl.Net4/
  BinaryKits.Zpl.Net4.sln
  src/
    BinaryKits.Zpl.Label/
    BinaryKits.Zpl.Labelary/
    BinaryKits.Zpl.Viewer/
    BinaryKits.Zpl.TestConsole/
    BinaryKits.Zpl.Label.UnitTest/
```

## Credits

Original project: [BinaryKits.Zpl](https://github.com/BinaryKits/BinaryKits.Zpl) by [BinaryKits](https://github.com/BinaryKits).

This repository is a compatibility port for .NET Framework 4.0.

## License

Follow the same license terms as the upstream BinaryKits.Zpl project.
