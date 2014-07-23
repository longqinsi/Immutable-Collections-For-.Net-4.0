namespace Validation
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, DebuggerNonUserCode, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal class ValidationStrings
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal ValidationStrings()
        {
        }

        internal static string Argument_EmptyArray
        {
            get
            {
                return ResourceManager.GetString("Argument_EmptyArray", resourceCulture);
            }
        }

        internal static string Argument_EmptyString
        {
            get
            {
                return ResourceManager.GetString("Argument_EmptyString", resourceCulture);
            }
        }

        internal static string Argument_NullElement
        {
            get
            {
                return ResourceManager.GetString("Argument_NullElement", resourceCulture);
            }
        }

        internal static string Argument_Whitespace
        {
            get
            {
                return ResourceManager.GetString("Argument_Whitespace", resourceCulture);
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

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Collections.Immutable.Validation.ValidationStrings", typeof(ValidationStrings).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }
    }
}

