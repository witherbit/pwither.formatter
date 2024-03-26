﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    public interface ISurrogateSelector
    {
        ISerializationSurrogate? GetSurrogate(Type type, StreamingContext context);
    }
}
