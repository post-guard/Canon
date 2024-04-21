using Canon.Core.SyntaxNodes;
using SkiaSharp;

namespace Canon.Server.Models;

/// <summary>
/// 展示树节点
/// 参考 https://blog.csdn.net/sinat_33488770/article/details/123713490
/// </summary>
public class PresentableTreeNode
{
    public float X { get; set; }
    public float Y { get; set; }

    public SKPoint Position => new(X, Y);

    public string DisplayText { get; }
    public List<PresentableTreeNode> Children { get; } = [];

    /// <summary>
    /// 该节点在其兄弟节点中的排位
    /// </summary>
    private readonly int _index;

    /// <summary>
    /// 该节点的父节点
    /// </summary>
    private readonly PresentableTreeNode? _parent;

    /// <summary>
    /// 根据左兄弟节点和子节点中间定位的差值
    /// </summary>
    private float _mod;

    /// <summary>
    /// 分摊偏移量
    /// </summary>
    private float _change;
    private float _shift;

    /// <summary>
    /// 线程节点
    /// 指向下一个轮廓节点
    /// </summary>
    private PresentableTreeNode? _thread;

    /// <summary>
    /// 节点所在的树的根节点
    /// </summary>
    private PresentableTreeNode _ancestor;

    private PresentableTreeNode(string displayText, PresentableTreeNode? parent, int depth, int index)
    {
        Y = depth;
        DisplayText = displayText;
        _index = index;
        _ancestor = this;
        _parent = parent;
    }

    /// <summary>
    /// 计算树的高度和宽度
    /// </summary>
    /// <returns>(高度, 宽度)二元组</returns>
    public (float, float) CalculateImageSize()
    {
        Queue<PresentableTreeNode> queue = [];
        queue.Enqueue(this);

        float height = 1f;
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        while (queue.Count != 0)
        {
            // 每次只遍历当前层的节点
            int size = queue.Count;

            for (int i = 0; i < size; i++)
            {
                PresentableTreeNode node = queue.Dequeue();
                minX = float.Min(minX, node.X);
                maxX = float.Max(node.X, maxX);

                foreach (PresentableTreeNode child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }

            height++;
        }

        return (height, maxX - minX);
    }

    /// <summary>
    /// 从非终结节点构建展示树
    /// </summary>
    /// <param name="node">语法树上的节点</param>
    /// <returns>构建展示树的根节点</returns>
    public static PresentableTreeNode Build(NonTerminatedSyntaxNode node)
    {
        PresentableTreeNode root = Build(node, null, 0, 0);

        root.FirstWalk(1);
        root.SecondWalk(0, 0);

        return root;
    }

    private static PresentableTreeNode Build(NonTerminatedSyntaxNode node, PresentableTreeNode? parent, int depth,
        int index)
    {
        PresentableTreeNode root = new(node.ToString(), parent, depth, index);

        int i = 0;
        foreach (SyntaxNodeBase child in node.Children)
        {
            if (child.IsTerminated)
            {
                TerminatedSyntaxNode terminatedNode = child.Convert<TerminatedSyntaxNode>();
                root.Children.Add(
                    new PresentableTreeNode(terminatedNode.ToString(), root, depth + 1, i));
            }
            else
            {
                NonTerminatedSyntaxNode nonTerminatedNode = child.Convert<NonTerminatedSyntaxNode>();
                root.Children.Add(Build(nonTerminatedNode, root, depth, i));
            }

            i++;
        }

        return root;
    }

    /// <summary>
    /// 第一次遍历树
    /// </summary>
    /// <param name="distance">节点间的距离</param>
    private void FirstWalk(float distance)
    {
        if (Children.Count == 0)
        {
            // 节点没有子节点
            if (LeftBrother is not null)
            {
                X = LeftBrother.X + distance;
            }
            else
            {
                X = 0;
            }
        }
        else
        {
            PresentableTreeNode defaultAncestor = Children.First();
            foreach (PresentableTreeNode child in Children)
            {
                child.FirstWalk(distance);
                defaultAncestor = child.Apportion(distance, defaultAncestor);
            }

            ExecuteShift();

            // 子节点的中点是父节点的位置
            float midPoint = (Children.First().X + Children.Last().X) / 2;
            PresentableTreeNode? leftBrother = LeftBrother;
            if (leftBrother is null)
            {
                X = midPoint;
            }
            else
            {
                X = leftBrother.X + distance;
                _mod = X - midPoint;
            }
        }
    }


    private void SecondWalk(float mod, float depth)
    {
        X += mod;
        Y = depth;

        foreach (PresentableTreeNode child in Children)
        {
            child.SecondWalk(mod + _mod, depth + 1);
        }
    }

    /// <summary>
    /// 最左子节点
    /// </summary>
    private PresentableTreeNode? LeftNode
    {
        get
        {
            if (_thread is not null)
            {
                return _thread;
            }

            return Children.FirstOrDefault();
        }
    }

    /// <summary>
    /// 最右子节点
    /// </summary>
    private PresentableTreeNode? RightNode
    {
        get
        {
            if (_thread is not null)
            {
                return _thread;
            }

            return Children.LastOrDefault();
        }
    }

    /// <summary>
    /// 左兄弟节点
    /// </summary>
    private PresentableTreeNode? LeftBrother
    {
        get
        {
            PresentableTreeNode? leftBrother = null;
            if (_parent is null)
            {
                return leftBrother;
            }

            foreach (PresentableTreeNode node in _parent.Children)
            {
                if (node == this)
                {
                    return leftBrother;
                }

                leftBrother = node;
            }

            return leftBrother;
        }
    }

    private PresentableTreeNode? _leftMostSibling;

    /// <summary>
    /// 同一层的第一个兄弟节点
    /// </summary>
    private PresentableTreeNode? LeftMostSibling
    {
        get
        {
            if (_leftMostSibling is null && _parent is not null)
            {
                PresentableTreeNode? leftMostSibling = _parent.Children.FirstOrDefault();

                if (leftMostSibling != this)
                {
                    _leftMostSibling = leftMostSibling;
                }
            }

            return _leftMostSibling;
        }
    }

    private PresentableTreeNode GetAncestor(PresentableTreeNode leftChild, PresentableTreeNode defaultAncestor)
    {
        if (_parent is null)
        {
            return defaultAncestor;
        }

        if (_parent.Children.Contains(leftChild._ancestor))
        {
            return leftChild._ancestor;
        }

        return defaultAncestor;
    }

    private void ExecuteShift()
    {
        float change = 0, shift = 0;

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            PresentableTreeNode child = Children[i];

            child.X += shift;
            child._mod += shift;
            change += child._change;
            shift += child._shift + change;
        }
    }

    /// <summary>
    /// 判断两个子树的轮廓是否重合并移动分开
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="defaultAncestor"></param>
    /// <returns></returns>
    private PresentableTreeNode Apportion(float distance, PresentableTreeNode defaultAncestor)
    {
        if (LeftBrother is null || LeftMostSibling is null)
        {
            return defaultAncestor;
        }

        PresentableTreeNode innerRightNode = this;
        PresentableTreeNode outerRightNode = this;
        PresentableTreeNode innerLeftNode = LeftBrother;
        PresentableTreeNode outerLeftNode = LeftMostSibling;

        float innerRightMod = _mod;
        float outerRightMod = _mod;
        float innerLeftMod = innerLeftNode._mod;
        float outerLeftMod = outerLeftNode._mod;

        while (innerLeftNode.RightNode is not null && innerRightNode.LeftNode is not null)
        {
            innerLeftNode = innerLeftNode.RightNode;
            innerRightNode = innerRightNode.LeftNode;
            outerLeftNode = outerLeftNode.LeftNode!;
            outerRightNode = outerRightNode.RightNode!;

            outerRightNode._ancestor = this;

            float shift = innerLeftNode.X + innerLeftMod + distance - (innerRightNode.X + innerRightMod);
            if (shift > 0)
            {
                PresentableTreeNode ancestor = GetAncestor(innerLeftNode, defaultAncestor);
                MoveSubTree(ancestor, this, shift);

                innerRightMod += shift;
                outerRightMod += shift;
            }

            innerRightMod += innerRightNode._mod;
            outerRightMod += outerRightNode._mod;
            innerLeftMod += innerLeftNode._mod;
            outerLeftMod += outerLeftNode._mod;
        }

        if (innerLeftNode.RightNode is not null && outerRightNode.RightNode is null)
        {
            outerRightNode._thread = innerLeftNode.RightNode;
            outerRightNode._mod += innerLeftMod - outerRightMod;
        }
        else
        {
            if (innerRightNode.LeftNode is not null && outerLeftNode.LeftNode is null)
            {
                outerLeftNode._thread = innerRightNode.LeftNode;
                outerLeftNode._mod += innerRightMod - outerLeftMod;
            }

            defaultAncestor = this;
        }

        return defaultAncestor;
    }

    private static void MoveSubTree(PresentableTreeNode left, PresentableTreeNode right, float shift)
    {
        int subTrees = right._index - left._index;
        right._change -= shift / subTrees;
        right._shift += shift;
        left._change += shift / subTrees;
        right.X += shift;
        right._mod += shift;
    }
}
