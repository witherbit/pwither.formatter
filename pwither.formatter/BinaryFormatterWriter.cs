﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace pwither.formatter
{
    internal sealed class BinaryFormatterWriter
    {
        private const int ChunkSize = 4096;

        private readonly Stream _outputStream;
        private readonly BitFormatterTypeStyle _formatterTypeStyle;
        private readonly ObjectWriter _objectWriter;
        private readonly BinaryWriter _dataWriter;

        private int _consecutiveNullArrayEntryCount;
        private Dictionary<string, ObjectMapInfo>? _objectMapTable;

        private BinaryObject? _binaryObject;
        private BinaryObjectWithMap? _binaryObjectWithMap;
        private BinaryObjectWithMapTyped? _binaryObjectWithMapTyped;
        private BinaryObjectString? _binaryObjectString;
        private BinaryArray? _binaryArray;
        private byte[]? _byteBuffer;
        private MemberPrimitiveUnTyped? _memberPrimitiveUnTyped;
        private MemberPrimitiveTyped? _memberPrimitiveTyped;
        private ObjectNull? _objectNull;
        private MemberReference? _memberReference;
        private BinaryAssembly? _binaryAssembly;

        internal BinaryFormatterWriter(Stream outputStream, ObjectWriter objectWriter, BitFormatterTypeStyle formatterTypeStyle)
        {
            _outputStream = outputStream;
            _formatterTypeStyle = formatterTypeStyle;
            _objectWriter = objectWriter;
            _dataWriter = new BinaryWriter(outputStream, Encoding.UTF8);
        }

        internal void WriteBegin() { }

        internal void WriteEnd()
        {
            _dataWriter.Flush();
        }

        internal void WriteBoolean(bool value) => _dataWriter.Write(value);

        internal void WriteByte(byte value) => _dataWriter.Write(value);

        private void WriteBytes(byte[] value) => _dataWriter.Write(value);

        private void WriteBytes(byte[] byteA, int offset, int size) => _dataWriter.Write(byteA, offset, size);

        internal void WriteChar(char value) => _dataWriter.Write(value);

        internal void WriteChars(char[] value) => _dataWriter.Write(value);

        internal void WriteDecimal(decimal value) => WriteString(value.ToString(CultureInfo.InvariantCulture));

        internal void WriteSingle(float value) => _dataWriter.Write(value);

        internal void WriteDouble(double value) => _dataWriter.Write(value);

        internal void WriteInt16(short value) => _dataWriter.Write(value);

        internal void WriteInt32(int value) => _dataWriter.Write(value);

        internal void WriteInt64(long value) => _dataWriter.Write(value);

        internal void WriteSByte(sbyte value) => WriteByte(unchecked((byte)value));

        internal void WriteString(string value) => _dataWriter.Write(value);

        internal void WriteTimeSpan(TimeSpan value) => WriteInt64(value.Ticks);

        internal void WriteDateTime(DateTime value)
        {
            // In .NET Framework, BinaryFormatter is able to access DateTime's ToBinaryRaw,
            // which just returns the value of its sole Int64 dateData field.  Here, we don't
            // have access to that member (which doesn't even exist anymore, since it was only for
            // BinaryFormatter, which is now in a separate assembly).  To address that,
            // we access the sole field directly via an unsafe cast.
            long dateData = Unsafe.As<DateTime, long>(ref value);
            WriteInt64(dateData);
        }

        internal void WriteUInt16(ushort value) => _dataWriter.Write(value);

        internal void WriteUInt32(uint value) => _dataWriter.Write(value);

        internal void WriteUInt64(ulong value) => _dataWriter.Write(value);

        internal void WriteObjectEnd() 
        {
            // nop
        }

        internal void WriteSerializationHeaderEnd()
        {
            var record = new MessageEnd();
            record.Write(this);
        }

        internal void WriteSerializationHeader(int topId, int headerId, int minorVersion, int majorVersion)
        {
            var record = new SerializationHeaderRecord(BitBinaryHeaderEnum.SerializedStreamHeader, topId, headerId, minorVersion, majorVersion);
            record.Write(this);
        }

        /// <summary>
        /// return true if BinaryObjectWithMapTyped. If true, BinaryTypeEnum is also set
        /// </summary>
        internal void WriteObject(TypeInfo memberInfo, TypeInfo dataInfo, int numMembers, string[] memberNames, Type[] memberTypes, WriteObjectInfo?[] memberObjectInfos)
        {
            InternalWriteItemNull();
            int assemId;
            int objectId = (int)memberInfo._objectId;

            Debug.Assert(dataInfo != null); // Explicitly called with null. Potential bug, but closed as Won't Fix: https://github.com/dotnet/runtime/issues/31402
            string? objectName = objectId < 0 ?
                dataInfo.NIname : // Nested Object
                memberInfo.NIname; // Non-Nested

            _objectMapTable ??= new Dictionary<string, ObjectMapInfo>();

            Debug.Assert(objectName != null);
            if (_objectMapTable.TryGetValue(objectName, out ObjectMapInfo? objectMapInfo) &&
                objectMapInfo.IsCompatible(numMembers, memberNames, memberTypes))
            {
                // Object
                _binaryObject ??= new BinaryObject();

                _binaryObject.Set(objectId, objectMapInfo._objectId);
                _binaryObject.Write(this);
            }
            else if (!dataInfo._transmitTypeOnObject)
            {
                // ObjectWithMap
                _binaryObjectWithMap ??= new BinaryObjectWithMap();

                // BCL types are not placed into table
                assemId = (int)dataInfo._assemId;
                _binaryObjectWithMap.Set(objectId, objectName, numMembers, memberNames, assemId);

                _binaryObjectWithMap.Write(this);
                if (objectMapInfo == null)
                {
                    _objectMapTable.Add(objectName, new ObjectMapInfo(objectId, numMembers, memberNames, memberTypes));
                }
            }
            else // typeNameInfo._transmitTypeOnObject is true
            {
                // ObjectWithMapTyped
                var binaryTypeEnumA = new BitBinaryTypeEnum[numMembers];
                var typeInformationA = new object?[numMembers];
                var assemIdA = new int[numMembers];
                for (int i = 0; i < numMembers; i++)
                {
                    object? typeInformation;
                    binaryTypeEnumA[i] = BinaryTypeConverter.GetBinaryTypeInfo(memberTypes[i], memberObjectInfos[i], null, _objectWriter,
                        out typeInformation, out assemId);
                    typeInformationA[i] = typeInformation;
                    assemIdA[i] = assemId;
                }

                _binaryObjectWithMapTyped ??= new BinaryObjectWithMapTyped();

                // BCL types are not placed in table
                assemId = (int)dataInfo._assemId;
                _binaryObjectWithMapTyped.Set(objectId, objectName, numMembers, memberNames, binaryTypeEnumA, typeInformationA, assemIdA, assemId);
                _binaryObjectWithMapTyped.Write(this);
                if (objectMapInfo == null)
                {
                    _objectMapTable.Add(objectName, new ObjectMapInfo(objectId, numMembers, memberNames, memberTypes));
                }
            }
        }

        internal void WriteObjectString(int objectId, string? value)
        {
            InternalWriteItemNull();

            _binaryObjectString ??= new BinaryObjectString();

            _binaryObjectString.Set(objectId, value);
            _binaryObjectString.Write(this);
        }

        internal void WriteSingleArray(TypeInfo arrayInfo, WriteObjectInfo? objectInfo, TypeInfo arrayElemInfo, int length, int lowerBound, Array array)
        {
            InternalWriteItemNull();
            BitBinaryArrayTypeEnum binaryArrayTypeEnum;
            var lengthA = new int[1];
            lengthA[0] = length;
            int[]? lowerBoundA = null;
            object? typeInformation;

            if (lowerBound == 0)
            {
                binaryArrayTypeEnum = BitBinaryArrayTypeEnum.Single;
            }
            else
            {
                binaryArrayTypeEnum = BitBinaryArrayTypeEnum.SingleOffset;
                lowerBoundA = new int[1];
                lowerBoundA[0] = lowerBound;
            }

            int assemId;
            BitBinaryTypeEnum binaryTypeEnum = BinaryTypeConverter.GetBinaryTypeInfo(
                arrayElemInfo._type!, objectInfo, arrayElemInfo.NIname, _objectWriter,
                out typeInformation, out assemId);

            _binaryArray ??= new BinaryArray();
            _binaryArray.Set((int)arrayInfo._objectId, 1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, binaryArrayTypeEnum, assemId);

            _binaryArray.Write(this);

            if (Converter.IsWriteAsByteArray(arrayElemInfo._primitiveTypeEnum) && (lowerBound == 0))
            {
                //array is written out as an array of bytes
                if (arrayElemInfo._primitiveTypeEnum == BitInternalPrimitiveTypeE.Byte)
                {
                    WriteBytes((byte[])array);
                }
                else if (arrayElemInfo._primitiveTypeEnum == BitInternalPrimitiveTypeE.Char)
                {
                    WriteChars((char[])array);
                }
                else
                {
                    WriteArrayAsBytes(array, Converter.TypeLength(arrayElemInfo._primitiveTypeEnum));
                }
            }
        }

        private void WriteArrayAsBytes(Array array, int typeLength)
        {
            InternalWriteItemNull();
            int arrayOffset = 0;
            _byteBuffer ??= new byte[ChunkSize];

            while (arrayOffset < array.Length)
            {
                int numArrayItems = Math.Min(ChunkSize / typeLength, array.Length - arrayOffset);
                int bufferUsed = numArrayItems * typeLength;
                Buffer.BlockCopy(array, arrayOffset * typeLength, _byteBuffer, 0, bufferUsed);
                if (!BitConverter.IsLittleEndian)
                {
                    // we know that we are writing a primitive type, so just do a simple swap
                    for (int i = 0; i < bufferUsed; i += typeLength)
                    {
                        for (int j = 0; j < typeLength / 2; j++)
                        {
                            byte tmp = _byteBuffer[i + j];
                            _byteBuffer[i + j] = _byteBuffer[i + typeLength - 1 - j];
                            _byteBuffer[i + typeLength - 1 - j] = tmp;
                        }
                    }
                }
                WriteBytes(_byteBuffer, 0, bufferUsed);
                arrayOffset += numArrayItems;
            }
        }

        internal void WriteJaggedArray(TypeInfo arrayInfo, WriteObjectInfo? objectInfo, TypeInfo arrayElemInfo, int length, int lowerBound)
        {
            InternalWriteItemNull();
            BitBinaryArrayTypeEnum binaryArrayTypeEnum;
            var lengthA = new int[1];
            lengthA[0] = length;
            int[]? lowerBoundA = null;
            object? typeInformation;
            int assemId;

            if (lowerBound == 0)
            {
                binaryArrayTypeEnum = BitBinaryArrayTypeEnum.Jagged;
            }
            else
            {
                binaryArrayTypeEnum = BitBinaryArrayTypeEnum.JaggedOffset;
                lowerBoundA = new int[1];
                lowerBoundA[0] = lowerBound;
            }

            BitBinaryTypeEnum binaryTypeEnum = BinaryTypeConverter.GetBinaryTypeInfo(arrayElemInfo._type!, objectInfo, arrayElemInfo.NIname, 
                _objectWriter, out typeInformation, out assemId);

            _binaryArray ??= new BinaryArray();
            _binaryArray.Set((int)arrayInfo._objectId, 1, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, binaryArrayTypeEnum, assemId);

            _binaryArray.Write(this);
        }

        internal void WriteRectangleArray(TypeInfo arrayInfo, WriteObjectInfo? objectInfo, TypeInfo arrayElemInfo, int rank, int[] lengthA, int[] lowerBoundA)
        {
            InternalWriteItemNull();

            BitBinaryArrayTypeEnum binaryArrayTypeEnum = BitBinaryArrayTypeEnum.Rectangular;
            object? typeInformation;
            int assemId;
            BitBinaryTypeEnum binaryTypeEnum = BinaryTypeConverter.GetBinaryTypeInfo(arrayElemInfo._type!, objectInfo, arrayElemInfo.NIname,
                _objectWriter, out typeInformation, out assemId);

            _binaryArray ??= new BinaryArray();

            for (int i = 0; i < rank; i++)
            {
                if (lowerBoundA[i] != 0)
                {
                    binaryArrayTypeEnum = BitBinaryArrayTypeEnum.RectangularOffset;
                    break;
                }
            }

            _binaryArray.Set((int)arrayInfo._objectId, rank, lengthA, lowerBoundA, binaryTypeEnum, typeInformation, binaryArrayTypeEnum, assemId);
            _binaryArray.Write(this);
        }

        internal void WriteObjectByteArray(TypeInfo arrayInfo, WriteObjectInfo? objectInfo, TypeInfo arrayElemInfo, int length, int lowerBound, byte[] byteA)
        {
            InternalWriteItemNull();
            WriteSingleArray(arrayInfo, objectInfo, arrayElemInfo, length, lowerBound, byteA);
        }

        internal void WriteMember(TypeInfo memberInfo, TypeInfo dataInfo, object value)
        {
            InternalWriteItemNull();
            BitInternalPrimitiveTypeE typeInformation = dataInfo._primitiveTypeEnum;

            //if (typeNameInfo._transmitTypeOnMember) ....seems more correct...
//            Func<bool> transmitType = () => TraceFlags.IConvertibleFixArrayAlt2 ? typeNameInfo._transmitTypeOnMember : memberNameInfo._transmitTypeOnMember;
            // FIXME: weird logic. we are writing objects but check for setting in member.

            // Writes Members with primitive values
            if (memberInfo._transmitTypeOnMember)
            {
                _memberPrimitiveTyped ??= new MemberPrimitiveTyped();
                _memberPrimitiveTyped.Set(typeInformation, value);
                _memberPrimitiveTyped.Write(this);
            }
            else
            {
                _memberPrimitiveUnTyped ??= new MemberPrimitiveUnTyped();
                _memberPrimitiveUnTyped.Set(typeInformation, value);
                _memberPrimitiveUnTyped.Write(this);
            }
        }

        internal void WriteNullMember(TypeInfo memberInfo)
        {
            InternalWriteItemNull();
            _objectNull ??= new ObjectNull();

            if (!memberInfo._isArrayItem)
            {
                _objectNull.SetNullCount(1);
                _objectNull.Write(this);
                _consecutiveNullArrayEntryCount = 0;
            }
        }

        internal void WriteMemberObjectRef(int idRef)
        {
            InternalWriteItemNull();
            _memberReference ??= new MemberReference();
            _memberReference.Set(idRef);
            _memberReference.Write(this);
        }

        internal void WriteMemberNested()
        {
            InternalWriteItemNull();
        }

        internal void WriteMemberString(TypeInfo dataInfo, string? value)
        {
            InternalWriteItemNull();
            WriteObjectString((int)dataInfo._objectId, value);
        }

        internal void WriteItem(TypeInfo memberInfo, TypeInfo dataInfo, object value)
        {
            InternalWriteItemNull();
            WriteMember(memberInfo, dataInfo, value);
        }

        internal void WriteNullItem()
        {
            _consecutiveNullArrayEntryCount++;
            InternalWriteItemNull();
        }

        internal void WriteDelayedNullItem()
        {
            _consecutiveNullArrayEntryCount++;
        }

        internal void WriteItemEnd() => InternalWriteItemNull();

        private void InternalWriteItemNull()
        {
            if (_consecutiveNullArrayEntryCount > 0)
            {
                _objectNull ??= new ObjectNull();
                _objectNull.SetNullCount(_consecutiveNullArrayEntryCount);
                _objectNull.Write(this);
                _consecutiveNullArrayEntryCount = 0;
            }
        }

        internal void WriteItemObjectRef(int idRef)
        {
            InternalWriteItemNull();
            WriteMemberObjectRef(idRef);
        }

        internal void WriteAssembly(string assemblyString, int assemId, bool isNew)
        {
            //If the file being tested wasn't built as an assembly, then we're going to get null back
            //for the assembly name.  This is very unfortunate.
            InternalWriteItemNull();
            assemblyString ??= string.Empty;

            if (isNew)
            {
                _binaryAssembly ??= new BinaryAssembly();
                _binaryAssembly.Set(assemId, assemblyString);
                _binaryAssembly.Write(this);
            }
        }

        // Method to write a value onto a stream given its primitive type code
        internal void WriteValue(BitInternalPrimitiveTypeE code, object? value)
        {
            switch (code)
            {
                case BitInternalPrimitiveTypeE.Boolean: WriteBoolean(Convert.ToBoolean(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Byte: WriteByte(Convert.ToByte(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Char: WriteChar(Convert.ToChar(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Double: WriteDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Int16: WriteInt16(Convert.ToInt16(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Int32: WriteInt32(Convert.ToInt32(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Int64: WriteInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.SByte: WriteSByte(Convert.ToSByte(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Single: WriteSingle(Convert.ToSingle(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.UInt16: WriteUInt16(Convert.ToUInt16(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.UInt32: WriteUInt32(Convert.ToUInt32(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.UInt64: WriteUInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.Decimal: WriteDecimal(Convert.ToDecimal(value, CultureInfo.InvariantCulture)); break;
                case BitInternalPrimitiveTypeE.TimeSpan: WriteTimeSpan((TimeSpan)value!); break;
                case BitInternalPrimitiveTypeE.DateTime: WriteDateTime((DateTime)value!); break;
                default: throw new BitSerializationException(SR.Format(SR.Serialization_TypeCode, code.ToString()));
            }
        }

        private sealed class ObjectMapInfo
        {
            internal readonly int _objectId;
            private readonly int _numMembers;
            private readonly string[] _memberNames;
            private readonly Type[] _memberTypes;

            internal ObjectMapInfo(int objectId, int numMembers, string[] memberNames, Type[] memberTypes)
            {
                _objectId = objectId;
                _numMembers = numMembers;
                _memberNames = memberNames;
                _memberTypes = memberTypes;
            }

            internal bool IsCompatible(int numMembers, string[] memberNames, Type[]? memberTypes)
            {
                if (_numMembers != numMembers)
                {
                    return false;
                }

                for (int i = 0; i < numMembers; i++)
                {
                    if (!(_memberNames[i].Equals(memberNames[i])))
                    {
                        return false;
                    }

                    if ((memberTypes != null) && (_memberTypes[i] != memberTypes[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
