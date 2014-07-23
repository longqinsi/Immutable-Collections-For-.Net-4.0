namespace System.Collections.Immutable
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{Value,nq}")]
    internal struct RefAsValueType<T>
    {
        internal T Value;
        internal RefAsValueType(T value)
        {
            this.Value = value;
        }
    }
}

