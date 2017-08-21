using UnityEngine;
using System.Collections;
using System.IO;

public class NetworkMessage
{
    byte[] body;
    public ushort protoID;
    MemoryStream stream;
    public NetworkMessage(ushort protoID, byte[] data)
    {
        stream = new MemoryStream(data);
        this.protoID = protoID;
    }
   

}
