﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace pwither.formatter
{
    public sealed partial class BitBinaryFormatter : IFormatter
    {
        private static readonly ConcurrentDictionary<Type, TypeInformation> s_typeNameCache = new ConcurrentDictionary<Type, TypeInformation>();

        internal ISurrogateSelector? _surrogates;
        internal StreamingContext _context;
        internal SerializationBinder? _binder;
        internal SerializationControl _control = SerializationControl.Default;
        internal BitFormatterTypeStyle _typeFormat = BitFormatterTypeStyle.TypesAlways; // For version resiliency, always put out types
        internal BitFormatterAssemblyStyle _assemblyFormat = BitFormatterAssemblyStyle.Simple;
        internal BitTypeFilterLevel _securityLevel = BitTypeFilterLevel.Full;
        internal object[]? _crossAppDomainArray;

        public BitFormatterTypeStyle TypeFormat { get { return _typeFormat; } set { _typeFormat = value; } }
        public BitFormatterAssemblyStyle AssemblyFormat { get { return _assemblyFormat; } set { _assemblyFormat = value; } }
        public BitTypeFilterLevel FilterLevel { get { return _securityLevel; } set { _securityLevel = value; } }
        public ISurrogateSelector? SurrogateSelector { get { return _surrogates; } set { _surrogates = value; } }
        public SerializationBinder? Binder { get { return _binder; } set { _binder = value; } }
        public StreamingContext Context { get { return _context; } set { _context = value; } }
        public SerializationControl Control { get { return _control; } set { _control = value; } }

        public BitBinaryFormatter() : this(null, new StreamingContext(StreamingContextStates.All))
        {
        }

        public BitBinaryFormatter(ISurrogateSelector? selector, StreamingContext context)
        {
            _surrogates = selector;
            _context = context;
        }

        internal static TypeInformation GetTypeInformation(Type type) =>
            s_typeNameCache.GetOrAdd(type, t =>
            {
                string assemblyName = BitFormatterServices.GetClrAssemblyName(t, out bool hasTypeForwardedFrom);
                return new TypeInformation(BitFormatterServices.GetClrTypeFullName(t), assemblyName, hasTypeForwardedFrom);
            });
    }
}
