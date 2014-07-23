namespace Validation
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static class Requires
    {
        [DebuggerStepThrough]
        public static void Argument(bool condition)
        {
            if (!condition)
            {
                throw new ArgumentException();
            }
        }

        [DebuggerStepThrough]
        public static void Argument(bool condition, string parameterName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(message, parameterName);
            }
        }

        [DebuggerStepThrough]
        public static Exception FailRange(string parameterName, string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
            throw new ArgumentOutOfRangeException(parameterName, message);
        }

        [DebuggerStepThrough]
        public static T NotNull<T>([ValidatedNotNull] T value, string parameterName) where T: class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        [DebuggerStepThrough]
        public static T NotNullAllowStructs<T>([ValidatedNotNull] T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        [DebuggerStepThrough]
        public static void Range(bool condition, string parameterName, string message = null)
        {
            if (!condition)
            {
                FailRange(parameterName, message);
            }
        }
    }
}

