﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{

    /// <summary>
    /// BinaryHeaderEnum is the first byte on binary records (except for primitive types which do not have a header)
    /// 
    /// This enumeration identifies the type of the record. 
    /// Each record (except for MemberPrimitiveUnTyped) starts with a record type enumeration. The size of the enumeration is one BYTE. 
    /// </summary>
    internal enum BitBinaryHeaderEnum
    {
        SerializedStreamHeader = 0,
        Object = 1,
        ObjectWithMap = 2,
        ObjectWithMapAssemId = 3,
        ObjectWithMapTyped = 4,
        ObjectWithMapTypedAssemId = 5,
        ObjectString = 6,
        Array = 7,
        MemberPrimitiveTyped = 8,
        MemberReference = 9,
        ObjectNull = 10,
        MessageEnd = 11,
        Assembly = 12,
        ObjectNullMultiple256 = 13,
        ObjectNullMultiple = 14,
        ArraySinglePrimitive = 15,
        ArraySingleObject = 16,
        ArraySingleString = 17,
        CrossAppDomainMap = 18,
        CrossAppDomainString = 19,
        CrossAppDomainAssembly = 20,
        MethodCall = 21,
        MethodReturn = 22,
    }

    // BinaryTypeEnum is used specify the type on the wire. Additional information is transmitted with Primitive and Object types
    internal enum BitBinaryTypeEnum
    {
        Primitive = 0,
        String = 1,
        Object = 2, // System.Object
        /// <summary>
        /// Urt: Uses runtime type?
        /// Urt type is an assemId of 0. No assemblyString needs to be sent
        /// </summary>
        ObjectUrt = 3, // SystemClass
		ObjectUser = 4, // Class
        ObjectArray = 5,
        StringArray = 6,
        PrimitiveArray = 7,
    }

    internal enum BitBinaryArrayTypeEnum
    {
        Single = 0,
        Jagged = 1,
        Rectangular = 2,
        SingleOffset = 3,
        JaggedOffset = 4,
        RectangularOffset = 5,
    }

    // Enums are for internal use by the XML and Binary Serializers

    // Formatter Enums
    internal enum BitInternalSerializerTypeE
    {
        Soap = 1,
        Binary = 2,
    }

    // ParseRecord Enums
    internal enum BitInternalParseTypeE
    {
        Empty = 0,
        SerializedStreamHeader = 1,
        Object = 2,
        Member = 3,
        ObjectEnd = 4,
        MemberEnd = 5,
        Headers = 6,
        HeadersEnd = 7,
        SerializedStreamHeaderEnd = 8,
        Envelope = 9,
        EnvelopeEnd = 10,
        Body = 11,
        BodyEnd = 12,
    }

    internal enum BitInternalObjectTypeE
    {
        Empty = 0,
        Object = 1,
        Array = 2,
    }

    internal enum BitInternalObjectPositionE
    {
        Empty = 0,
        Top = 1,
        Child = 2,
        Headers = 3,
    }

    internal enum BitInternalArrayTypeE
    {
        Empty = 0,
        Single = 1,
        Jagged = 2,
        Rectangular = 3,
        Base64 = 4,
    }

    internal enum BitInternalMemberTypeE
    {
        Empty = 0,
        Header = 1,
        Field = 2,
        Item = 3,
    }

    internal enum BitInternalMemberValueE
    {
        Empty = 0,
        InlineValue = 1,
        Nested = 2,
        Reference = 3,
        Null = 4,
    }

    // Data Type Enums
    internal enum BitInternalPrimitiveTypeE
    {
        Invalid = 0,
        Boolean = 1,
        Byte = 2,
        Char = 3,
        Currency = 4,
        Decimal = 5,
        Double = 6,
        Int16 = 7,
        Int32 = 8,
        Int64 = 9,
        SByte = 10,
        Single = 11,
        TimeSpan = 12,
        DateTime = 13,
        UInt16 = 14,
        UInt32 = 15,
        UInt64 = 16,

        // Used in only for MethodCall or MethodReturn header
        Null = 17,
        String = 18,
    }

    // ValueType Fixup Enum
    internal enum BitValueFixupEnum
    {
        Empty = 0,
        Array = 1,
        Header = 2,
        Member = 3,
    }
}
