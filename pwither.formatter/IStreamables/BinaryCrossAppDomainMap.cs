﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class BinaryCrossAppDomainMap : IStreamable
    {
        internal int _crossAppDomainArrayIndex;

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)BitBinaryHeaderEnum.CrossAppDomainMap);
            output.WriteInt32(_crossAppDomainArrayIndex);
        }

        public void Read(BinaryParser input)
        {
            _crossAppDomainArrayIndex = input.ReadInt32();
        }
    }
}
