using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AssetBundles;


public class AssetManager : Manager<AssetManager>
{
    /// <summary>
    /// Asset缓存
    /// </summary>
    Dictionary<string, UnityEngine.Object> assets = new Dictionary<string, UnityEngine.Object>();

    public override IEnumerator Initialize()
    {
        yield return base.Initialize();
        AssetBundleManager.SetSourceAssetBundleURL("file://" + BuildPath.ResourceFolder);
        var req = AssetBundleManager.Initialize();
        while (req != null && !req.IsDone()) yield return null;
    }

    public void LoadAsset(string assetbundleName, Action<UnityEngine.Object> callBack)
    {
        var assetName = assetbundleName.Substring(assetbundleName.LastIndexOf('/'));
        LoadAsset(assetbundleName, assetName, callBack);
    }

    public new void LoadAsset<T>(string assetbundleName, Action<T> callBack) where T : UnityEngine.Object
    {
        var assetName = assetbundleName.Substring(assetbundleName.LastIndexOf('/') + 1);
        LoadAsset(assetbundleName, assetName, callBack);
    }

    public void LoadAsset(string assetbundleName, string assetName, Action<UnityEngine.Object> callBack)
    {
        StartCoroutine(m_LoadAsset(assetbundleName, assetName, callBack));
    }
    public void LoadAsset<T>(string assetbundleName, string assetName, Action<T> callBack) where T : UnityEngine.Object
    {
        StartCoroutine(m_LoadAsset(assetbundleName, assetName, callBack));
    }

    IEnumerator m_LoadAsset<T>(string assetbundleName, string assetName, Action<T> callBack) where T : UnityEngine.Object
    {
        if (callBack == null) yield break;
        assetbundleName = assetbundleName.ToLower();
        string assetKey = string.Concat(assetbundleName, "_", assetName);
        if (assets.ContainsKey(assetKey))
        {
            callBack(assets[assetKey] as T);
            yield break;
        }
        var type = typeof(T);
        var request = AssetBundleManager.LoadAssetAsync(assetbundleName, assetName, type);
        if (request == null)
            yield break;
        while (!request.IsDone()) yield return null;
        assets[assetKey] = request.GetAsset<T>();
        callBack(assets[assetKey] as T);
        AssetBundleManager.UnloadAssetBundle(assetbundleName);
    }


}
