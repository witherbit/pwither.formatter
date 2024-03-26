// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace pwither.formatter
{
    internal sealed class ParseRecord
    {
        // Enums
        internal BitInternalParseTypeE _parseTypeEnum = BitInternalParseTypeE.Empty;
        internal BitInternalObjectTypeE _objectTypeEnum = BitInternalObjectTypeE.Empty;
        internal BitInternalArrayTypeE _arrayTypeEnum = BitInternalArrayTypeE.Empty;
        internal BitInternalMemberTypeE _memberTypeEnum = BitInternalMemberTypeE.Empty;
        internal BitInternalMemberValueE _memberValueEnum = BitInternalMemberValueE.Empty;
        internal BitInternalObjectPositionE _objectPositionEnum = BitInternalObjectPositionE.Empty;

        // Object
        internal string? _name;

        // Value
        internal string? _value;
        internal object? _varValue;

        // dt attribute
        internal string? _keyDt;
        internal Type? _dtType;
        internal BitInternalPrimitiveTypeE _dtTypeCode;

        // Object ID
        internal long _objectId;

        // Reference ID
        internal long _idRef;

        // Array

        // Array Element Type
        internal string? _arrayElementTypeString;
        internal Type? _arrayElementType;
        internal bool _isArrayVariant;
        internal BitInternalPrimitiveTypeE _arrayElementTypeCode;

        // Parsed array information
        internal int _rank;
        internal int[]? _lengthA;
        internal int[]? _lowerBoundA;

        // Array map for placing array elements in array
        internal int[]? _indexMap;
        internal int _memberIndex;
        internal int _linearlength;
        internal int[]? _rectangularMap;
        internal bool _isLowerBound;

        // MemberInfo accumulated during parsing of members

        internal ReadObjectInfo? _objectInfo;

        // ValueType Fixup needed
        internal bool _isValueTypeFixup;

        // Created object
        internal object? _newObj;
        internal object?[]? _objectA; //optimization, will contain object[]
        internal PrimitiveArray? _primitiveArray; // for Primitive Soap arrays, optimization
        internal bool _isRegistered; // Used when registering nested classes
        internal object?[]? _memberData; // member data is collected here before populating
        internal SerializationInfo? _si;

        internal int _consecutiveNullArrayEntryCount;

        internal ParseRecord() { }

        // Initialize ParseRecord. Called when reusing.
        internal void Init()
        {
            // Enums
            _parseTypeEnum = BitInternalParseTypeE.Empty;
            _objectTypeEnum = BitInternalObjectTypeE.Empty;
            _arrayTypeEnum = BitInternalArrayTypeE.Empty;
            _memberTypeEnum = BitInternalMemberTypeE.Empty;
            _memberValueEnum = BitInternalMemberValueE.Empty;
            _objectPositionEnum = BitInternalObjectPositionE.Empty;

            // Object
            _name = null;

            // Value
            _value = null;

            // dt attribute
            _keyDt = null;
            _dtType = null;
            _dtTypeCode = BitInternalPrimitiveTypeE.Invalid;

            // Object ID
            _objectId = 0;

            // Reference ID
            _idRef = 0;

            // Array

            // Array Element Type
            _arrayElementTypeString = null;
            _arrayElementType = null;
            _isArrayVariant = false;
            _arrayElementTypeCode = BitInternalPrimitiveTypeE.Invalid;

            // Parsed array information
            _rank = 0;
            _lengthA = null;
            _lowerBoundA = null;

            // Array map for placing array elements in array
            _indexMap = null;
            _memberIndex = 0;
            _linearlength = 0;
            _rectangularMap = null;
            _isLowerBound = false;

            // ValueType Fixup needed
            _isValueTypeFixup = false;

            _newObj = null;
            _objectA = null;
            _primitiveArray = null;
            _objectInfo = null;
            _isRegistered = false;
            _memberData = null;
            _si = null;

            _consecutiveNullArrayEntryCount = 0;
        }
    }

    // Implements a stack used for parsing
    internal sealed class SerStack
    {
        internal object?[] _objects = new object?[5];
        internal string _stackId;
        internal int _top = -1;

        internal SerStack(string stackId)
        {
            _stackId = stackId;
        }

        // Push the object onto the stack
        internal void Push(object obj)
        {
            if (_top == (_objects.Length - 1))
            {
                IncreaseCapacity();
            }
            _objects[++_top] = obj;
        }

        // Pop the object from the stack
        internal object? Pop()
        {
            if (_top < 0)
            {
                return null;
            }

            object? obj = _objects[_top];
            _objects[_top--] = null;
            return obj;
        }

        internal void IncreaseCapacity()
        {
            int size = _objects.Length * 2;
            object[] newItems = new object[size];
            Array.Copy(_objects, newItems, _objects.Length);
            _objects = newItems;
        }

        // Gets the object on the top of the stack
        internal object? Peek() => _top < 0 ? null : _objects[_top];

        // Gets the second entry in the stack.
        internal object? PeekPeek() => _top < 1 ? null : _objects[_top - 1];

        // The number of entries in the stack
        internal bool IsEmpty() => _top <= 0;
    }

    // Implements a Growable array
    internal sealed class SizedArray : ICloneable
    {
        internal object?[] _objects;
        internal object?[] _negObjects;

        internal SizedArray()
        {
            _objects = new object[16];
            _negObjects = new object[4];
        }

        internal SizedArray(int length)
        {
            _objects = new object[length];
            _negObjects = new object[length];
        }

        private SizedArray(SizedArray sizedArray)
        {
            _objects = new object[sizedArray._objects.Length];
            sizedArray._objects.CopyTo(_objects, 0);
            _negObjects = new object[sizedArray._negObjects.Length];
            sizedArray._negObjects.CopyTo(_negObjects, 0);
        }

        public object Clone() => new SizedArray(this);

        internal object? this[int index]
        {
            get
            {
                if (index < 0)
                {
                    return -index > _negObjects.Length - 1 ? null : _negObjects[-index];
                }
                else
                {
                    return index > _objects.Length - 1 ? null : _objects[index];
                }
            }
            set
            {
                if (index < 0)
                {
                    if (-index > _negObjects.Length - 1)
                    {
                        IncreaseCapacity(index);
                    }
                    _negObjects[-index] = value;
                }
                else
                {
                    if (index > _objects.Length - 1)
                    {
                        IncreaseCapacity(index);
                    }
                    _objects[index] = value;
                }
            }
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int size = Math.Max(_negObjects.Length * 2, (-index) + 1);
                    object[] newItems = new object[size];
                    Array.Copy(_negObjects, newItems, _negObjects.Length);
                    _negObjects = newItems;
                }
                else
                {
                    int size = Math.Max(_objects.Length * 2, index + 1);
                    object[] newItems = new object[size];
                    Array.Copy(_objects, newItems, _objects.Length);
                    _objects = newItems;
                }
            }
            catch (Exception)
            {
                throw new BitSerializationException(SR.Serialization_CorruptedStream);
            }
        }
    }

    internal sealed class IntSizedArray : ICloneable
    {
        internal int[] _objects = new int[16];
        internal int[] _negObjects = new int[4];

        public IntSizedArray() { }

        private IntSizedArray(IntSizedArray sizedArray)
        {
            _objects = new int[sizedArray._objects.Length];
            sizedArray._objects.CopyTo(_objects, 0);
            _negObjects = new int[sizedArray._negObjects.Length];
            sizedArray._negObjects.CopyTo(_negObjects, 0);
        }

        public object Clone() => new IntSizedArray(this);

        internal int this[int index]
        {
            get
            {
                if (index < 0)
                {
                    return -index > _negObjects.Length - 1 ? 0 : _negObjects[-index];
                }
                else
                {
                    return index > _objects.Length - 1 ? 0 : _objects[index];
                }
            }
            set
            {
                if (index < 0)
                {
                    if (-index > _negObjects.Length - 1)
                    {
                        IncreaseCapacity(index);
                    }
                    _negObjects[-index] = value;
                }
                else
                {
                    if (index > _objects.Length - 1)
                    {
                        IncreaseCapacity(index);
                    }
                    _objects[index] = value;
                }
            }
        }

        internal void IncreaseCapacity(int index)
        {
            try
            {
                if (index < 0)
                {
                    int size = Math.Max(_negObjects.Length * 2, (-index) + 1);
                    int[] newItems = new int[size];
                    Array.Copy(_negObjects, newItems, _negObjects.Length);
                    _negObjects = newItems;
                }
                else
                {
                    int size = Math.Max(_objects.Length * 2, index + 1);
                    int[] newItems = new int[size];
                    Array.Copy(_objects, newItems, _objects.Length);
                    _objects = newItems;
                }
            }
            catch (Exception)
            {
                throw new BitSerializationException(SR.Serialization_CorruptedStream);
            }
        }
    }

    internal sealed class NameCache
    {
        private static readonly ConcurrentDictionary<string, object> s_ht = new ConcurrentDictionary<string, object>();
        private string? _name;

        internal object? GetCachedValue(string name)
        {
            _name = name;
            return s_ht.TryGetValue(name, out object? value) ? value : null;
        }

        internal void SetCachedValue(object value) => s_ht[_name!] = value;
    }


    // Used to fixup value types. Only currently used for valuetypes which are array items.
    internal sealed class ValueFixup
    {
        internal BitValueFixupEnum _valueFixupEnum = BitValueFixupEnum.Empty;
        internal Array? _arrayObj;
        internal int[]? _indexMap;
        internal object? _memberObject;
        internal ReadObjectInfo? _objectInfo;
        internal string? _memberName;

        internal ValueFixup(Array arrayObj, int[] indexMap)
        {
            _valueFixupEnum = BitValueFixupEnum.Array;
            _arrayObj = arrayObj;
            _indexMap = indexMap;
        }

        internal ValueFixup(object? memberObject, string memberName, ReadObjectInfo objectInfo)
        {
            _valueFixupEnum = BitValueFixupEnum.Member;
            _memberObject = memberObject;
            _memberName = memberName;
            _objectInfo = objectInfo;
        }

        internal void Fixup(ParseRecord record, ParseRecord parent)
        {
            object? obj = record._newObj;
            switch (_valueFixupEnum)
            {
                case BitValueFixupEnum.Array:
                    _arrayObj!.SetValue(obj, _indexMap!);
                    break;
                case BitValueFixupEnum.Header:
                    throw new PlatformNotSupportedException();
                case BitValueFixupEnum.Member:
                    Debug.Assert(_objectInfo!._objectManager != null);
                    if (_objectInfo._isSi)
                    {
                        _objectInfo._objectManager.RecordDelayedFixup(parent._objectId, _memberName!, record._objectId);
                    }
                    else
                    {
                        MemberInfo? memberInfo = _objectInfo.GetMemberInfo(_memberName);
                        if (memberInfo != null)
                        {
                            _objectInfo._objectManager.RecordFixup(parent._objectId, memberInfo, record._objectId);
                        }
                    }
                    break;
            }
        }
    }

    // Class used to transmit Enums from the XML and Binary Formatter class to the ObjectWriter and ObjectReader class
    internal sealed class InternalFE
    {
        internal BitFormatterTypeStyle _typeFormat;
        internal BitFormatterAssemblyStyle _assemblyFormat;
        internal BitTypeFilterLevel _securityLevel;
        internal BitInternalSerializerTypeE _serializerTypeEnum;
    }

    /// <summary>
    /// TODO: maybe split into MemberTypeInfo and DataTypeInfo classes?
    /// </summary>
    internal sealed class TypeInfo
    {
        internal string? _fullName; // Name from SerObjectInfo.GetType
        internal long _objectId;
        internal long _assemId;
        internal BitInternalPrimitiveTypeE _primitiveTypeEnum = BitInternalPrimitiveTypeE.Invalid;
        internal Type? _type;
        internal bool _isSealed;
        internal bool _isArray;
        internal bool _isArrayItem;
        internal bool _transmitTypeOnObject;
        internal bool _transmitTypeOnMember;
        internal bool _isParentTypeOnObject;
        internal BitInternalArrayTypeE _arrayEnum;
        private bool _sealedStatusChecked;

        internal TypeInfo() { }

        internal void Init()
        {
            _fullName = null;
            _objectId = 0;
            _assemId = 0;
            _primitiveTypeEnum = BitInternalPrimitiveTypeE.Invalid;
            _type = null;
            _isSealed = false;
            _transmitTypeOnObject = false;
            _transmitTypeOnMember = false;
            _isParentTypeOnObject = false;
            _isArray = false;
            _isArrayItem = false;
            _arrayEnum = BitInternalArrayTypeE.Empty;
            _sealedStatusChecked = false;
        }

        public bool IsSealed
        {
            get
            {
                if (!_sealedStatusChecked)
                {
                    Debug.Assert(_type != null);
                    _isSealed = _type.IsSealed;
                    _sealedStatusChecked = true;
                }
                return _isSealed;
            }
        }

        public string? NIname
        {
            get { return _fullName ??= _type?.FullName; }
            set { _fullName = value; }
        }
    }

    internal sealed class PrimitiveArray
    {
        private readonly BitInternalPrimitiveTypeE _code;
        private readonly bool[] _booleanA = null!;
        private readonly char[] _charA = null!;
        private readonly double[] _doubleA = null!;
        private readonly short[] _int16A = null!;
        private readonly int[] _int32A = null!;
        private readonly long[] _int64A = null!;
        private readonly sbyte[] _sbyteA = null!;
        private readonly float[] _singleA = null!;
        private readonly ushort[] _uint16A = null!;
        private readonly uint[] _uint32A = null!;
        private readonly ulong[] _uint64A = null!;

        internal PrimitiveArray(BitInternalPrimitiveTypeE code, Array array)
        {
            _code = code;
            switch (code)
            {
                case BitInternalPrimitiveTypeE.Boolean: _booleanA = (bool[])array; break;
                case BitInternalPrimitiveTypeE.Char: _charA = (char[])array; break;
                case BitInternalPrimitiveTypeE.Double: _doubleA = (double[])array; break;
                case BitInternalPrimitiveTypeE.Int16: _int16A = (short[])array; break;
                case BitInternalPrimitiveTypeE.Int32: _int32A = (int[])array; break;
                case BitInternalPrimitiveTypeE.Int64: _int64A = (long[])array; break;
                case BitInternalPrimitiveTypeE.SByte: _sbyteA = (sbyte[])array; break;
                case BitInternalPrimitiveTypeE.Single: _singleA = (float[])array; break;
                case BitInternalPrimitiveTypeE.UInt16: _uint16A = (ushort[])array; break;
                case BitInternalPrimitiveTypeE.UInt32: _uint32A = (uint[])array; break;
                case BitInternalPrimitiveTypeE.UInt64: _uint64A = (ulong[])array; break;
            }
        }

        internal void SetValue(string value, int index)
        {
            switch (_code)
            {
                case BitInternalPrimitiveTypeE.Boolean:
                    _booleanA[index] = bool.Parse(value);
                    break;
                case BitInternalPrimitiveTypeE.Char:
                    if ((value[0] == '_') && (value.Equals("_0x00_")))
                    {
                        _charA[index] = char.MinValue;
                    }
                    else
                    {
                        _charA[index] = char.Parse(value);
                    }
                    break;
                case BitInternalPrimitiveTypeE.Double:
                    _doubleA[index] = double.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.Int16:
                    _int16A[index] = short.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.Int32:
                    _int32A[index] = int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.Int64:
                    _int64A[index] = long.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.SByte:
                    _sbyteA[index] = sbyte.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.Single:
                    _singleA[index] = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.UInt16:
                    _uint16A[index] = ushort.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.UInt32:
                    _uint32A[index] = uint.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case BitInternalPrimitiveTypeE.UInt64:
                    _uint64A[index] = ulong.Parse(value, CultureInfo.InvariantCulture);
                    break;
            }
        }
    }
}
