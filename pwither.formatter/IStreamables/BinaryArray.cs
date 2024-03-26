// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace pwither.formatter
{
    internal sealed class BinaryArray : IStreamable
    {
        internal int _objectId;
        internal int _rank;
        internal int[]? _lengthA;
        internal int[]? _lowerBoundA;
        internal BitBinaryTypeEnum _binaryTypeEnum;
        internal object? _typeInformation;
        internal int _assemId;
        private BitBinaryHeaderEnum _binaryHeaderEnum;
        internal BitBinaryArrayTypeEnum _binaryArrayTypeEnum;

        internal BinaryArray() { }

        internal BinaryArray(BitBinaryHeaderEnum binaryHeaderEnum)
        {
            _binaryHeaderEnum = binaryHeaderEnum;
        }

        internal void Set(int objectId, int rank, int[] lengthA, int[]? lowerBoundA, BitBinaryTypeEnum binaryTypeEnum, object? typeInformation, BitBinaryArrayTypeEnum binaryArrayTypeEnum, int assemId)
        {
            _objectId = objectId;
            _binaryArrayTypeEnum = binaryArrayTypeEnum;
            _rank = rank;
            _lengthA = lengthA;
            _lowerBoundA = lowerBoundA;
            _binaryTypeEnum = binaryTypeEnum;
            _typeInformation = typeInformation;
            _assemId = assemId;

            _binaryHeaderEnum = BitBinaryHeaderEnum.Array;
            if (binaryArrayTypeEnum == BitBinaryArrayTypeEnum.Single)
            {
                if (binaryTypeEnum == BitBinaryTypeEnum.Primitive)
                {
                    _binaryHeaderEnum = BitBinaryHeaderEnum.ArraySinglePrimitive;
                }
                else if (binaryTypeEnum == BitBinaryTypeEnum.String)
                {
                    _binaryHeaderEnum = BitBinaryHeaderEnum.ArraySingleString;
                }
                else if (binaryTypeEnum == BitBinaryTypeEnum.Object)
                {
                    _binaryHeaderEnum = BitBinaryHeaderEnum.ArraySingleObject;
                }
            }
        }

        public void Write(BinaryFormatterWriter output)
        {
            Debug.Assert(_lengthA != null);
            switch (_binaryHeaderEnum)
            {
                case BitBinaryHeaderEnum.ArraySinglePrimitive:
                    output.WriteByte((byte)_binaryHeaderEnum);
                    output.WriteInt32(_objectId);
                    output.WriteInt32(_lengthA[0]);
                    output.WriteByte((byte)((BitInternalPrimitiveTypeE)_typeInformation!));
                    break;
                case BitBinaryHeaderEnum.ArraySingleString:
                    output.WriteByte((byte)_binaryHeaderEnum);
                    output.WriteInt32(_objectId);
                    output.WriteInt32(_lengthA[0]);
                    break;
                case BitBinaryHeaderEnum.ArraySingleObject:
                    output.WriteByte((byte)_binaryHeaderEnum);
                    output.WriteInt32(_objectId);
                    output.WriteInt32(_lengthA[0]);
                    break;
                default:
                    output.WriteByte((byte)_binaryHeaderEnum);
                    output.WriteInt32(_objectId);
                    output.WriteByte((byte)_binaryArrayTypeEnum);
                    output.WriteInt32(_rank);
                    for (int i = 0; i < _rank; i++)
                    {
                        output.WriteInt32(_lengthA[i]);
                    }
                    if ((_binaryArrayTypeEnum == BitBinaryArrayTypeEnum.SingleOffset) ||
                        (_binaryArrayTypeEnum == BitBinaryArrayTypeEnum.JaggedOffset) ||
                        (_binaryArrayTypeEnum == BitBinaryArrayTypeEnum.RectangularOffset))
                    {
                        Debug.Assert(_lowerBoundA != null);
                        for (int i = 0; i < _rank; i++)
                        {
                            output.WriteInt32(_lowerBoundA[i]);
                        }
                    }
                    output.WriteByte((byte)_binaryTypeEnum);
                    BinaryTypeConverter.WriteTypeInfo(_binaryTypeEnum, _typeInformation, _assemId, output);
                    break;
            }
        }

        public void Read(BinaryParser input)
        {
            switch (_binaryHeaderEnum)
            {
                case BitBinaryHeaderEnum.ArraySinglePrimitive:
                    _objectId = input.ReadInt32();
                    _lengthA = new int[1];
                    _lengthA[0] = input.ReadInt32();
                    _binaryArrayTypeEnum = BitBinaryArrayTypeEnum.Single;
                    _rank = 1;
                    _lowerBoundA = new int[_rank];
                    _binaryTypeEnum = BitBinaryTypeEnum.Primitive;
                    _typeInformation = (BitInternalPrimitiveTypeE)input.ReadByte();
                    break;
                case BitBinaryHeaderEnum.ArraySingleString:
                    _objectId = input.ReadInt32();
                    _lengthA = new int[1];
                    _lengthA[0] = input.ReadInt32();
                    _binaryArrayTypeEnum = BitBinaryArrayTypeEnum.Single;
                    _rank = 1;
                    _lowerBoundA = new int[_rank];
                    _binaryTypeEnum = BitBinaryTypeEnum.String;
                    _typeInformation = null;
                    break;
                case BitBinaryHeaderEnum.ArraySingleObject:
                    _objectId = input.ReadInt32();
                    _lengthA = new int[1];
                    _lengthA[0] = input.ReadInt32();
                    _binaryArrayTypeEnum = BitBinaryArrayTypeEnum.Single;
                    _rank = 1;
                    _lowerBoundA = new int[_rank];
                    _binaryTypeEnum = BitBinaryTypeEnum.Object;
                    _typeInformation = null;
                    break;
                default:
                    _objectId = input.ReadInt32();
                    _binaryArrayTypeEnum = (BitBinaryArrayTypeEnum)input.ReadByte();
                    _rank = input.ReadInt32();
                    _lengthA = new int[_rank];
                    _lowerBoundA = new int[_rank];
                    for (int i = 0; i < _rank; i++)
                    {
                        _lengthA[i] = input.ReadInt32();
                    }
                    if ((_binaryArrayTypeEnum == BitBinaryArrayTypeEnum.SingleOffset) ||
                        (_binaryArrayTypeEnum == BitBinaryArrayTypeEnum.JaggedOffset) ||
                        (_binaryArrayTypeEnum == BitBinaryArrayTypeEnum.RectangularOffset))
                    {
                        for (int i = 0; i < _rank; i++)
                        {
                            _lowerBoundA[i] = input.ReadInt32();
                        }
                    }
                    _binaryTypeEnum = (BitBinaryTypeEnum)input.ReadByte();
                    _typeInformation = BinaryTypeConverter.ReadTypeInfo(_binaryTypeEnum, input, out _assemId);
                    break;
            }
        }
    }
}
