using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NetworkReader
{
    MemoryStream stream;
    public NetworkReader(MemoryStream stream)
    {
        this.stream = stream;
    }

    public NetworkReader(byte[] buffer)
    {
        stream = new MemoryStream(buffer);
    }

    public void read()
    {
        
    }

    public void Read<T>()
    {

    }

}
