﻿
namespace pwither.formatter
{
    public static class TypeHelper
    {
//        static readonly Type _runtimeType = Type.GetType("System.RuntimeType") ?? throw new Exception("System.RuntimeType not found");
        public static bool IsRuntimeType(Type type)
        {
            // Example of what would not be a runtime type: new TypeDelegator(typeof(int))
            return type.GetType() == typeof(void).GetType();
            //            //https://stackoverflow.com/a/10183678/2671330
        }
    }
}

