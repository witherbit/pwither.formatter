﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class MemberReference : IStreamable
    {
        internal int _idRef;

        internal MemberReference() { }

        internal void Set(int idRef)
        {
            _idRef = idRef;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)BitBinaryHeaderEnum.MemberReference);
            output.WriteInt32(_idRef);
        }

        public void Read(BinaryParser input)
        {
            //binaryHeaderEnum = input.ReadByte(); already read
            _idRef = input.ReadInt32();
        }
    }
}
