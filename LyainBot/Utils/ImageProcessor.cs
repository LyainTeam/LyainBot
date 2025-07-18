using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace LyainBot.Utils;

public static class ImageProcessor
{
    public static byte[] ProcessImage(byte[] imageData)
    {
        if (imageData == null || imageData.Length == 0)
            throw new ArgumentException("Image data cannot be null or empty.", nameof(imageData));
        using MemoryStream inputStream = new(imageData);
        using Image image = Image.Load(inputStream);
        using MemoryStream outputStream = new();
        float scale = Math.Min(512f / image.Width, 512f / image.Height);
        int newWidth = (int)(image.Width * scale);
        int newHeight = (int)(image.Height * scale);
        if (newWidth >= 512) newWidth = 512;
        if (newHeight >= 512) newHeight = 512;
        image.Mutate(x => x.Resize(newWidth, newHeight));
        image.SaveAsWebp(outputStream);
        return outputStream.ToArray();
    }
}