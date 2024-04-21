using SkiaSharp;

namespace Canon.Server.Models;

public sealed class Brush(SKCanvas canvas) : IDisposable
{
    /// <summary>
    /// 在指定的位置绘制
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="text"></param>
    public void DrawText(SKPoint pos, string text)
    {
        SKPaint textPainter = new() { Color = SKColors.Black, IsAntialias = true, TextSize = 12f, StrokeWidth = 3f };
        SKPaint backgroundPainter = new() { Color = SKColors.White };

        SKRect textBorder = new();
        textPainter.MeasureText(text, ref textBorder);

        // 文字居中
        float x = pos.X - textBorder.Width / 2;
        float y = pos.Y - textBorder.Height / 2;
        textBorder.Offset(x, y);
        textBorder.Inflate(5, 5);
        canvas.DrawRect(textBorder, backgroundPainter);
        canvas.DrawText(text, x, y, textPainter);
    }

    /// <summary>
    /// 绘制线段
    /// </summary>
    /// <param name="start">线段的起始点</param>
    /// <param name="end">线段的终点</param>
    public void DrawLine(SKPoint start, SKPoint end)
    {
        SKPaint linePainter = new()
        {
            Color = SKColors.Black, IsAntialias = true, StrokeWidth = 3f, StrokeCap = SKStrokeCap.Round
        };

        canvas.DrawLine(start, end, linePainter);
    }

    public void Dispose()
    {
        canvas.Dispose();
    }
}
