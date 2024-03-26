// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class MessageEnd : IStreamable
    {
        internal MessageEnd() { }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)BitBinaryHeaderEnum.MessageEnd);
        }

        public void Read(BinaryParser input)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
        }
    }
}
