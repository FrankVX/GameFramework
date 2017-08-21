using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public abstract class SignalBase
{
    protected Dictionary<Type, MethodInfo> events = new Dictionary<Type, MethodInfo>();

    protected HashSet<Delegate> handlers = new HashSet<Delegate>();

    protected void AddListener(Delegate handler)
    {
        if (handler != null)
            handlers.Add(handler);
    }

    protected void RemoveListener(Delegate handler)
    {
        if (handler == null) return;
        if (handlers.Contains(handler))
            handlers.Remove(handler);
    }

    public void Clear()
    {
        handlers.Clear();
    }

    protected void Dispatch(params object[] args)
    {
        foreach (var h in handlers)
            Invoke(h, args);
    }

    protected void Invoke(Delegate handle, params object[] args)
    {
        try
        {
            if (handle != null)
                handle.DynamicInvoke(args);
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Invoke Error !! type: {0} ,Method: {1}.  \nException->{2}", this, handle.Method, e));
        }
    }

}
public class Signal : SignalBase
{
    public void AddListener(Action handler)
    {
        base.AddListener(handler);
    }

    public void RemoveListener(Action handler)
    {
        base.RemoveListener(handler);
    }

    public void Dispatch()
    {
        base.Dispatch(null);
    }
}

public class Signal<T> : SignalBase
{

    public void AddListener(Action<T> handler)
    {
        base.AddListener(handler);
    }

    public void RemoveListener(Action<T> handler)
    {
        base.RemoveListener(handler);
    }

    public void Dispatch(T arg)
    {
        base.Dispatch(arg);
    }
}

public class Signal<T, T2> : SignalBase
{

    public void AddListener(Action<T, T2> handler)
    {
        base.AddListener(handler);
    }
    public void RemoveListener(Action<T, T2> handler)
    {
        base.RemoveListener(handler);
    }
    public void Dispatch(T arg, T2 arg2)
    {
        base.Dispatch(arg, arg2);
    }
}

public class Signal<T, T2, T3> : SignalBase
{
    public void AddListener(Action<T, T2, T3> handler)
    {
        base.AddListener(handler);
    }
    public void RemoveListener(Action<T, T2, T3> handler)
    {
        base.RemoveListener(handler);
    }
    public void Dispatch(T arg, T2 arg2, T3 arg3)
    {
        base.Dispatch(arg, arg2, arg3);
    }
}

public class Signal<T, T2, T3, T4> : SignalBase
{
    public void AddListener(Action<T, T2, T3, T4> handler)
    {
        base.AddListener(handler);
    }
    public void RemoveListener(Action<T, T2, T3, T4> handler)
    {
        base.RemoveListener(handler);
    }
    public void Dispatch(T arg, T2 arg2, T3 arg3, T4 arg4)
    {
        base.Dispatch(arg, arg2, arg3, arg4);
    }
}
