// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{

    public enum BitFormatterTypeStyle
    {
        TypesWhenNeeded = 0, // Types are outputted only for Arrays of Objects, Object Members of type Object, and ISerializable non-primitive value types
        TypesAlways = 0x1,   // Types are outputted for all Object members and ISerialiable object members.
        XsdString = 0x2      // Strings are outputed as xsd rather then SOAP-ENC strings. No string ID's are transmitted
    }


    public enum BitFormatterAssemblyStyle
    {
        Simple = 0,
        Full = 1,
    }


    public enum BitTypeFilterLevel
    {
        Low = 0x2,
        Full = 0x3
    }
}
