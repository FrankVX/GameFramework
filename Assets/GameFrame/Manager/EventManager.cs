using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Manager<EventManager>
{

    public class EventData
    {
        public Type type;
        public string identity;
        public object key;
        public object[] args;
        public float timer = 0;
    }

    public int MaxEventCountPreFrame = 10;

    Dictionary<Type, Dictionary<string, IEventDispatcher>> dispatchers = new Dictionary<Type, Dictionary<string, IEventDispatcher>>();
    /// <summary>
    /// 异步消息列队 支持多线程
    /// </summary>
    Queue<EventData> asyncEventQueue = new Queue<EventData>();
    /// <summary>
    /// 延迟消息列队 支持多线程
    /// </summary>
    List<EventData> delayEventQueue = new List<EventData>();


    private void Update()
    {
        lock (asyncEventQueue)
        {
            int maxNum = MaxEventCountPreFrame;
            while (asyncEventQueue.Count > 0 && maxNum-- > 0)
            {
                var item = asyncEventQueue.Dequeue();
                GetDispatcher(item.type, item.identity).Dispatch(item.key, item.args);
            }
        }
        lock (delayEventQueue)
        {
            int maxNum = MaxEventCountPreFrame;
            for (int i = delayEventQueue.Count - 1; i >= 0; i--)
            {
                var item = delayEventQueue[i];
                if (item.timer <= Time.realtimeSinceStartup)
                {
                    GetDispatcher(item.type, item.identity).Dispatch(item.key, item.args);
                    delayEventQueue.RemoveAt(i);
                    if (--maxNum <= 0) break;
                }
            }
        }
    }


    EventDispatcher<T> GetDispatcher<T>(string identity)
    {
        var type = typeof(T);
        return GetDispatcher(type, identity) as EventDispatcher<T>;
    }

    IEventDispatcher GetDispatcher(Type type, string identity)
    {
        if (!dispatchers.ContainsKey(type))
        {
            dispatchers[type] = new Dictionary<string, IEventDispatcher>();
        }
        if (!dispatchers[type].ContainsKey(identity))
        {
            var dtype = typeof(EventDispatcher<>);
            dtype = dtype.MakeGenericType(type);
            IEventDispatcher o = Activator.CreateInstance(dtype) as IEventDispatcher;
            dispatchers[type][identity] = o;
        }
        return dispatchers[type][identity];
    }
    public void AddListener<T>(T type, Delegate listener)
    {
        AddListener(type, string.Empty, listener);
    }
    public void AddListener<T>(T type, string identity, Delegate listener)
    {
        GetDispatcher<T>(identity).AddListener(type, listener);
    }
    public void RemoveListener<T>(T type, Delegate listener)
    {
        RemoveListener(type, string.Empty, listener);
    }
    public void RemoveListener<T>(T type, string identity, Delegate listener)
    {
        GetDispatcher<T>(identity).RemoveListenner(type, listener);
    }

    /// <summary>
    /// 同步派发消息 不支持多线程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="args"></param>
    public void Dispatch<T>(T type, params object[] args)
    {
        GetDispatcher<T>(string.Empty).Dispatch(type, args);
    }
    /// <summary>
    /// 同步派发消息 不支持多线程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="args"></param>
    public void Dispatch<T>(T type, string identity, params object[] args)
    {
        GetDispatcher<T>(identity).Dispatch(type, args);
    }
    /// <summary>
    /// 延迟派发消息 支持多线程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="delay"></param>
    /// <param name="args"></param>
    public void DispatchDelay<T>(T type, float delay, params object[] args)
    {
        DispatchDelay(type, string.Empty, delay, args);
    }
    /// <summary>
    /// 延迟派发消息 支持多线程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="delay"></param>
    /// <param name="args"></param>
    public void DispatchDelay<T>(T type, string identity, float delay, params object[] args)
    {
        if (delay <= 0)
        {
            DispatchAsync(type, args);
            return;
        }
        lock (delayEventQueue)
        {
            delayEventQueue.Add(new EventData()
            {
                type = typeof(T),
                key = type,
                args = args,
                timer = Time.realtimeSinceStartup + delay,
                identity = identity,
            });
        }
    }
    /// <summary>
    /// 异步派发消息 支持多线程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="args"></param>
    public void DispatchAsync<T>(T type, params object[] args)
    {
        DispatchAsync(type, string.Empty, args);
    }
    /// <summary>
    /// 异步派发消息 支持多线程
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="args"></param>
    public void DispatchAsync<T>(T type, string identity, params object[] args)
    {
        lock (asyncEventQueue)
        {
            asyncEventQueue.Enqueue(new EventData()
            {
                type = typeof(T),
                key = type,
                args = args,
                identity= identity,
            });
        }
    }

    public void Clear<T>(string identity)
    {
        GetDispatcher<T>(identity).Clear();
    }

    public void ClearAll()
    {
        dispatchers.Clear();
    }

}
