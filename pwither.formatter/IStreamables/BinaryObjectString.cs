﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    internal sealed class BinaryObjectString : IStreamable
    {
        internal int _objectId;
        internal string? _value;

        internal BinaryObjectString() { }

        internal void Set(int objectId, string? value)
        {
            _objectId = objectId;
            _value = value;
        }

        public void Write(BinaryFormatterWriter output)
        {
            output.WriteByte((byte)BitBinaryHeaderEnum.ObjectString);
            output.WriteInt32(_objectId);
            output.WriteString(_value!);
        }

        public void Read(BinaryParser input)
        {
            _objectId = input.ReadInt32();
            _value = input.ReadString();
        }
    }
}
