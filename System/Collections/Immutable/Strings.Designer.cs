namespace System.Collections.Immutable
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class Strings
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Strings()
        {
        }

        internal static string ArrayInitializedStateNotEqual
        {
            get
            {
                return ResourceManager.GetString("ArrayInitializedStateNotEqual", resourceCulture);
            }
        }

        internal static string ArrayLengthsNotEqual
        {
            get
            {
                return ResourceManager.GetString("ArrayLengthsNotEqual", resourceCulture);
            }
        }

        internal static string CannotFindOldValue
        {
            get
            {
                return ResourceManager.GetString("CannotFindOldValue", resourceCulture);
            }
        }

        internal static string CollectionModifiedDuringEnumeration
        {
            get
            {
                return ResourceManager.GetString("CollectionModifiedDuringEnumeration", resourceCulture);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string DuplicateKey
        {
            get
            {
                return ResourceManager.GetString("DuplicateKey", resourceCulture);
            }
        }

        internal static string InvalidEmptyOperation
        {
            get
            {
                return ResourceManager.GetString("InvalidEmptyOperation", resourceCulture);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Collections.Immutable.Strings", typeof(Strings).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}

