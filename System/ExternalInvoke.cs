using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System
{
    internal class ExternalInvoke
    {
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet = CharSet.Unicode)]
        private static extern bool InternalUseRandomizedHashing();
        [SecuritySafeCritical]
        internal static bool UseRandomizedHashing()
        {
            return InternalUseRandomizedHashing();
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet = CharSet.Unicode)]
        internal static extern int InternalMarvin32HashString(string s, int sLen, long additionalEntropy);

    }
}
