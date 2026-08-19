using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace PdfImageRenderer;

/// <summary>
/// Renders PDF pages as base64-encoded data URL images.
/// </summary>
public sealed class PdfRenderService
{
  /// <summary>
  /// Returns the total number of pages in the given PDF.
  /// </summary>
  public int GetPageCount(byte[] pdfBytes)
  {
    using var doc = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(1000, 1000));
    return doc.GetPageCount();
  }

  /// <summary>
  /// Renders all pages of the given PDF as base64 data URL strings (one per page).
  /// </summary>
  /// <param name="pdfBytes">Raw PDF file bytes.</param>
  /// <param name="cssMaxWidth">Maximum render width in CSS pixels.</param>
  /// <param name="oversample">Oversampling factor for sharpness (2.0 = retina).</param>
  /// <param name="format">Output image format: "webp", "png", or "jpeg".</param>
  /// <param name="quality">Compression quality for webp/jpeg (0-100).</param>
  /// <returns>List of data URL strings, one per PDF page.</returns>
  public List<string> RenderAllPagesDataUrls(
    byte[] pdfBytes,
    int cssMaxWidth = 1200,
    double oversample = 2.0,
    string format = "webp",
    int quality = 90)
  {
    var renderBase = (int)Math.Round(cssMaxWidth * oversample);
    using var doc = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(renderBase, renderBase));
    int count = doc.GetPageCount();
    var result = new List<string>(count);

    var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

    for (int i = 0; i < count; i++)
    {
      using var page = doc.GetPageReader(i);

      int w = page.GetPageWidth();
      int h = page.GetPageHeight();

      int targetW = Math.Min(cssMaxWidth, w);
      int targetH = (int)Math.Round(h * (targetW / (double)w));

      byte[] pixels = page.GetImage();
      var srcInfo = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul);
      using var srcBmp = new SKBitmap(srcInfo);
      Marshal.Copy(pixels, 0, srcBmp.GetPixels(), pixels.Length);

      var dstInfo = new SKImageInfo(targetW, targetH, srcInfo.ColorType, srcInfo.AlphaType);
      using var dstBmp = new SKBitmap(dstInfo);

      using (var canvas = new SKCanvas(dstBmp))
      using (var srcImage = SKImage.FromBitmap(srcBmp))
      {
        canvas.Clear();
        canvas.DrawImage(srcImage, new SKRect(0, 0, targetW, targetH), sampling);
      }

      using var img = SKImage.FromBitmap(dstBmp);

      SKData data =
        format.Equals("png", StringComparison.OrdinalIgnoreCase)
          ? img.Encode(SKEncodedImageFormat.Png, 0)
          : format.StartsWith("jp", StringComparison.OrdinalIgnoreCase)
            ? img.Encode(SKEncodedImageFormat.Jpeg, quality)
            : img.Encode(SKEncodedImageFormat.Webp, quality);

      string mime = format.Equals("png", StringComparison.OrdinalIgnoreCase) ? "image/png"
        : format.StartsWith("jp", StringComparison.OrdinalIgnoreCase) ? "image/jpeg"
        : "image/webp";

      result.Add($"data:{mime};base64,{Convert.ToBase64String(data.ToArray())}");
    }

    return result;
  }
}
