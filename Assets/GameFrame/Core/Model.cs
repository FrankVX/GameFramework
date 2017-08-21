using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

interface IModel
{
}

public class Protocol : Attribute
{
    public readonly int ProtocolID;
    public Protocol(int protocolID)
    {
        ProtocolID = protocolID;
    }
}
public abstract class Model : BaseBehaviour, IModel
{
    Dictionary<int, MethodInfo> _methods = new Dictionary<int, MethodInfo>();
    protected virtual void Awake()
    {
        var aType = typeof(Protocol);
        var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var m in methods)
        {
            if (m.IsDefined(aType, true))
            {
                var ps = m.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(NetworkMessage))
                {
                    var s = (from a in m.GetCustomAttributes(true) where a.GetType().Equals(aType) select a).ToArray();
                    var a_obj = s[0] as Protocol;
                    _methods[a_obj.ProtocolID] = m;
                }
            }
        }
        foreach (var m in _methods)
        {
            RegisterListener(m.Key, new Action<NetworkMessage>(MessagHandle));
        }
    }
    void MessagHandle(NetworkMessage msg)
    {
        if (_methods.ContainsKey(msg.protoID))
        {
            _methods[msg.protoID].Invoke(this, new object[] { msg });
        }
    }
}
