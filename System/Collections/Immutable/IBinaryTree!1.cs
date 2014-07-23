namespace System.Collections.Immutable
{
    using System;

    internal interface IBinaryTree<out T>
    {
        int Count { get; }

        int Height { get; }

        bool IsEmpty { get; }

        IBinaryTree<T> Left { get; }

        IBinaryTree<T> Right { get; }

        T Value { get; }
    }
}

