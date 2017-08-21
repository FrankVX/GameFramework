using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public abstract class ConfigPackage : ScriptableObject
{
    public string type;

    public abstract IEnumerator<BaseConfig> GetEnumerator();

}


public class ConfigsContainer : ScriptableObject
{
    [SerializeField]
    public List<ConfigPackage> allconfigs;
}
