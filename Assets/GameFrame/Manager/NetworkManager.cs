using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public enum NetworkEvent
{
    None,
    Conneted,
    ConnetFailed,
    Disconneted,
    ConnetError,
}

public class NetworkManager : Manager<NetworkManager>
{

    public const int maxBuffer = 10240;

    Dictionary<string, NetworkClient> clients = new Dictionary<string, NetworkClient>();

    Dictionary<int, Action<NetworkMessage>> messagHadnles = new Dictionary<int, Action<NetworkMessage>>();


    public void Connect(string ip, int port, string serverType, System.Action onConnect)
    {
        NetworkClient client;
        if (clients.ContainsKey(serverType))
        {
            client = clients[serverType];
        }
        else
        {
            client = new NetworkClient(ip, port, maxBuffer);
            clients[serverType] = client;
        }
        client.Connect(onConnect);
    }


    void OnReceive(byte[] data)
    {

    }


    public void RegisterHandle(int protoID, Action<NetworkMessage> handle)
    {

    }





}