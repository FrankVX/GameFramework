using UnityEngine;
using System.Collections;
using System.Reflection;
using System;
using System.Collections.Generic;

public class AssemblyManager : Manager<AssemblyManager>
{
    public Assembly CurrentAssembly { get; private set; }

    public Type[] Types { get; private set; }

    protected override void Awake()
    {
        CurrentAssembly = Assembly.GetExecutingAssembly();
        Types = CurrentAssembly.GetTypes();
        base.Awake();
    }
    /// <summary>
    /// 获取所有子类(不包括自身)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<Type> GetChildTypes<T>()
    {
        var type = typeof(T);
        foreach (var t in Types)
        {
            if (type.IsAssignableFrom(t) && !t.Equals(type) && !t.IsAbstract)
            {
                yield return t;
            }
        }
    }
}
