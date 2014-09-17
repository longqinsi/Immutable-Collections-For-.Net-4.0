using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{
    internal interface IWellKnownStringEqualityComparer
    {
        // Methods
        IEqualityComparer GetEqualityComparerForSerialization();
        IEqualityComparer GetRandomizedEqualityComparer();
    }

 

}
