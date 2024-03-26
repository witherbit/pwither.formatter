// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class ObjectNull : IStreamable
    {
        internal int _nullCount;

        internal ObjectNull() { }

        internal void SetNullCount(int nullCount)
        {
            _nullCount = nullCount;
        }

        public void Write(BinaryFormatterWriter output)
        {
            if (_nullCount == 1)
            {
                output.WriteByte((byte)BitBinaryHeaderEnum.ObjectNull);
            }
            else if (_nullCount < 256)
            {
                output.WriteByte((byte)BitBinaryHeaderEnum.ObjectNullMultiple256);
                output.WriteByte((byte)_nullCount);
            }
            else
            {
                output.WriteByte((byte)BitBinaryHeaderEnum.ObjectNullMultiple);
                output.WriteInt32(_nullCount);
            }
        }

        public void Read(BinaryParser input)
        {
            Read(input, BitBinaryHeaderEnum.ObjectNull);
        }

        public void Read(BinaryParser input, BitBinaryHeaderEnum binaryHeaderEnum)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
            switch (binaryHeaderEnum)
            {
                case BitBinaryHeaderEnum.ObjectNull:
                    _nullCount = 1;
                    break;
                case BitBinaryHeaderEnum.ObjectNullMultiple256:
                    _nullCount = input.ReadByte();
                    break;
                case BitBinaryHeaderEnum.ObjectNullMultiple:
                    _nullCount = input.ReadInt32();
                    break;
            }
        }
    }
}
