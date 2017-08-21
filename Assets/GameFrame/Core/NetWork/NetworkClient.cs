using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkClient : IDisposable
{
    /// <summary>客户端连接标示</summary>
    private TcpClient client = null;

    private MemoryStream stream;
    private BinaryReader reader;
    /// <summary>
    /// 缓存
    /// </summary>
    private byte[] byteBuffer = null;
    /// <summary>链接地址</summary>
    private string _ip = string.Empty;
    /// <summary>链接地址</summary>
    public string IP { get { return this._ip; } set { this._ip = value; this.Dispose(); } }

    /// <summary>链接端口</summary>
    private int _port = 0;
    /// <summary>链接端口</summary>
    public int Port { get { return this._port; } set { this._port = value; this.Dispose(); } }
    /// <summary>剩余字节</summary>
    public long RemainingBytes { get { return stream.Length - stream.Position; } }

    public Action<byte[]> MessageHandle;

    public Action<NetworkEvent, object> networkHandle;

    public bool Connected
    {
        get
        {
            return (client != null && client.Connected);
        }
    }
    private object lockObj = new object();
    public NetworkClient(string ip, int port, int maxBuffer)
    {
        this._ip = ip;
        this._port = port;
        byteBuffer = new byte[maxBuffer];
    }
    Action _onConnect;
    Action _onDisConnect;
    public bool Connect(Action onConnect = null, Action onDisConnect = null)
    {
        try
        {
            if (client != null && client.Connected) return true;
            if (client == null)
            {
                client = new TcpClient();
                client.SendTimeout = 1000;
                client.ReceiveTimeout = 1000;
                client.NoDelay = true;
            }
            _onConnect = onConnect;
            client.BeginConnect(IP, Port, OnConnect, null);
            Debug.Log(string.Format("Client BeginConnect, IP:{0}  Port:{1} ", IP, Port));
        }
        catch (Exception e)
        {
            Dispose();
            Debug.LogError(e.Message);
            return false;
        }
        return true;
    }



    /// <summary>回调</summary>
    /// <param name="asyncresult"></param>
    private void OnConnect(IAsyncResult asyncresult)
    {
        try
        {
            lock (lockObj)
            {
                Debug.Log(asyncresult.IsCompleted);
                if (client.Client != null && client.Connected)
                {
                    client.EndConnect(asyncresult);
                    stream = new MemoryStream();
                    reader = new BinaryReader(stream);
                    client.GetStream().BeginRead(byteBuffer, 0, byteBuffer.Length, OnRead, null);
                    if (_onConnect != null) _onConnect();
                }
                else
                {
                    Dispose();
                    Debug.LogError("Clinet Connect failed!!");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Dispose();
        }
    }

    void OnRead(IAsyncResult asr)
    {
        int bytesRead = 0;
        try
        {
            lock (lockObj)
            {         //读取字节流到缓冲区
                bytesRead = client.GetStream().EndRead(asr);

                if (bytesRead < 1)
                {
                    //服务器主动断开链接
                    Debug.LogError("DisConnect!!!");
                    Dispose();
                    return;
                }
                OnReceive(bytesRead);   //分析数据包内容，抛给逻辑层

                //分析完，再次监听服务器发过来的新消息
                client.GetStream().BeginRead(byteBuffer, 0, byteBuffer.Length, new AsyncCallback(OnRead), null);
            }
        }
        catch (Exception e)
        {
            //数据流异常，断开连接
            Debug.LogError(e);
            Dispose();
        }
    }


    void OnReceive(int length)
    {
        try
        {
            stream.Seek(0, SeekOrigin.End);
            stream.Write(byteBuffer, 0, length);
            stream.Seek(0, SeekOrigin.Begin);
            while (RemainingBytes >= 2)
            {
                int bodyLength = reader.ReadUInt16();
                if (RemainingBytes >= bodyLength)
                {
                    var body = reader.ReadBytes(bodyLength);
                    OnReceiveMessage(body);
                }
                else
                {
                    stream.Position -= 2;
                    break;
                }
            }
            var bytes = reader.ReadBytes((int)RemainingBytes);
            stream.SetLength(0);
            stream.Write(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Send(byte[] data)
    {
        if (data.Length > 0)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryWriter writer = new BinaryWriter(ms);
                        ushort msglen = (ushort)(data.Length);
                        writer.Write(msglen);
                        writer.Write(data);
                        writer.Flush();
                        client.GetStream().BeginWrite(ms.ToArray(), 0, (int)ms.Length, OnWrite, null);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    private void OnWrite(IAsyncResult r)
    {
        try
        {
            lock (lockObj)
            {
                client.GetStream().EndWrite(r);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }



    void OnReceiveMessage(byte[] bytes)
    {
        if (MessageHandle != null)
        {
            MessageHandle(bytes);
        }
    }


    public void Dispose()
    {
        lock (lockObj)
        {
            if (client != null)
            {
                client.Close();
            }
            if (reader != null)
            {
                reader.Close();
                reader = null;
            }
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
            }
            if (_onDisConnect != null) _onDisConnect();
        }
    }
}
