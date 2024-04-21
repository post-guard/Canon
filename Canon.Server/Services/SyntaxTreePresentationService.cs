using Canon.Core.SyntaxNodes;
using Canon.Server.Models;
using SkiaSharp;

namespace Canon.Server.Services;

public class SyntaxTreePresentationService
{
    private const float Scale = 150;

    public Stream Present(ProgramStruct root)
    {
        PresentableTreeNode presentableTreeRoot = PresentableTreeNode.Build(root);
        ScaleTree(presentableTreeRoot);

        (float height, float width) = presentableTreeRoot.CalculateImageSize();
        using SKSurface surface = SKSurface.Create(
            new SKImageInfo((int)(width + 2 * Scale), (int)(height * Scale)));

        surface.Canvas.Clear(SKColors.White);

        using Brush brush = new(surface.Canvas);
        DrawNode(presentableTreeRoot, brush);

        using SKImage image = surface.Snapshot();
        SKData data = image.Encode();

        return data.AsStream();
    }

    private void DrawNode(PresentableTreeNode node, Brush brush)
    {
        foreach (PresentableTreeNode child in node.Children)
        {
            brush.DrawLine(node.Position, child.Position);
            DrawNode(child, brush);
        }

        brush.DrawText(node.Position, node.DisplayText);
    }

    private void ScaleTree(PresentableTreeNode root)
    {
        Queue<PresentableTreeNode> queue = [];
        queue.Enqueue(root);
        float minX = float.MaxValue;

        // 第一次遍历
        // 放大坐标并获得最左侧的节点X坐标
        while (queue.Count != 0)
        {
            PresentableTreeNode node = queue.Dequeue();

            node.X *= Scale;
            node.X += Scale;
            node.Y *= Scale;
            node.Y += Scale;

            minX = float.Min(minX, node.X);

            foreach (PresentableTreeNode child in node.Children)
            {
                queue.Enqueue(child);
            }
        }

        if (minX >= Scale)
        {
            // 判断最左侧的节点位置是否正确
            return;
        }

        float delta = Scale - minX;

        // 第二次遍历调整位置
        queue.Enqueue(root);
        while (queue.Count != 0)
        {
            PresentableTreeNode node = queue.Dequeue();

            node.X += delta;

            foreach (PresentableTreeNode child in node.Children)
            {
                queue.Enqueue(child);
            }
        }
    }
}
