using ZXing;
using ZXing.SkiaSharp;
using SkiaSharp;
using System.IO;
using ZXing.SkiaSharp.Rendering;

public static class BarcodeGenerator
{

    public static byte[] GenerateBarcode(string data)
    {
        var barcodeWriter = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = 300,
                Height = 100,
                Margin = 1
            },
            Renderer = new SKBitmapRenderer() 
        };

        using var bitmap = barcodeWriter.Write(data);
        using var stream = new MemoryStream();
        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        return stream.ToArray();
    }

}
