using System.IO;
using System.Text;

namespace ChessChallenge.Application.NetworkHelpers;

public struct ServerHelloMsg : ISerializableMessage
{
    public string ProtocolVersion;
    public string ServerVersion;
    public string SessionId;
    
    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(ProtocolVersion);
        writer.Write(ServerVersion);
        writer.Write(SessionId);
    }
    
    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        
        ProtocolVersion = reader.ReadString();
        ServerVersion = reader.ReadString();
        SessionId = reader.ReadString();
    }
}

public struct ClientHelloMsg : ISerializableMessage
{
    public string ProtocolVersion;
    public string ClientVersion; 
            
    // RoomId: No weird characters (maybe)
    public string RoomId; // Create or connect to this room  
    
    
    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(ProtocolVersion);
        writer.Write(ClientVersion);
        writer.Write(RoomId);
    }
    
    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        
        ProtocolVersion = reader.ReadString();
        ClientVersion = reader.ReadString();
        RoomId = reader.ReadString();
    }
    
}

public struct RoomInfo : ISerializableMessage
{
    public string RoomId;
    public bool StartsOffAsWhite; // True if this client will be on the white side for the first match
    
    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(RoomId);
        writer.Write(StartsOffAsWhite);
    }
    
    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        
        RoomId = reader.ReadString();
        StartsOffAsWhite = reader.ReadBoolean();

    }
}

public struct GameSettings : ISerializableMessage
{
    public int TimeForEachPlayer; // in seconds
    public bool IsWhite;
    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(TimeForEachPlayer);
        writer.Write(IsWhite);
    }

    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        
        TimeForEachPlayer = reader.ReadInt32();
        IsWhite = reader.ReadBoolean();
    }
}
public struct MoveMessage : ISerializableMessage
{
    public bool LastMove; // True if last move in a game
    public string MoveName; // If move name is null move then its probably timeout
    public long Clock; // Timestamp (set only by server)
    //TODO: Implement clock sync (server authoritative)
    
    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(LastMove);
        writer.Write(MoveName);
        writer.Write(Clock);
    }

    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        
        LastMove = reader.ReadBoolean();
        MoveName = reader.ReadString();
        Clock = reader.ReadInt64();
    }
}

public struct GameStart : ISerializableMessage
{
    public long Timestamp; // Use this timestamp to calculate client move timings


    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(Timestamp);
    }

    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        Timestamp = reader.ReadInt64();
    }
}

public struct IsReady : ISerializableMessage
{
    public bool isReady;
    public void SerializeIntoStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(isReady);
    }

    public void ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8,  true);
        isReady = reader.ReadBoolean();
    }
}