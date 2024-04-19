using Canon.Core.SyntaxNodes;
using Canon.Visualization.Models;
using SkiaSharp;

namespace Canon.Visualization.Services;

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

    private void ScaleTree(PresentableTreeNode node)
    {
        node.X *= Scale;
        node.X += Scale;
        node.Y *= Scale;
        node.Y += Scale;

        foreach (PresentableTreeNode child in node.Children)
        {
            ScaleTree(child);
        }
    }
}
