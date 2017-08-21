using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BaseBehaviour : MonoBehaviour
{
    protected virtual void OnDestroy()
    {
        RevokeAllListener();
    }

    #region Event
    Dictionary<object, Dictionary<string, Delegate>> listeners = new Dictionary<object, Dictionary<string, Delegate>>();

    public void RegisterListener<T>(T type, Delegate listener, string identity = "")
    {
        //if (!listeners.ContainsKey(type))
        //listeners[type] = listener;
        //EventManager.Instance.AddListener(type, identity, listener);
    }

    public void RegisterListener<T>(T type, Action listener, string identity = "")
    {
        //if (listeners.ContainsKey(type)) return;
        //listeners[type] = listener;
        //EventManager.Instance.AddListener(type, identity, listener);
    }

    public void RevokeListener<T>(T type, Delegate listener, string identity = "")
    {
        if (!listeners.ContainsKey(type)) return;
        listeners.Remove(type);
        EventManager.Instance.RemoveListener(type, identity, listener);
    }
    public void DispatchEvent<T>(T type, string identity, params object[] args)
    {
        EventManager.Instance.Dispatch(type, identity, args);
    }
    public void DispatchEvent<T>(T type, params object[] args)
    {
        EventManager.Instance.Dispatch(type, args);
    }

    public void DispatchEventAsync<T>(T type, string identity, params object[] args)
    {
        EventManager.Instance.DispatchAsync(type, identity, args);
    }
    public void DispatchEventAsync<T>(T type, params object[] args)
    {
        EventManager.Instance.DispatchAsync<T>(type, args);
    }

    public void DispatchEventDelay<T>(T type, string identity, float delay, params object[] args)
    {
        EventManager.Instance.DispatchDelay<T>(type, identity, delay, args);
    }
    public void DispatchEventDelay<T>(T type, float delay, params object[] args)
    {
        EventManager.Instance.DispatchDelay<T>(type, delay, args);
    }

    public void RevokeAllListener()
    {
        foreach (var l in listeners)
        {
            //EventManager.Instance.RemoveListener(l.Key, l.Value);
        }
        listeners.Clear();
    }

    public T GetSignal<T>() where T : SignalBase
    {
        return SignalManager.Instance.GetSignal<T>();
    }
    #endregion

    #region AssetLoad
    public void LoadAsset<T>(string path, Action<T> callBack) where T : UnityEngine.Object
    {
        AssetManager.Instance.LoadAsset<T>(path, callBack);
    }

    public T GetConfig<T>(int id) where T : BaseConfig
    {
        return ConfigManager.Instance.GetConfig<T>(id);
    }

    public List<T> GetConfigs<T>() where T : BaseConfig
    {
        return ConfigManager.Instance.GetConfigs<T>();
    }
    #endregion
}
