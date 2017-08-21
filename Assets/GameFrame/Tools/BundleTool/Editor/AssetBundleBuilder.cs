using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;
using AssetBundles;

public class AssetBundleBuilder : EditorWindow
{
    public const string InputKey = "InputBundleKey", OutputKey = "OutputBundleKey", optionKey = "BundleOptionKey";
    public static string InputPath;
    public static string OutputPath;
    BuildTarget target;
    BuildAssetBundleOptions option = BuildAssetBundleOptions.None;

    public static AssetBundleBuilder window;
    [MenuItem("Tools/AssetBundles/AssetBundleBuilder")]
    public static void Open()
    {
        window = CreateInstance<AssetBundleBuilder>();
        window.Init();
        window.Show();
    }

    void Init()
    {
        InputPath = EditorPrefs.GetString(InputKey, Application.dataPath);
        OutputPath = EditorPrefs.GetString(OutputKey, Application.streamingAssetsPath);
        option = (BuildAssetBundleOptions)EditorPrefs.GetInt(optionKey, 0);
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                target = BuildTarget.Android;
                break;
            case RuntimePlatform.IPhonePlayer:
                target = BuildTarget.iOS;
                break;
            default:
                target = BuildTarget.StandaloneWindows64;
                break;
        }

    }


    string GetPathAndSave(string key)
    {
        string path = EditorPrefs.GetString(key, "");
        path = EditorUtility.OpenFolderPanel("选择路径", path, "");
        EditorPrefs.SetString(key, path);
        return path;
    }

    bool isIncludDepend = true;
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        isIncludDepend = EditorGUILayout.ToggleLeft("是否包含依赖依赖", isIncludDepend);
        if (GUILayout.Button("设置BundleName"))
        {
            SetTotalAssetBundleName(isIncludDepend);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("输出BundleName"))
        {
            GetNames();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("清除所有BundleName"))
        {
            ClearAssetBundlesName();
        }
        EditorGUILayout.Space();
        target = (BuildTarget)EditorGUILayout.EnumPopup("平台", target);
        option = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("选项", option);
        if (GUILayout.Button("打包"))
        {
            //Debug.Log(option);
            EditorPrefs.SetInt(optionKey, (int)option);
            CreateAllAssetBundles();

        }
    }


    /// <summary>
    /// 查看所有的Assetbundle名称（设置Assetbundle Name的对象）
    /// </summary>
    void GetNames()
    {
        var names = AssetDatabase.GetAllAssetBundleNames(); //获取所有设置的AssetBundle
        foreach (var name in names)
            Debug.Log("AssetBundle: " + name);
    }

    /// <summary>
    /// 自动打包所有资源（设置了Assetbundle Name的资源）
    /// </summary>

    void CreateAllAssetBundles()
    {
        OutputPath = GetPathAndSave(OutputKey);
        if (string.IsNullOrEmpty(OutputPath)) return;

        //打包资源的路径，打包在对应平台的文件夹下
        string targetPath = OutputPath + "/AssetsResources/" + Utility.GetPlatformName();
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
        }
        if (option < 0)
        {
            EditorUtility.DisplayDialog("错误", "选项设置不正确", "OK");
        }
        AssetDatabase.RemoveUnusedAssetBundleNames();
        RemoveNoNamedBundle(targetPath);
        //打包资源
        BuildPipeline.BuildAssetBundles(targetPath, option, target);
        Debug.Log(option + "   " + target);
        //刷新编辑器
        AssetDatabase.Refresh();
    }




    public void SetTotalAssetBundleName(bool isDepend)
    {
        InputPath = GetPathAndSave(InputKey);
        SetAssetBundleName(InputPath, isDepend);
    }


    /// <summary>
    /// 设置资源的资源包名称
    /// </summary>
    /// <param name="path">资源主路径</param>
    /// <param name="ContainDependences">资源包中是否包含依赖资源的标志位：true表示分离打包，false表示整体打包</param>
    static void SetAssetBundleName(string path, bool ContainDependences = false)
    {
        //ClearAssetBundlesName();    //先清楚之前设置过的AssetBundleName，避免产生不必要的资源也打包

        if (Directory.Exists(path))
        {
            EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 0f);   //显示进程加载条
            DirectoryInfo dir = new DirectoryInfo(path);    //获取目录信息
            FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);  //获取所有的文件信息
            HashSet<string> baseBundle = new HashSet<string>();
            for (var i = 0; i < files.Length; ++i)
            {
                FileInfo fileInfo = files[i];
                EditorUtility.DisplayProgressBar("设置AssetName名称", "正在设置AssetName名称中...", 1f * i / files.Length);
                if (!fileInfo.Name.EndsWith(".meta"))   //判断去除掉扩展名为“.meta”的文件
                {
                    string basePath = "Assets" + fileInfo.FullName.Substring(Application.dataPath.Length);  //编辑器下路径Assets/..
                    baseBundle.Add(basePath);
                    string assetName = fileInfo.FullName.Substring(path.Length + 1);  //预设的Assetbundle名字，带上一级目录名称
                    assetName = assetName.Substring(0, assetName.LastIndexOf('.')); //名称要去除扩展名
                    assetName = assetName.Replace('\\', '/');   //注意此处的斜线一定要改成反斜线，否则不能设置名称
                    AssetImporter importer = AssetImporter.GetAtPath(basePath);
                    if (importer && importer.assetBundleName != assetName)
                    {
                        //Debug.Log(assetName);
                        importer.assetBundleName = assetName;  //设置预设的AssetBundleName名称
                        //importer.SaveAndReimport();
                    }
                }
            }
            if (ContainDependences)    //把依赖资源分离打包
            {
                Dictionary<string, int> dependMpa = new Dictionary<string, int>();
                foreach (var b in baseBundle)
                {
                    //获得他们的所有依赖，不过AssetDatabase.GetDependencies返回的依赖是包含对象自己本身的
                    string[] dps = AssetDatabase.GetDependencies(b, true); //获取依赖的相对路径Assets/...
                    //遍历设置依赖资源的Assetbundle名称，用哈希Id作为依赖资源的名称
                    for (int j = 0; j < dps.Length; j++)
                    {
                        //要过滤掉依赖的自己本身和脚本文件，自己本身的名称已设置，而脚本不能打包
                        if (baseBundle.Contains(dps[j]) || dps[j].Contains(".cs"))
                            continue;
                        else
                        {
                            if (dependMpa.ContainsKey(dps[j])) dependMpa[dps[j]]++;
                            else dependMpa[dps[j]] = 1;
                        }
                    }
                }
                var depends = from d in dependMpa where d.Value > 1 select d.Key;
                foreach (var d in depends)
                {
                    Debug.Log("依赖包个数:" + d.Count());
                    var importer = AssetImporter.GetAtPath(d);
                    if (string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        importer.assetBundleName = "alldependencies/" + importer.GetHashCode();
                        Debug.Log("Set depend :" + d);
                    }
                }

            }
            EditorUtility.ClearProgressBar();   //清除进度条
        }
    }

    /// <summary>
    /// 清除之前设置过的AssetBundleName，避免产生不必要的资源也打包 
    /// 因为只要设置了AssetBundleName的，都会进行打包，不论在什么目录下 
    /// </summary> 
    public void ClearAssetBundlesName()
    {
        string[] oldAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
        }
    }



    public static void RemoveNoNamedBundle(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        var bundleName = path + "/" + BuildPath.GetLastName(path);
        if (!File.Exists(bundleName)) return;
        var bundle = AssetBundle.LoadFromFile(bundleName);
        if (bundle == null) return;
        var mainFest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        if (mainFest == null) return;
        var oldBundles = mainFest.GetAllAssetBundles();
        var newBundles = new HashSet<string>(AssetDatabase.GetAllAssetBundleNames());
        foreach (var name in oldBundles)
        {
            if (!newBundles.Contains(name))
            {
                var fullName = path + "/" + name;
                if (File.Exists(fullName))
                    File.Delete(fullName);
                fullName = path + "/" + name + ".manifest";
                if (File.Exists(fullName))
                    File.Delete(fullName);
                fullName = path + "/" + name + ".meta";
                if (File.Exists(fullName))
                    File.Delete(fullName);
            }
        }
        bundle.Unload(true);
        AssetDatabase.Refresh();
        DeleteEmptyFolder(path);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 删除所有空文件夹
    /// </summary>
    /// <param name="rootPath"></param>
    public static void DeleteEmptyFolder(string rootPath)
    {
        if (Directory.Exists(rootPath))
        {
            var dirs = Directory.GetDirectories(rootPath);
            if (dirs != null && dirs.Length > 0)
            {
                foreach (var d in dirs)
                {
                    DeleteEmptyFolder(d);
                }
            }
            var fs = Directory.GetFiles(rootPath);
            if (fs == null || fs.Length == 0)
            {
                Directory.Delete(rootPath);
            }
        }
    }

    //[MenuItem("Test/tt")]
    //public static void GetDependencies()
    //{
    //    var path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //    Debug.Log(path);
    //    var ds = AssetDatabase.GetDependencies(path);
    //    foreach (var d in ds) Debug.Log(d);
    //}

    //[MenuItem("Test/tt2")]
    //public static void GetDependencies2()
    //{
    //    var path = AssetDatabase.GetAssetPath(Selection.activeObject);
    //    Debug.Log(path);
    //    var ds = AssetDatabase.GetDependencies(path, false);
    //    foreach (var d in ds) Debug.Log(d);
    //}


}


