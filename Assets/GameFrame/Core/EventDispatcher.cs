using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Reflection;
using System.Collections;
using System.Linq;

interface IEventDispatcher
{
    void AddListener(object type, Delegate listener);
    void RemoveListenner(object type, Delegate listener);
    void Dispatch(object type, params object[] args);
    void Clear();
}

interface IEventDispatcher<T> : IEventDispatcher
{
    void AddListener(T type, Delegate listener);
    void RemoveListener(T type, Delegate listener);
    void Dispatch(T type, params object[] args);
}

public class EventDispatcher : IEventDispatcher
{
    Dictionary<object, HashSet<Delegate>> events = new Dictionary<object, HashSet<Delegate>>();
    public void AddListener(object type, Delegate listener)
    {
        if (events.ContainsKey(type))
        {
            if (!events[type].Contains(listener))
                events[type].Add(listener);
            else
                Debug.LogError(string.Format("type[{0}] Oready have Listener[{1}]!", type, listener));
        }
        else
        {
            events[type] = new HashSet<Delegate>();
            events[type].Add(listener);
        }
    }

    public void Dispatch(object type, params object[] args)
    {
        if (events.ContainsKey(type))
        {
            foreach (var l in events[type])
            {
                try
                {
                    l.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Invoke Error !! type: {0} ,Method: {1}.  \nException->{2}", type, l.Method, e));
                }
            }
        }
    }

    public void RemoveListenner(object type, Delegate listener)
    {
        if (events.ContainsKey(type))
        {
            events[type].Remove(listener);
        }
    }

    public void Clear()
    {
        events.Clear();
    }

}


public class EventDispatcher<T> : EventDispatcher, IEventDispatcher<T>, IEventDispatcher
{
    public void AddListener(T type, Delegate listener)
    {
        base.AddListener(type, listener);
    }

    public void Dispatch(T type, params object[] args)
    {
        base.Dispatch(type, args);
    }

    public void RemoveListener(T type, Delegate listener)
    {
        base.RemoveListenner(type, listener);
    }
}