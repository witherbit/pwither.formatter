using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace pwither.formatter
{
    public static class BitSerializer
    {
        public static byte[] Serialize(object obj)
        {
            var formatter = new BitBinaryFormatter();
            formatter = formatter.Initialize(GetTypes(obj.GetType()));
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static T Deserialize<T>(byte[] arr)
        {
            var formatter = new BitBinaryFormatter();
            formatter = formatter.Initialize(GetTypes(typeof(T)));
            using (var ms = new MemoryStream())
            {
                ms.Write(arr, 0, arr.Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }
        public static byte[] SerializeNative(object obj, params Type[] nativeTypeBindings)
        {
            var formatter = new BitBinaryFormatter();
            formatter = formatter.Initialize(nativeTypeBindings);
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static T DeserializeNative<T>(byte[] arr, params Type[] nativeTypeBindings)
        {
            var formatter = new BitBinaryFormatter();
            formatter = formatter.Initialize(nativeTypeBindings);
            using (var ms = new MemoryStream())
            {
                ms.Write(arr, 0, arr.Length);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(ms);
            }
        }


        private static BitBinaryFormatter Initialize(this BitBinaryFormatter formatter, params Type[] typeBindings)
        {
            formatter.SurrogateSelector = new ConverterSelector();
            formatter.Control.IsSerializableHandlers = new IsSerializableHandlers();
            formatter.Control.IsSerializableHandlers.Handlers.OfType<SerializeAllowedTypes>().Single().AllowedTypes.Add(typeof(object));
            var binder = new AllowedTypesBinder();
            foreach (var typeBinding in typeBindings)
            {
                binder.AddAllowedType(typeBinding);
            }
            formatter.Binder = binder;
            return formatter;
        }
        private static Type[] GetTypes(Type obj, params Type[] customTypeBindings)
        {
            List<Type> types = new List<Type>();
            types.Add(obj);
            var props = obj.GetProperties();
            foreach (var prop in props)
            {
                var attr = prop.PropertyType.GetCustomAttribute(typeof(BitSerializableAttribute), false);
                if (!prop.PropertyType.IsPrimitive && prop.PropertyType.Module.ScopeName != "CommonLanguageRuntimeLibrary" && attr != null && !types.Contains(prop.PropertyType))
                {
                    types.Add(prop.PropertyType);
                }
            }
            var fields = obj.GetFields();
            foreach (var field in fields)
            {
                var attr = field.FieldType.GetCustomAttribute(typeof(BitSerializableAttribute), false);
                if (!field.FieldType.IsPrimitive && field.FieldType.Module.ScopeName != "CommonLanguageRuntimeLibrary" && attr != null && !types.Contains(field.FieldType))
                {
                    types.Add(field.FieldType);
                }
            }
            if (customTypeBindings != null && customTypeBindings.Length > 0)
            foreach (var type in customTypeBindings)
            {
                if (!type.IsPrimitive && !types.Contains(type))
                    types.Add(type);
            }
            return types.ToArray();
        }
    }
}
