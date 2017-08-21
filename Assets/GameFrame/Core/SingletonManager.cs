using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonManager : Manager<SingletonManager>
{

    public new static SingletonManager Instance
    {
        get
        {
            if (instance == null) instance = CreatInstance<SingletonManager>();
            return instance;
        }
    }

    static SingletonManager instance;

    Dictionary<Type, MonoBehaviour> singletons = new Dictionary<Type, MonoBehaviour>();

    protected override void Awake()
    {

    }

    public void RegisterSingleton(MonoBehaviour singleton)
    {
        var type = singleton.GetType();
        if (singletons.ContainsKey(type))
        {
            Debug.LogError("singleton oready have!");
            Destroy(singleton);
            return;
        }
        singletons[type] = singleton;
        singleton.gameObject.transform.parent = transform;
    }

    public T GetSingleton<T>() where T : MonoBehaviour
    {
        var type = typeof(T);
        if (!singletons.ContainsKey(type))
            singletons[type] = CreatInstance<T>();
        return singletons[type] as T;
    }

    static T CreatInstance<T>() where T : MonoBehaviour
    {
        var name = typeof(T).Name;
        var obj = new GameObject(name).AddComponent<T>();
        DontDestroyOnLoad(obj.gameObject);
        return obj;
    }

}
