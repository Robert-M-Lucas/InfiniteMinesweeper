using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectCopier
{
    public static T Clone<T>(T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", nameof(source));
        }

        // Don't serialize a null object, simply return the default for that object
        if (ReferenceEquals(source, null)) return default;

        using Stream stream = new MemoryStream();
        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, source);
        stream.Seek(0, SeekOrigin.Begin);
        return (T)formatter.Deserialize(stream);
    }
}