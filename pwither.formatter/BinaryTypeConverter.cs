// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace pwither.formatter
{
    // Routines to convert between the runtime type and the type as it appears on the wire
    internal static class BinaryTypeConverter
    {
        // From the type create the BinaryTypeEnum and typeInformation which describes the type on the wire
        internal static BitBinaryTypeEnum GetBinaryTypeInfo(Type type, WriteObjectInfo? objectInfo, string? typeName, 
            ObjectWriter objectWriter, out object? typeInformation, out int assemId)
        {
            BitBinaryTypeEnum binaryTypeEnum;

            assemId = 0;
            typeInformation = null;

            if (ReferenceEquals(type, Converter.s_typeofString))
            {
                binaryTypeEnum = BitBinaryTypeEnum.String;
            }
            else if (((objectInfo == null) || ((objectInfo != null) && !objectInfo._isSi)) && (ReferenceEquals(type, Converter.s_typeofObject)))
            {
                // If objectInfo.Si then can be a surrogate which will change the type
                binaryTypeEnum = BitBinaryTypeEnum.Object;
            }
            else if (ReferenceEquals(type, Converter.s_typeofStringArray))
            {
                binaryTypeEnum = BitBinaryTypeEnum.StringArray;
            }
            else if (ReferenceEquals(type, Converter.s_typeofObjectArray))
            {
                binaryTypeEnum = BitBinaryTypeEnum.ObjectArray;
            }
            else if (Converter.IsPrimitiveArray(type, out typeInformation))
            {
                binaryTypeEnum = BitBinaryTypeEnum.PrimitiveArray;
            }
            else
            {
                BitInternalPrimitiveTypeE primitiveTypeEnum = objectWriter.ToCode(type);
                switch (primitiveTypeEnum)
                {
                    case BitInternalPrimitiveTypeE.Invalid:
                        string? assembly;
                        if (objectInfo == null)
                        {
                            assembly = type.Assembly.FullName;
                            typeInformation = type.FullName;
                        }
                        else
                        {
                            assembly = objectInfo.GetAssemblyString();
                            typeInformation = objectInfo.GetTypeFullName();
                        }

                        Debug.Assert(assembly != null);
                        if (assembly.Equals(Converter.s_urt_CoreLib_AssemblyString) || assembly.Equals(Converter.s_urt_mscorlib_AssemblyString))
                        {
                            binaryTypeEnum = BitBinaryTypeEnum.ObjectUrt;
                            assemId = 0;
                        }
                        else
                        {
                            binaryTypeEnum = BitBinaryTypeEnum.ObjectUser;
                            Debug.Assert(objectInfo != null, "[BinaryConverter.GetBinaryTypeInfo]objectInfo null for user object");
                            assemId = (int)objectInfo._assemId;
                            if (assemId == 0)
                            {
                                throw new BitSerializationException(SR.Format(SR.Serialization_AssemblyId, typeInformation));
                            }
                        }
                        break;
                    default:
                        binaryTypeEnum = BitBinaryTypeEnum.Primitive;
                        typeInformation = primitiveTypeEnum;
                        break;
                }
            }

            return binaryTypeEnum;
        }

        // Used for non Si types when Parsing
        internal static BitBinaryTypeEnum GetParserBinaryTypeInfo(Type type, out object? typeInformation, StreamingContext context)
        {
            BitBinaryTypeEnum binaryTypeEnum;
            typeInformation = null;

            if (ReferenceEquals(type, Converter.s_typeofString))
            {
                binaryTypeEnum = BitBinaryTypeEnum.String;
            }
            else if (ReferenceEquals(type, Converter.s_typeofObject))
            {
                binaryTypeEnum = BitBinaryTypeEnum.Object;
            }
            else if (ReferenceEquals(type, Converter.s_typeofObjectArray))
            {
                binaryTypeEnum = BitBinaryTypeEnum.ObjectArray;
            }
            else if (ReferenceEquals(type, Converter.s_typeofStringArray))
            {
                binaryTypeEnum = BitBinaryTypeEnum.StringArray;
            }
            else if (Converter.IsPrimitiveArray(type, out typeInformation))
            {
                binaryTypeEnum = BitBinaryTypeEnum.PrimitiveArray;
            }
            else
            {
                BitInternalPrimitiveTypeE primitiveTypeEnum = Converter.ToCode(type);
                switch (primitiveTypeEnum)
                {
                    case BitInternalPrimitiveTypeE.Invalid:
                        binaryTypeEnum = type.Assembly == Converter.s_urt_CoreLib_Assembly ?
                            BitBinaryTypeEnum.ObjectUrt :
                            BitBinaryTypeEnum.ObjectUser;
                        typeInformation = type.FullName;
                        break;
                    default:
                        binaryTypeEnum = BitBinaryTypeEnum.Primitive;
                        typeInformation = primitiveTypeEnum;
                        break;
                }
            }

            return binaryTypeEnum;
        }

        // Writes the type information on the wire
        internal static void WriteTypeInfo(BitBinaryTypeEnum binaryTypeEnum, object? typeInformation, int assemId, BinaryFormatterWriter output)
        {
            switch (binaryTypeEnum)
            {
                case BitBinaryTypeEnum.Primitive:
                case BitBinaryTypeEnum.PrimitiveArray:
                    Debug.Assert(typeInformation != null, "[BinaryConverter.WriteTypeInfo]typeInformation!=null");
                    output.WriteByte((byte)((BitInternalPrimitiveTypeE)typeInformation));
                    break;
                case BitBinaryTypeEnum.String:
                case BitBinaryTypeEnum.Object:
                case BitBinaryTypeEnum.StringArray:
                case BitBinaryTypeEnum.ObjectArray:
                    break;
                case BitBinaryTypeEnum.ObjectUrt:
                    Debug.Assert(typeInformation != null, "[BinaryConverter.WriteTypeInfo]typeInformation!=null");
                    output.WriteString(typeInformation.ToString()!);
                    break;
                case BitBinaryTypeEnum.ObjectUser:
                    Debug.Assert(typeInformation != null, "[BinaryConverter.WriteTypeInfo]typeInformation!=null");
                    output.WriteString(typeInformation.ToString()!);
                    output.WriteInt32(assemId);
                    break;
                default:
                    throw new BitSerializationException(SR.Format(SR.Serialization_TypeWrite, binaryTypeEnum.ToString()));
            }
        }

        // Reads the type information from the wire
        internal static object ReadTypeInfo(BitBinaryTypeEnum binaryTypeEnum, BinaryParser input, out int assemId)
        {
            object var = null!;
            int readAssemId = 0;

            switch (binaryTypeEnum)
            {
                case BitBinaryTypeEnum.Primitive:
                case BitBinaryTypeEnum.PrimitiveArray:
                    var = (BitInternalPrimitiveTypeE)input.ReadByte();
                    break;
                case BitBinaryTypeEnum.String:
                case BitBinaryTypeEnum.Object:
                case BitBinaryTypeEnum.StringArray:
                case BitBinaryTypeEnum.ObjectArray:
                    break;
                case BitBinaryTypeEnum.ObjectUrt:
                    var = input.ReadString();
                    break;
                case BitBinaryTypeEnum.ObjectUser:
                    var = input.ReadString();
                    readAssemId = input.ReadInt32();
                    break;
                default:
                    throw new BitSerializationException(SR.Format(SR.Serialization_TypeRead, binaryTypeEnum.ToString()));
            }
            assemId = readAssemId;
            return var;
        }

        // Given the wire type information, returns the actual type and additional information
        [RequiresUnreferencedCode("Types might be removed")]
        internal static void TypeFromInfo(BitBinaryTypeEnum binaryTypeEnum,
                                          object? typeInformation,
                                          ObjectReader objectReader,
                                          BinaryAssemblyInfo? assemblyInfo,
                                          out BitInternalPrimitiveTypeE primitiveTypeEnum,
                                          out string? typeString,
                                          out Type? type,
                                          out bool isVariant)
        {
            isVariant = false;
            primitiveTypeEnum = BitInternalPrimitiveTypeE.Invalid;
            typeString = null;
            type = null;

            switch (binaryTypeEnum)
            {
                case BitBinaryTypeEnum.Primitive:
                    primitiveTypeEnum = (BitInternalPrimitiveTypeE)typeInformation!;
                    typeString = Converter.ToComType(primitiveTypeEnum);
                    type = Converter.ToType(primitiveTypeEnum);
                    break;
                case BitBinaryTypeEnum.String:
                    type = Converter.s_typeofString;
                    break;
                case BitBinaryTypeEnum.Object:
                    type = Converter.s_typeofObject;
                    isVariant = true;
                    break;
                case BitBinaryTypeEnum.ObjectArray:
                    type = Converter.s_typeofObjectArray;
                    break;
                case BitBinaryTypeEnum.StringArray:
                    type = Converter.s_typeofStringArray;
                    break;
                case BitBinaryTypeEnum.PrimitiveArray:
                    primitiveTypeEnum = (BitInternalPrimitiveTypeE)typeInformation!;
                    type = Converter.ToArrayType(primitiveTypeEnum);
                    break;
                case BitBinaryTypeEnum.ObjectUser:
                case BitBinaryTypeEnum.ObjectUrt:
                    if (typeInformation != null)
                    {
                        typeString = typeInformation.ToString();
                        type = objectReader.GetType(assemblyInfo!, typeString!);
                        if (ReferenceEquals(type, Converter.s_typeofObject))
                        {
                            isVariant = true;
                        }
                    }
                    break;
                default:
                    throw new BitSerializationException(SR.Format(SR.Serialization_TypeRead, binaryTypeEnum.ToString()));
            }
        }
    }
}
