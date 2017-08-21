using UnityEngine;
using System.Collections;


public static class BuildPath
{
    static string PCPath = Application.dataPath + "/../Captures/";

    static string AdnroidPath = "/storage/emulated/0/T-ShirtPhotos/";

    static string MacPath = Application.dataPath + "/../Captures/";

    static string IphonePath = Application.dataPath + "/../Captures/";


    /// <summary>
    /// 服务器资源URL
    /// </summary>
    public static string ServerResourcePath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return FileStreamFolder;
                default:
                    return FileStreamFolder;
            }

        }
    }

    public static string GetLastName(string path)
    {
        return path.Substring(path.LastIndexOf('/') + 1);
    }



    /// <summary>
    /// 外部资源存放的路径（AssetBundle、xml、txt）
    /// </summary>
    public static string ResourceFolder
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:   //安卓
                case RuntimePlatform.IPhonePlayer:  //Iphone
                    return string.Concat(Application.persistentDataPath, "/AssetsResources/");
                case RuntimePlatform.OSXEditor: //MAC
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor: //windows
                case RuntimePlatform.WindowsPlayer:
                    return string.Concat(Application.dataPath, "/../AssetsResources/");
                default:
                    return string.Concat(Application.dataPath, "/../AssetsResources/");

            }

        }
    }

    /// <summary>
    /// 初始资源存放的位置（只读）
    /// </summary>
    public static string FileStreamFolder
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:   //安卓
                case RuntimePlatform.IPhonePlayer:  //Iphone
                    return string.Concat(Application.streamingAssetsPath, "/AssetsResources/");
                case RuntimePlatform.OSXEditor: //MAC
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor: //windows
                case RuntimePlatform.WindowsPlayer:
                    return string.Concat("file://", Application.streamingAssetsPath, "/AssetsResources/");
                default:
                    return string.Concat("file://", Application.streamingAssetsPath, "/AssetsResources/");
            }
        }
    }

    /// <summary>
    /// 图片存放的路径
    /// </summary>
    public static string TargetTexturePath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:   //安卓
                    return AdnroidPath;
                case RuntimePlatform.WindowsEditor: //windows
                case RuntimePlatform.WindowsPlayer:
                    return PCPath;
                case RuntimePlatform.OSXEditor: //MAC
                case RuntimePlatform.OSXPlayer:
                    return MacPath;
                case RuntimePlatform.IPhonePlayer:  //Iphone
                    return IphonePath;
                default:
                    return null;
            }
        }
    }


}