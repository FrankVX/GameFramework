using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConfigManager : Manager<ConfigManager>
{

    private ConfigsContainer container;

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        container = null;
        AssetManager.Instance.LoadAsset<ConfigsContainer>("Configs/ConfigsContainer", InitConfigs);
        while (container == null) yield return null;
    }

    public Dictionary<string, Dictionary<int, BaseConfig>> datas;

    public void InitConfigs(ConfigsContainer container)
    {
        this.container = container;
        datas = new Dictionary<string, Dictionary<int, BaseConfig>>();
        foreach (var c in container.allconfigs)
        {
            if (c == null) continue;
            if (!datas.ContainsKey(c.type)) datas[c.type] = new Dictionary<int, BaseConfig>();
            foreach (var c2 in c)
            {
                datas[c.type][c2.Id] = c2;
            }
        }
    }

    public new T GetConfig<T>(int id) where T : BaseConfig
    {
        var name = typeof(T).Name;
        if (datas.ContainsKey(name) && datas[name].ContainsKey(id))
            return datas[name][id] as T;
        return null;
    }

    public new List<T> GetConfigs<T>() where T : BaseConfig
    {
        var name = typeof(T).Name;
        if (datas.ContainsKey(name))
        {
            return datas[name].Values.Select((v) => v as T).ToList();
        }
        return null;
    }
}
