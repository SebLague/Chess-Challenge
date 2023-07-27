using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChessChallenge.API;
using ChessChallenge.Example;
using static ChessChallenge.Application.ConsoleHelper;

namespace ChessChallenge.Application.NetworkHelpers;

public static class ServerConnectionHelper
{
    public static bool StartsOffWhite { get; private set; }
    public static TcpClient TcpClient { get; private set; }

    public static async Task ConnectToServerAsync(string host, int port, string roomId, string protocolVersion)
    {
        if(TcpClient is not null && TcpClient.Connected)
            return;

        if (TcpClient is not null)
        {
            Log("Trying to dispose old inactive TCP Client object!", isError: false);
            try
            {
                TcpClient.Dispose();
                Log("Disposed TCP Client object!", isError: false);
            }
            catch
            {
                Log("Failed to dispose off old TCP Client!", isError: true, ConsoleColor.Red);
            }
        }

        Log($"Connecting to {host}:{port}");
        TcpClient = new TcpClient();
        try
        {
            await TcpClient.ConnectAsync(host, port);
        }
        catch (Exception e)
        {
            Log($"Failed to connect to {host} on port {port} due to {e.ToString()}");
            return;
        }
        
        Log("Connection established!");

        try
        {
            Log("Waiting for Server's Hello");
            var serverHelloMsg = ReadMessage<ServerHelloMsg>();
            
            var isCompatible = VerifyServer(serverHelloMsg, protocolVersion);
            if (!isCompatible)
            {
                Log("Incompatible server version! Disconnecting...");
                TcpClient.Dispose();
            }
            SendMessage(new ClientHelloMsg
            {
                RoomId = roomId,
                ProtocolVersion = protocolVersion,
                ClientVersion = "0.1"
            });

            var roomInfo = ReadMessage<RoomInfo>();
            Log($"Joined {roomId}");
            StartsOffWhite = roomInfo.StartsOffAsWhite;
        }
        catch (Exception e)
        {
            Log("Error occured while initialising client-server connection! Room probably full!", isError: true, ConsoleColor.Red);
            Log(e.ToString());
            TcpClient.Dispose();
        }
    }
    

    private static bool VerifyServer(ServerHelloMsg serverHelloMsg, string protocolVersion) 
        => serverHelloMsg.ProtocolVersion == protocolVersion;
    
    public static void SendMessage(ISerializableMessage message) => message.SerializeIntoStream(TcpClient.GetStream());

    public static T ReadMessage<T>() where T : ISerializableMessage, new()
    {
        var ret = new T();
            
        ret.ReadFromStream(TcpClient.GetStream());
        return ret;
    }
}