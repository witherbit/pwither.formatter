// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace pwither.formatter
{
    internal static class Converter
    {
        internal static readonly Type s_typeofISerializable = typeof(ISerializable);
        internal static readonly Type s_typeofString = typeof(string);
        //internal static readonly Type s_typeofConverter = typeof(Converter);
        internal static readonly Type s_typeofBoolean = typeof(bool);
        internal static readonly Type s_typeofByte = typeof(byte);
        internal static readonly Type s_typeofChar = typeof(char);
        internal static readonly Type s_typeofDecimal = typeof(decimal);
        internal static readonly Type s_typeofDouble = typeof(double);
        internal static readonly Type s_typeofInt16 = typeof(short);
        internal static readonly Type s_typeofInt32 = typeof(int);
        internal static readonly Type s_typeofInt64 = typeof(long);
        internal static readonly Type s_typeofSByte = typeof(sbyte);
        internal static readonly Type s_typeofSingle = typeof(float);
        internal static readonly Type s_typeofTimeSpan = typeof(TimeSpan);
        internal static readonly Type s_typeofDateTime = typeof(DateTime);
        internal static readonly Type s_typeofUInt16 = typeof(ushort);
        internal static readonly Type s_typeofUInt32 = typeof(uint);
        internal static readonly Type s_typeofUInt64 = typeof(ulong);
        internal static readonly Type s_typeofObject = typeof(object);
        internal static readonly Type s_typeofSystemVoid = typeof(void);

        // In .NET Framework the default assembly is mscorlib.dll --> typeof(string).Assembly.
        // In Core type string lives in System.Private.Corelib.dll which doesn't
        // contain all the types which are living in mscorlib in .NET Framework. Therefore we
        // use our mscorlib facade which also contains manual type forwards for deserialization.

        //internal static readonly Assembly s_urtAssembly = Assembly.Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        //internal static readonly string s_urtAssemblyString = s_urtAssembly.FullName!;

        //internal static readonly Assembly s_urtAlternativeAssembly = s_typeofString.Assembly;
        //internal static readonly string s_urtAlternativeAssemblyString = s_urtAlternativeAssembly.FullName!;

        // I don't care about .NET Framework ....
        internal static readonly Assembly s_urt_CoreLib_Assembly = s_typeofString.Assembly;
        internal static readonly string s_urt_CoreLib_AssemblyString = s_urt_CoreLib_Assembly.FullName!;

        internal static readonly Assembly s_urt_mscorlib_Assembly = Assembly.Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
        internal static readonly string s_urt_mscorlib_AssemblyString = s_urt_mscorlib_Assembly.FullName!;


        // Arrays
        internal static readonly Type s_typeofTypeArray = typeof(Type[]);
        internal static readonly Type s_typeofObjectArray = typeof(object[]);
        internal static readonly Type s_typeofStringArray = typeof(string[]);
        internal static readonly Type s_typeofBooleanArray = typeof(bool[]);
        internal static readonly Type s_typeofByteArray = typeof(byte[]);
        internal static readonly Type s_typeofCharArray = typeof(char[]);
        internal static readonly Type s_typeofDecimalArray = typeof(decimal[]);
        internal static readonly Type s_typeofDoubleArray = typeof(double[]);
        internal static readonly Type s_typeofInt16Array = typeof(short[]);
        internal static readonly Type s_typeofInt32Array = typeof(int[]);
        internal static readonly Type s_typeofInt64Array = typeof(long[]);
        internal static readonly Type s_typeofSByteArray = typeof(sbyte[]);
        internal static readonly Type s_typeofSingleArray = typeof(float[]);
        internal static readonly Type s_typeofTimeSpanArray = typeof(TimeSpan[]);
        internal static readonly Type s_typeofDateTimeArray = typeof(DateTime[]);
        internal static readonly Type s_typeofUInt16Array = typeof(ushort[]);
        internal static readonly Type s_typeofUInt32Array = typeof(uint[]);
        internal static readonly Type s_typeofUInt64Array = typeof(ulong[]);

        private const int PrimitiveTypeEnumLength = 17; // Number of PrimitiveTypeEnums

        private static volatile Type?[]? s_typeA;
        private static volatile Type?[]? s_arrayTypeA;
        private static volatile string?[]? s_valueA;
        private static volatile TypeCode[]? s_typeCodeA;
        private static volatile BitInternalPrimitiveTypeE[]? s_codeA;

        internal static BitInternalPrimitiveTypeE ToCode(Type? type) =>
                type == null ? ToPrimitiveTypeEnum(TypeCode.Empty) :
                type.IsPrimitive ? ToPrimitiveTypeEnum(Type.GetTypeCode(type)) :
                ReferenceEquals(type, s_typeofDateTime) ? BitInternalPrimitiveTypeE.DateTime :
                ReferenceEquals(type, s_typeofTimeSpan) ? BitInternalPrimitiveTypeE.TimeSpan :
                ReferenceEquals(type, s_typeofDecimal) ? BitInternalPrimitiveTypeE.Decimal :
                BitInternalPrimitiveTypeE.Invalid;

        internal static bool IsWriteAsByteArray(BitInternalPrimitiveTypeE code)
        {
            switch (code)
            {
                case BitInternalPrimitiveTypeE.Boolean:
                case BitInternalPrimitiveTypeE.Char:
                case BitInternalPrimitiveTypeE.Byte:
                case BitInternalPrimitiveTypeE.Double:
                case BitInternalPrimitiveTypeE.Int16:
                case BitInternalPrimitiveTypeE.Int32:
                case BitInternalPrimitiveTypeE.Int64:
                case BitInternalPrimitiveTypeE.SByte:
                case BitInternalPrimitiveTypeE.Single:
                case BitInternalPrimitiveTypeE.UInt16:
                case BitInternalPrimitiveTypeE.UInt32:
                case BitInternalPrimitiveTypeE.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        internal static int TypeLength(BitInternalPrimitiveTypeE code) =>
            code switch
            {
                BitInternalPrimitiveTypeE.Boolean => 1,
                BitInternalPrimitiveTypeE.Char => 2,
                BitInternalPrimitiveTypeE.Byte => 1,
                BitInternalPrimitiveTypeE.Double => 8,
                BitInternalPrimitiveTypeE.Int16 => 2,
                BitInternalPrimitiveTypeE.Int32 => 4,
                BitInternalPrimitiveTypeE.Int64 => 8,
                BitInternalPrimitiveTypeE.SByte => 1,
                BitInternalPrimitiveTypeE.Single => 4,
                BitInternalPrimitiveTypeE.UInt16 => 2,
                BitInternalPrimitiveTypeE.UInt32 => 4,
                BitInternalPrimitiveTypeE.UInt64 => 8,
                _ => 0,
            };

        internal static Type? ToArrayType(BitInternalPrimitiveTypeE code)
        {
            if (s_arrayTypeA == null)
            {
                InitArrayTypeA();
            }
            return s_arrayTypeA![(int)code];
        }

        private static void InitTypeA()
        {
            var typeATemp = new Type?[PrimitiveTypeEnumLength];
            typeATemp[(int)BitInternalPrimitiveTypeE.Invalid] = null;
            typeATemp[(int)BitInternalPrimitiveTypeE.Boolean] = s_typeofBoolean;
            typeATemp[(int)BitInternalPrimitiveTypeE.Byte] = s_typeofByte;
            typeATemp[(int)BitInternalPrimitiveTypeE.Char] = s_typeofChar;
            typeATemp[(int)BitInternalPrimitiveTypeE.Decimal] = s_typeofDecimal;
            typeATemp[(int)BitInternalPrimitiveTypeE.Double] = s_typeofDouble;
            typeATemp[(int)BitInternalPrimitiveTypeE.Int16] = s_typeofInt16;
            typeATemp[(int)BitInternalPrimitiveTypeE.Int32] = s_typeofInt32;
            typeATemp[(int)BitInternalPrimitiveTypeE.Int64] = s_typeofInt64;
            typeATemp[(int)BitInternalPrimitiveTypeE.SByte] = s_typeofSByte;
            typeATemp[(int)BitInternalPrimitiveTypeE.Single] = s_typeofSingle;
            typeATemp[(int)BitInternalPrimitiveTypeE.TimeSpan] = s_typeofTimeSpan;
            typeATemp[(int)BitInternalPrimitiveTypeE.DateTime] = s_typeofDateTime;
            typeATemp[(int)BitInternalPrimitiveTypeE.UInt16] = s_typeofUInt16;
            typeATemp[(int)BitInternalPrimitiveTypeE.UInt32] = s_typeofUInt32;
            typeATemp[(int)BitInternalPrimitiveTypeE.UInt64] = s_typeofUInt64;
            s_typeA = typeATemp;
        }

        private static void InitArrayTypeA()
        {
            var arrayTypeATemp = new Type?[PrimitiveTypeEnumLength];
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Invalid] = null;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Boolean] = s_typeofBooleanArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Byte] = s_typeofByteArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Char] = s_typeofCharArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Decimal] = s_typeofDecimalArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Double] = s_typeofDoubleArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Int16] = s_typeofInt16Array;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Int32] = s_typeofInt32Array;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Int64] = s_typeofInt64Array;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.SByte] = s_typeofSByteArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.Single] = s_typeofSingleArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.TimeSpan] = s_typeofTimeSpanArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.DateTime] = s_typeofDateTimeArray;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.UInt16] = s_typeofUInt16Array;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.UInt32] = s_typeofUInt32Array;
            arrayTypeATemp[(int)BitInternalPrimitiveTypeE.UInt64] = s_typeofUInt64Array;
            s_arrayTypeA = arrayTypeATemp;
        }

        internal static Type? ToType(BitInternalPrimitiveTypeE code)
        {
            if (s_typeA == null)
            {
                InitTypeA();
            }
            return s_typeA![(int)code];
        }

        internal static Array? CreatePrimitiveArray(BitInternalPrimitiveTypeE code, int length) =>
            code switch
            {
                BitInternalPrimitiveTypeE.Boolean => new bool[length],
                BitInternalPrimitiveTypeE.Byte => new byte[length],
                BitInternalPrimitiveTypeE.Char => new char[length],
                BitInternalPrimitiveTypeE.Decimal => new decimal[length],
                BitInternalPrimitiveTypeE.Double => new double[length],
                BitInternalPrimitiveTypeE.Int16 => new short[length],
                BitInternalPrimitiveTypeE.Int32 => new int[length],
                BitInternalPrimitiveTypeE.Int64 => new long[length],
                BitInternalPrimitiveTypeE.SByte => new sbyte[length],
                BitInternalPrimitiveTypeE.Single => new float[length],
                BitInternalPrimitiveTypeE.TimeSpan => new TimeSpan[length],
                BitInternalPrimitiveTypeE.DateTime => new DateTime[length],
                BitInternalPrimitiveTypeE.UInt16 => new ushort[length],
                BitInternalPrimitiveTypeE.UInt32 => new uint[length],
                BitInternalPrimitiveTypeE.UInt64 => new ulong[length],
                _ => null,
            };

        internal static bool IsPrimitiveArray(Type? type, [NotNullWhen(true)] out object? typeInformation)
        {
            if (ReferenceEquals(type, s_typeofBooleanArray)) typeInformation = BitInternalPrimitiveTypeE.Boolean;
            else if (ReferenceEquals(type, s_typeofByteArray)) typeInformation = BitInternalPrimitiveTypeE.Byte;
            else if (ReferenceEquals(type, s_typeofCharArray)) typeInformation = BitInternalPrimitiveTypeE.Char;
            else if (ReferenceEquals(type, s_typeofDoubleArray)) typeInformation = BitInternalPrimitiveTypeE.Double;
            else if (ReferenceEquals(type, s_typeofInt16Array)) typeInformation = BitInternalPrimitiveTypeE.Int16;
            else if (ReferenceEquals(type, s_typeofInt32Array)) typeInformation = BitInternalPrimitiveTypeE.Int32;
            else if (ReferenceEquals(type, s_typeofInt64Array)) typeInformation = BitInternalPrimitiveTypeE.Int64;
            else if (ReferenceEquals(type, s_typeofSByteArray)) typeInformation = BitInternalPrimitiveTypeE.SByte;
            else if (ReferenceEquals(type, s_typeofSingleArray)) typeInformation = BitInternalPrimitiveTypeE.Single;
            else if (ReferenceEquals(type, s_typeofUInt16Array)) typeInformation = BitInternalPrimitiveTypeE.UInt16;
            else if (ReferenceEquals(type, s_typeofUInt32Array)) typeInformation = BitInternalPrimitiveTypeE.UInt32;
            else if (ReferenceEquals(type, s_typeofUInt64Array)) typeInformation = BitInternalPrimitiveTypeE.UInt64;
            else
            {
                typeInformation = null;
                return false;
            }

            return true;
        }

        private static void InitValueA()
        {
            var valueATemp = new string?[PrimitiveTypeEnumLength];
            valueATemp[(int)BitInternalPrimitiveTypeE.Invalid] = null;
            valueATemp[(int)BitInternalPrimitiveTypeE.Boolean] = "Boolean";
            valueATemp[(int)BitInternalPrimitiveTypeE.Byte] = "Byte";
            valueATemp[(int)BitInternalPrimitiveTypeE.Char] = "Char";
            valueATemp[(int)BitInternalPrimitiveTypeE.Decimal] = "Decimal";
            valueATemp[(int)BitInternalPrimitiveTypeE.Double] = "Double";
            valueATemp[(int)BitInternalPrimitiveTypeE.Int16] = "Int16";
            valueATemp[(int)BitInternalPrimitiveTypeE.Int32] = "Int32";
            valueATemp[(int)BitInternalPrimitiveTypeE.Int64] = "Int64";
            valueATemp[(int)BitInternalPrimitiveTypeE.SByte] = "SByte";
            valueATemp[(int)BitInternalPrimitiveTypeE.Single] = "Single";
            valueATemp[(int)BitInternalPrimitiveTypeE.TimeSpan] = "TimeSpan";
            valueATemp[(int)BitInternalPrimitiveTypeE.DateTime] = "DateTime";
            valueATemp[(int)BitInternalPrimitiveTypeE.UInt16] = "UInt16";
            valueATemp[(int)BitInternalPrimitiveTypeE.UInt32] = "UInt32";
            valueATemp[(int)BitInternalPrimitiveTypeE.UInt64] = "UInt64";
            s_valueA = valueATemp;
        }

        internal static string? ToComType(BitInternalPrimitiveTypeE code)
        {
            if (s_valueA == null)
            {
                InitValueA();
            }
            return s_valueA![(int)code];
        }

        private static void InitTypeCodeA()
        {
            var typeCodeATemp = new TypeCode[PrimitiveTypeEnumLength];
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Invalid] = TypeCode.Object;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Boolean] = TypeCode.Boolean;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Byte] = TypeCode.Byte;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Char] = TypeCode.Char;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Decimal] = TypeCode.Decimal;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Double] = TypeCode.Double;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Int16] = TypeCode.Int16;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Int32] = TypeCode.Int32;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Int64] = TypeCode.Int64;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.SByte] = TypeCode.SByte;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.Single] = TypeCode.Single;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.TimeSpan] = TypeCode.Object;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.DateTime] = TypeCode.DateTime;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.UInt16] = TypeCode.UInt16;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.UInt32] = TypeCode.UInt32;
            typeCodeATemp[(int)BitInternalPrimitiveTypeE.UInt64] = TypeCode.UInt64;
            s_typeCodeA = typeCodeATemp;
        }

        // Returns a System.TypeCode from a InternalPrimitiveTypeE
        internal static TypeCode ToTypeCode(BitInternalPrimitiveTypeE code)
        {
            if (s_typeCodeA == null)
            {
                InitTypeCodeA();
            }
            return s_typeCodeA![(int)code];
        }

        private static void InitCodeA()
        {
            var codeATemp = new BitInternalPrimitiveTypeE[19];
            codeATemp[(int)TypeCode.Empty] = BitInternalPrimitiveTypeE.Invalid;
            codeATemp[(int)TypeCode.Object] = BitInternalPrimitiveTypeE.Invalid;
            codeATemp[(int)TypeCode.DBNull] = BitInternalPrimitiveTypeE.Invalid;
            codeATemp[(int)TypeCode.Boolean] = BitInternalPrimitiveTypeE.Boolean;
            codeATemp[(int)TypeCode.Char] = BitInternalPrimitiveTypeE.Char;
            codeATemp[(int)TypeCode.SByte] = BitInternalPrimitiveTypeE.SByte;
            codeATemp[(int)TypeCode.Byte] = BitInternalPrimitiveTypeE.Byte;
            codeATemp[(int)TypeCode.Int16] = BitInternalPrimitiveTypeE.Int16;
            codeATemp[(int)TypeCode.UInt16] = BitInternalPrimitiveTypeE.UInt16;
            codeATemp[(int)TypeCode.Int32] = BitInternalPrimitiveTypeE.Int32;
            codeATemp[(int)TypeCode.UInt32] = BitInternalPrimitiveTypeE.UInt32;
            codeATemp[(int)TypeCode.Int64] = BitInternalPrimitiveTypeE.Int64;
            codeATemp[(int)TypeCode.UInt64] = BitInternalPrimitiveTypeE.UInt64;
            codeATemp[(int)TypeCode.Single] = BitInternalPrimitiveTypeE.Single;
            codeATemp[(int)TypeCode.Double] = BitInternalPrimitiveTypeE.Double;
            codeATemp[(int)TypeCode.Decimal] = BitInternalPrimitiveTypeE.Decimal;
            codeATemp[(int)TypeCode.DateTime] = BitInternalPrimitiveTypeE.DateTime;
            codeATemp[17] = BitInternalPrimitiveTypeE.Invalid;
            codeATemp[(int)TypeCode.String] = BitInternalPrimitiveTypeE.Invalid;
            s_codeA = codeATemp;
        }

        // Returns a InternalPrimitiveTypeE from a System.TypeCode
        internal static BitInternalPrimitiveTypeE ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            if (s_codeA == null)
            {
                InitCodeA();
            }
            return s_codeA![(int)typeCode];
        }

        // Translates a string into an Object
        internal static object? FromString(string? value, BitInternalPrimitiveTypeE code)
        {
            // InternalPrimitiveTypeE needs to be a primitive type
            Debug.Assert((code != BitInternalPrimitiveTypeE.Invalid), "[Converter.FromString]!InternalPrimitiveTypeE.Invalid ");
            return code != BitInternalPrimitiveTypeE.Invalid ?
                Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture) :
                value;
        }
    }
}
