# pwither.formatter
#### Замена BinaryFormatter для .net 8.0 и более поздних версий.

BinaryFormatter, возможно, будет удален в .NET 9. Этот код скопирован из .NET 8.0, поэтому он полностью независим.

Он также не использует FieldAttributes.NotSerialized или Type.IsSerializable (TypeAttributes.Serializable).

Поскольку он независим, это также означает, что он не может ничего делать "из коробки", поскольку никакие типы не приписываются [BitSerializable] или не реализуют BitBinaryFormatter.ISerializable, даже примитивные типы, такие как int. 

Для начала измените ваш код следующим образом:
```csharp
from [Serializable] -> [BitSerializable]
from [NonSerialized] -> [BitNonSerialized]
from ISerializable -> pwither.formatter.ISerializable
и т.д.
```
Или можете использовать BitBinaryFormatter.Control для настройки этого поведения:
```csharp
BitBinaryFormatter.Control.IsSerializableHandlers
BitBinaryFormatter.Control.IsNotSerializedHandlers
```
Теперь вы должны иметь возможность сериализовать свои собственные типы, если они вообще ничего не содержат.
А как насчет типов во время выполнения? Да, это WIP, но некоторые типы были добавлены, и вы можете включить их следующим образом:
```csharp
var bf = new BinaryFormatter();
bf.SurrogateSelector = new ConverterSelector(); // добавить конвертеры по умолчанию, в настоящее время это Dictionary<,>, HashSet<>.
bf.Control.IsSerializableHandlers = new IsSerializableHandlers(); // добавляет обработчики IsSerializable по умолчанию, в настоящее время примитивные типы, List<>, Stack<>, DateTime, KeyValuePair<,>, и т.д.
```
Пример использования из еще одной моей библиотеки.
Представим, что мы имеем следующие типы, которые надо сериализовать через BinaryFormatter:
```csharp
[BitSerializable]
public class Packager
{
    public List<Package> Packages {  get; set; }
    public PackagerInfo PackageInfo { get; set; }
    public Packager(PackagerInfo info) : base()
    {
        PackageInfo = info;
        Packages = new List<Package>();
    }
}

[BitSerializable]
public class PackagerInfo
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Additional { get; set; }
    public string AppName { get; set; }
}

[BitSerializable]
public class Package
{
    public int ChunkSize { get; set; }
    public FilePart[] Parts {  get; set; }
    public string Name { get; set; }
    public string DestinationDirectory { get; set; }
    public string Additional {  get; set; }
}

[BitSerializable]
public class FilePart
{
    public byte[] Part { get; set; }
    public long Size {  get; set; }
    public int Index { get; set; }
}
```
Для сериализации напишем статический класс, в который поместим следующий код:
```csharp
public static byte[] PackagerToByteArray(this Packager obj)
{
    using (var ms = new MemoryStream())
    {
        var bf = new BitBinaryFormatter();
        bf.SurrogateSelector = new ConverterSelector();
        bf.Control.IsSerializableHandlers = new IsSerializableHandlers();
        bf.Control.IsSerializableHandlers.Handlers.OfType<SerializeAllowedTypes>().Single().AllowedTypes.Add(typeof(object));
        var b = new AllowedTypesBinder();
        b.AddAllowedType(typeof(Packager));
        b.AddAllowedType(typeof(PackagerInfo));
        b.AddAllowedType(typeof(Package));
        b.AddAllowedType(typeof(FilePart));
        bf.Binder = b;
        bf.Serialize(ms, obj);
        return ms.ToArray();
    }
}

public static Packager ByteArrayToPackager(this byte[] arrBytes)
{
    using (var ms = new MemoryStream())
    {
        ms.Write(arrBytes, 0, arrBytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        var bf = new BitBinaryFormatter();
        bf.SurrogateSelector = new ConverterSelector();
        bf.Control.IsSerializableHandlers = new IsSerializableHandlers();
        bf.Control.IsSerializableHandlers.Handlers.OfType<SerializeAllowedTypes>().Single().AllowedTypes.Add(typeof(object));
        var b = new AllowedTypesBinder();
        b.AddAllowedType(typeof(Packager));
        b.AddAllowedType(typeof(PackagerInfo));
        b.AddAllowedType(typeof(Package));
        b.AddAllowedType(typeof(FilePart));
        bf.Binder = b;
        return (Packager)bf.Deserialize(ms);
    }
}
```
Вы должны понимать, что это довольно сырая имплементация BinaryFormatter, и в большинстве случаев я рекомендую использовать вам Json или Xml, так как оригинальный BinaryFormatter больше не поддерживается из-за того, что это небезопасно, грубо говоря злоумышленник с помощью бинарной десериализации может вызвать отказ в обслуживании, выполнить произвольный код или раскрыть конфиденциальную информацию, с учетом того, что информационная безопасность становится неотъемлемой частью жизни, эти уязвимости для многих юридических и физических лиц являются критическими, более подробно о причинах отказа от BinaryFormatter написано в [Руководстве по безопасности BinaryFormatter](https://learn.microsoft.com/ru-ru/dotnet/standard/serialization/binaryformatter-security-guide "странице Microsoft")
