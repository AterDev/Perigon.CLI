using SkiaSharp;
namespace Ater.Web.Extension;

/// <summary>
/// 图形帮助类
/// </summary>
public class ImageHelper
{
    /// <summary>
    ///  生成图形验证码
    /// </summary>
    /// <param name="captchaText"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fontSize"></param>
    /// <returns>png文件的bytes</returns>
    public static byte[] GenerateImageCaptcha(string captchaText, int width = 80, int height = 40, int fontSize = 24)
    {
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        // 获取画布
        SKCanvas canvas = surface.Canvas;
        // 填充背景色
        canvas.Clear(SKColors.White);

        // 创建文本画笔
        var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = fontSize,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true,
            Typeface = SKTypeface.Default
        };
        // 计算文本位置
        var textX = width / 2;
        var textY = height * 3 / 4;
        // 绘制文本
        canvas.DrawText(captchaText, textX, textY, textPaint);

        // 添加干扰线条
        var random = new Random();
        for (var i = 0; i < 8; i++)
        {
            var startX = random.Next(width);
            var startY = random.Next(height);
            var endX = random.Next(width);
            var endY = random.Next(height);
            var linePaint = new SKPaint
            {
                Color = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)),
                StrokeWidth = 1,
                IsAntialias = true
            };
            canvas.DrawLine(startX, startY, endX, endY, linePaint);
        }

        // 随机生成干扰点
        for (var i = 0; i < 100; i++)
        {
            var paint = new SKPaint { Color = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)) };
            var point = new SKPoint(random.Next(width), random.Next(height));
            canvas.DrawPoint(point, paint);
        }

        // 将绘制的内容保存为图片
        using SKImage image = surface.Snapshot();
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        return stream.ToArray();
    }
}
