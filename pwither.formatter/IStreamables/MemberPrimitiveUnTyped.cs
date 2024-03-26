﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class MemberPrimitiveUnTyped : IStreamable
    {
        // Used for members with primitive values and types are needed
        internal BitInternalPrimitiveTypeE _typeInformation;
        internal object? _value;

        internal MemberPrimitiveUnTyped() { }

        internal void Set(BitInternalPrimitiveTypeE typeInformation, object? value)
        {
            _typeInformation = typeInformation;
            _value = value;
        }

        internal void Set(BitInternalPrimitiveTypeE typeInformation)
        {
            _typeInformation = typeInformation;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteValue(_typeInformation, _value);
        }

        public void Read(BinaryParser input)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
            _value = input.ReadValue(_typeInformation);
        }
    }
}
