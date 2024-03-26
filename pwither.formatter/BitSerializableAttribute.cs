using System.ComponentModel;
using System.Reflection;

namespace pwither.formatter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class BitSerializableAttribute : Attribute
    {
        public BitSerializableAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class BitNonSerializedAttribute : Attribute
    {
        public BitNonSerializedAttribute()
        {
        }
    }
}
