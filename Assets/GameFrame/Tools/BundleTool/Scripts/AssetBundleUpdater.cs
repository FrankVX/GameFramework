using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

public class AssetBundleUpdater : MonoBehaviour
{

    public static AssetBundleManifest AssetBundleManifest { get { return assetBundleManifest; } }
    static AssetBundleManifest assetBundleManifest;
    /// <summary>
    /// UI界面显示的提示信息
    /// </summary>
    private Text ShowMessage;
    /// <summary>
    /// BundleManiFest
    /// </summary>
    private string filelist = "AssetsResources";
    /// <summary>
    /// 资源更新列表，更新完后要删除
    /// </summary>
    private string updateFilelist = "updateFilelist.txt";
    /// <summary>
    /// 比较本地文件和最新文件后，判断是否需要更新的标志位
    /// </summary>
    private bool isNeedUpdate = false;

    // Use this for initialization
    void Start()
    {

        ShowMessage = transform.FindChild("Info").GetComponent<Text>();
        ShowMessage.text = "正在初始化......";
        if (Application.platform != RuntimePlatform.WindowsEditor)
            CheckBundleList();
        //Debug.Log(BuildPath._Instance.ResourceFolder);

    }

    /// <summary>
    /// 检查资源列表
    /// </summary>
    public void CheckBundleList()
    {
        if (IsNeedExportAssetBundle()) //判断是否需要解压资源
        {
            //开始解压资源
            StartCoroutine(ExportFileReources(() =>
            {
                //开始检查资源
                StartCoroutine(CheckResources(() =>
                    {
                        //开始更新资源
                        StartCoroutine(UpdateResource(() =>
                        {
                            //开始准备运行初始化
                            StartLoadData();
                        }));

                    }));
            }));
        }
        else
        {
            //开始检查资源
            StartCoroutine(CheckResources(() =>
            {
                //开始更新资源
                StartCoroutine(UpdateResource(() =>
                {
                    //开始准备运行初始化
                    //StartLoadData();
                }));
            }));
        }
    }

    /// <summary>
    /// 开始加载资源信息，检查完资源后开始加载数据
    /// </summary>
    public void StartLoadData()
    {
        //Assembly assem = LoadAssembly(BuildPath._Instance.ResourceFolder + "Demo.dll");
        //Type t = assem.GetType("AssetBundleDemo.SourceManager");    //获取SourceManager脚本组件

        //GameObject GlobalObj = new GameObject("GlobalObj");
        //GlobalObj.AddComponent(t);

        //Destroy(this.gameObject, 1f);

    }



    Dictionary<string, Hash128> needUpdateBundle;

    /// <summary>
    /// 比较检查资源，查看资源是否全新
    /// </summary>
    /// <param name="UpdateData">比较完成后执行的委托</param>
    /// <returns></returns>
    IEnumerator CheckResources(Action UpdateData = null)
    {
        ShowMessage.text = "正在检查资源......";


        string localFilePath = BuildPath.ResourceFolder + filelist;    //本地外部路径
        string newFilePath = BuildPath.ServerResourcePath + filelist;   //要对比更新的最新文件路径


        //获取最新的Manifest文件
        ManifestInfo newManifestInfo = new ManifestInfo();
        yield return newManifestInfo.GetManifest(newFilePath);

        if (newManifestInfo.manifest == null)  //判断是否获取到
        {
            Debug.Log("获取最新的Manifest失败！");
            yield break;
        }
        else
        {
            if (File.Exists(localFilePath))
            {
                //获取本地Manifest文件信息
                ManifestInfo localManifestInfo = new ManifestInfo();
                yield return localManifestInfo.GetManifest("file://" + localFilePath);
                if (localManifestInfo.manifest == null)  //判断是否获取到
                {
                    Debug.Log("获取本地Manifest失败！");
                    needUpdateBundle = newManifestInfo.DictBundleNamesHashID;
                }
                else
                {
                    //开始比较本地和最新的Manifest是否一致，从而判断是否需要更新资源
                    int count = newManifestInfo.DictBundleNamesHashID.Count;  //最新资源的总数
                    int index = 0;  //计数器
                    needUpdateBundle = new Dictionary<string, Hash128>();
                    //开始比较资源列表信息：1、比较是否存在资源；2、比较资源的HashID是否一样
                    foreach (var p in newManifestInfo.DictBundleNamesHashID)
                    {
                        //1、先判断本地资源列表是否存在,若不存在则属于新资源
                        if (!localManifestInfo.DictBundleNamesHashID.ContainsKey(p.Key))
                        {
                            needUpdateBundle.Add(p.Key, p.Value);
                        }
                        else
                        {
                            //2、再判断HashID是否相等,若不想等在则属于新资源
                            var hashCode = newManifestInfo.DictBundleNamesHashID[p.Key];   //获取最新资源的HashID
                            if (localManifestInfo.DictBundleNamesHashID[p.Key] != hashCode)
                            {
                                needUpdateBundle.Add(p.Key, p.Value);
                            }
                        }
                        //显示检查进度
                        index++;
                        ShowMessage.text = "正在检查资源......" + (int)((float)index * 100 / count) + "%";

                    }
                }
            }
            else
            {
                Debug.Log("本地资源目录不存在，下载全部资源！");
                needUpdateBundle = newManifestInfo.DictBundleNamesHashID;
            }
            ShowMessage.text = "资源检查完毕";
            yield return new WaitForSeconds(0.5f);  //等待0.5s

            if (UpdateData != null)
                UpdateData.Invoke();
        }
    }


    /// <summary>
    /// 根据更新列表更新资源
    /// </summary>
    /// <param name="beginRun">更新完资源后准备运行的委托</param>
    /// <returns></returns>
    IEnumerator UpdateResource(Action beginRun = null)
    {
        //如果需要更新资源
        if (needUpdateBundle != null && needUpdateBundle.Count > 0)
        {
            ShowMessage.text = "正在更新资源......";
            int num = 0;
            foreach (var p in needUpdateBundle)
            {
                num++;
                string inFilePath = BuildPath.ServerResourcePath + p.Key;
                while (!Caching.enabled) yield return null;
                WWW wwwfile = WWW.LoadFromCacheOrDownload(inFilePath, p.Value);

                yield return wwwfile;
                if (!string.IsNullOrEmpty(wwwfile.error))
                {
                    Debug.Log("资源名不存在：" + wwwfile.error);
                    yield break;
                }
                else
                {
                    Debug.Log(p.Key + "资源缓存完成：");
                }
                wwwfile.Dispose();
                ShowMessage.text = "正在更新资源......" + (int)((float)num * 100 / needUpdateBundle.Count) + "%" + '\n' + "本次更新不耗费流量";
            }
            yield return StartCoroutine(DownloadResource(filelist)); //保存最新的ManiFest
            ShowMessage.text = "资源更新完成！";
            yield return new WaitForSeconds(0.5f);
            Debug.Log("更新完成!!!");
        }
        else
        {
            Debug.Log("不需要更新!!!");
        }
        yield return new WaitForEndOfFrame();
        if (beginRun != null)
            beginRun.Invoke();
    }

    /// <summary>
    /// 下载资源
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    IEnumerator DownloadResource(string bundleName)
    {
        string inFilePath = BuildPath.ServerResourcePath + bundleName;

        using (WWW wwwfile = new WWW(inFilePath))
        {
            yield return wwwfile;
            if (!string.IsNullOrEmpty(wwwfile.error))
            {
                Debug.Log("资源名不存在：" + wwwfile.error);
            }
            else
            {
                string outFilePath = BuildPath.ResourceFolder + bundleName;
                SaveBytes(outFilePath, wwwfile.bytes);
            }
        }
    }


    IEnumerator DownloadResource(string bundleName, Hash128 id)
    {
        string inFilePath = BuildPath.FileStreamFolder + bundleName;
        WWW wwwfile = WWW.LoadFromCacheOrDownload(inFilePath, id);

        yield return wwwfile;
        if (!string.IsNullOrEmpty(wwwfile.error))
        {
            Debug.Log("资源名不存在：" + wwwfile.error);
        }
        else
        {
            string outFilePath = BuildPath.ResourceFolder + bundleName;
            Debug.Log(wwwfile.assetBundle.name + "资源缓存完成：");
        }
    }


    /// <summary>
    /// 第一次运行时，将资源列表和资源包全部导出
    /// </summary>
    /// <param name="UpdateReoures">更新资源的委托</param>
    /// <returns></returns>
    IEnumerator ExportFileReources(Action UpdateReoures = null)
    {
        string filePath = BuildPath.FileStreamFolder + filelist;  //内部路径

        string[] bundleList = null;//资源列表的资源信息

        AssetBundleManifest manifest = null;
        //下载资源列表到外部路径
        using (WWW wwwManifest = new WWW(filePath))
        {
            yield return wwwManifest;
            if (!string.IsNullOrEmpty(wwwManifest.error))
            {
                Debug.Log("获取AssetBundleManifest出错！");
            }
            else
            {
                AssetBundle manifestBundle = wwwManifest.assetBundle;
                manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");    //获取总的Manifest
                string outPath = BuildPath.ResourceFolder + filelist; //外部路径
                SaveBytes(outPath, wwwManifest.bytes);  //保存总的Manifest到外部路径
                bundleList = manifest.GetAllAssetBundles();   //获取所有的资源信息
                manifestBundle.Unload(false);   //释放掉
            }
        }
        if (bundleList == null)
        {
            yield return null;
        }
        else
        {
            if (manifest != null)
            {
                for (int i = 0; i < bundleList.Length; i++)
                {
                    Debug.Log("sss");
                    yield return DownloadResource(bundleList[i], manifest.GetAssetBundleHash(bundleList[i]));    //等待返回下载结果
                    ShowMessage.text = "第一次运行，解压资源..." + (int)((float)i * 100 / bundleList.Length) + "%" + '\n' + "解压不耗费流量";
                }
            }
        }

        ShowMessage.text = "解压完成，等待初始化...";
        yield return new WaitForSeconds(1f);    //资源全部导出后等待1s

        if (UpdateReoures != null)
            UpdateReoures.Invoke();
    }


    /// <summary>
    /// 判断是否需要导出资源列表的文本
    /// </summary>
    public bool IsNeedExportAssetBundle()
    {
        string outManifestPath = BuildPath.ResourceFolder + filelist;

        if (File.Exists(outManifestPath))
        {
            return false;
        }
        else
        {
            return true;
        }

    }

    /// <summary>
    /// 导出dll文件到外部路径
    /// </summary>
    /// <returns></returns>
    public IEnumerator ExportDll()
    {
        string inDllPath = BuildPath.FileStreamFolder + "Demo.dll";
        string outDllPath = BuildPath.ResourceFolder + "Demo.dll";

        using (WWW www = new WWW(inDllPath))
        {
            yield return www;
            if (www.error != null)
            {
                Debug.Log("工程内部不存在dll文件！");
            }
            else
            {
                SaveBytes(outDllPath, www.bytes);
            }
        }

    }

    /// <summary>
    /// 判断是否需要导出dll文件
    /// </summary>
    /// <returns></returns>
    public bool IsNeedExportDll()
    {
        string outDllPath = BuildPath.ResourceFolder + "Demo.dll";
        if (File.Exists(outDllPath))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <param name="fileFullName">要写入的文件全路径</param>
    /// <param name="data">字节数据</param>
    public void SaveBytes(string fileFullName, byte[] data)
    {
        if (File.Exists(fileFullName))
            File.Delete(fileFullName);
        else if (!Directory.Exists(GetDirectoryName(fileFullName)))  //判断目录是否存在
            Directory.CreateDirectory(GetDirectoryName(fileFullName));

        using (FileStream fs = new FileStream(fileFullName, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(data); //写入
                bw.Flush(); //清空缓冲区
                bw.Close(); //关闭流
            }
            fs.Close();
        }

    }

    /// <summary>
    /// 获取文件的目录
    /// </summary>
    /// <param name="fileFullName">文件全路径</param>
    /// <returns>返回目录全路径</returns>
    public string GetDirectoryName(string fileFullName)
    {
        return fileFullName.Substring(0, fileFullName.LastIndexOf('/'));
    }


}

/// <summary>
/// 总的Manifest信息，获取到所有的资源包内容
/// </summary>
public class ManifestInfo
{
    /// <summary>
    /// 总的Manifest
    /// </summary>
    public AssetBundleManifest manifest;

    /// <summary>
    /// 保存所有AssetBundle的名称和HashID的容器
    /// </summary>
    public Dictionary<string, Hash128> DictBundleNamesHashID = new Dictionary<string, Hash128>();

    public ManifestInfo()   //空的构造函数
    {
        manifest = null;
    }

    public ManifestInfo(AssetBundleManifest manifest)   //构造函数
    {
        this.manifest = manifest;

        GetBundleNamesAndHashID();

    }

    /// <summary>
    /// 获取总的Manifest
    /// </summary>
    /// <param name="manifestPath">总的Manifest路径</param>
    /// <returns></returns>
    public IEnumerator GetManifest(string manifestPath)
    {
        //下载资源列表到外部路径
        using (WWW wwwManifest = new WWW(manifestPath))
        {
            yield return wwwManifest;
            if (!string.IsNullOrEmpty(wwwManifest.error))
            {
                Debug.Log("获取AssetBundleManifest出错！");
            }
            else
            {
                AssetBundle manifestBundle = wwwManifest.assetBundle;
                manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");    //获取总的Manifest
                manifestBundle.Unload(false);
                GetBundleNamesAndHashID();
            }
        }
    }

    /// <summary>
    /// 获取当前总的Manifest中所有的AssetBundle名称与其HashID的对应集合
    /// </summary>
    public void GetBundleNamesAndHashID()
    {
        string[] assetBundleNames = manifest.GetAllAssetBundles();  //所有的资源包名称

        for (int i = 0; i < assetBundleNames.Length; i++)
        {
            DictBundleNamesHashID.Add(assetBundleNames[i], manifest.GetAssetBundleHash(assetBundleNames[i]));
        }

    }

}