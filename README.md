# PdfImageRenderer

A .NET 8 library for rendering PDF pages as base64 data URL images.

Uses [Docnet.Core](https://github.com/GowenGit/docnet) for PDF decoding and [SkiaSharp](https://github.com/mono/SkiaSharp) for image scaling and encoding.

## Installation

```bash
dotnet add package PdfImageRenderer
```

## Usage

```csharp
using PdfImageRenderer;

var service = new PdfRenderService();

byte[] pdfBytes = File.ReadAllBytes("document.pdf");

// Returns a list of base64 data URLs, one per page
List<string> images = service.RenderAllPagesDataUrls(pdfBytes);

// Use in HTML
// <img src="@images[0]" />
```

### Options

```csharp
service.RenderAllPagesDataUrls(
    pdfBytes,
    cssMaxWidth: 1200,   // max render width in CSS pixels
    oversample: 2.0,     // 2x for retina/sharp output
    format: "webp",      // "webp" | "png" | "jpeg"
    quality: 90          // quality for webp/jpeg (0-100)
);
```

## License

MIT. See [NOTICES.txt](NOTICES.txt) for third-party licenses.
