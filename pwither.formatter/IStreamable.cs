﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    // Interface for Binary Records.
    internal interface IStreamable
    {
        void Write(BinaryFormatterWriter output);
        void Read(BinaryParser input);
    }
}
