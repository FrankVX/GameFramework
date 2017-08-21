using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance
    {
        get
        {
            return SingletonManager.Instance.GetSingleton<T>();
        }
    }

    protected virtual void Awake()
    {
        SingletonManager.Instance.RegisterSingleton(this);
    }

}
