// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
//using System.Runtime.ExceptionServices;

namespace pwither.formatter
{
    [BitSerializable]
    public class BitSerializationException : SystemException
    {
        /// <summary>
        /// Creates a new SerializationException with its message
        /// string set to a default message.
        /// </summary>
        public BitSerializationException()
            : base(SR.SerializationException)
        {
            HResult = HResults.COR_E_SERIALIZATION;
        }

        public BitSerializationException(string? message)
            : base(message)
        {
            HResult = HResults.COR_E_SERIALIZATION;
        }

        public BitSerializationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            HResult = HResults.COR_E_SERIALIZATION;
        }
    }

    internal class HResults
    {
        internal const int COR_E_SERIALIZATION = unchecked((int)0x8013150C);
    }
}
