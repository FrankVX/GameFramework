using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ModelManager : Manager<ModelManager>
{

    Dictionary<Type, IModel> models = new Dictionary<Type, IModel>();

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        var types = AssemblyManager.Instance.GetChildTypes<IModel>();
        foreach (var t in types)
        {
            if (!t.IsGenericType)
            {
                var go = new GameObject(t.Name);
                go.transform.SetParent(transform);
                models[t] = go.AddComponent(t) as IModel;
            }
        }
    }


}
