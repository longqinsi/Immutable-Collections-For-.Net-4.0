namespace Validation
{
    using System;

    [AttributeUsage((AttributeTargets) AttributeTargets.Parameter, AllowMultiple=false, Inherited=false)]
    internal sealed class ValidatedNotNullAttribute : Attribute
    {
    }
}

