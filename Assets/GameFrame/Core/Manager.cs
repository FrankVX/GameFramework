using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager<T> : Singleton<T>, IManager where T : Manager<T>
{
    public bool IsInitialized { get; private set; }
    public virtual IEnumerator Initialize()
    {
        IsInitialized = true;
        yield return null;
    }
}

interface IManager
{
    bool IsInitialized { get; }
    IEnumerator Initialize();
}
