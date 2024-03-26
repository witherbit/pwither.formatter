// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class MemberPrimitiveTyped : IStreamable
    {
        internal BitInternalPrimitiveTypeE _primitiveTypeEnum;
        internal object? _value;

        internal MemberPrimitiveTyped() { }

        internal void Set(BitInternalPrimitiveTypeE primitiveTypeEnum, object? value)
        {
            _primitiveTypeEnum = primitiveTypeEnum;
            _value = value;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)BitBinaryHeaderEnum.MemberPrimitiveTyped);
            output.WriteByte((byte)_primitiveTypeEnum);
            output.WriteValue(_primitiveTypeEnum, _value);
        }

        public void Read(BinaryParser input)
        {
            _primitiveTypeEnum = (BitInternalPrimitiveTypeE)input.ReadByte(); //PDJ
            _value = input.ReadValue(_primitiveTypeEnum);
        }
    }
}
