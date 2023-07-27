using System.IO;

namespace ChessChallenge.Application.NetworkHelpers;

public interface ISerializableMessage
{
    public byte[] SerializeAsArray()
    {
        using var stream = new MemoryStream();
        SerializeIntoStream(stream);
        return stream.ToArray();
    }
    
    public void SerializeIntoStream(Stream stream);

    public void DeserializeFromArray(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        
        ReadFromStream(stream);
    }
    
    public void ReadFromStream(Stream stream);
}