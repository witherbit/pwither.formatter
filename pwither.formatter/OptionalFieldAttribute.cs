﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace pwither.formatter
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class OptionalFieldAttribute : Attribute
    {
        private int _versionAdded = 1;

        public int VersionAdded
        {
            get => _versionAdded;
            set
            {
                if (value < 1)
                {
                    throw new ArgumentException(SR.Serialization_OptionalFieldVersionValue);
                }
                _versionAdded = value;
            }
        }
    }
}
