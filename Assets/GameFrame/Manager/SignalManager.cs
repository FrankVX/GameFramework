using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalManager : Manager<SignalManager>
{

    Dictionary<Type, SignalBase> signals = new Dictionary<Type, SignalBase>();


    SignalBase GetSignal(Type type)
    {
        if (signals.ContainsKey(type))
        {
            return signals[type];
        }
        else
        {
            SignalBase o = Activator.CreateInstance(type) as SignalBase;
            signals[type] = o;
            return o;
        }
    }

    public T GetSignal<T>() where T : SignalBase
    {
        return GetSignal(typeof(T)) as T;
    }

    public void ClearAll()
    {
        signals.Clear();
    }
}
