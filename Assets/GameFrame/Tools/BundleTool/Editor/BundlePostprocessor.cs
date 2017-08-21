using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BundlePostprocessor : AssetPostprocessor
{

    public const string AssetPath = "Assets/AssetBundle/";

    public const string ConfigPath = AssetPath + "Configs";

    static HashSet<string> filiter = new HashSet<string>() { ".meta", ".cs" };

    /// <summary>
    /// 自动为AssetPath下的资源添加AssetBundle的Name
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            SetBundleName(str);
        }

        foreach (string str in movedAssets)
        {
            SetBundleName(str);
        }
    }



    static void SetBundleName(string str)
    {
        //Debug.Log("Reimported Asset: " + str);
        if (!Path.HasExtension(str) || filiter.Contains(Path.GetExtension(str))) return;
        if (str.Contains(AssetPath))
        {
            var dir = Path.GetDirectoryName(str);
            AssetImporter importer = AssetImporter.GetAtPath(str);
            //if (str.Contains(ConfigPath) && dir != ConfigPath)
            //{
            //    var name = "Configs" + dir.Substring(dir.LastIndexOf('/'));
            //    importer.assetBundleName = name;
            //}
            //else
            {
                var name = str.Substring(0, str.LastIndexOf('.')).Replace(AssetPath, "");
                importer.assetBundleName = name;
            }
        }
    }
}
