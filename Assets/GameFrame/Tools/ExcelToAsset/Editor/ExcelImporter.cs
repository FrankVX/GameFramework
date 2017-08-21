using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Excel;
using System.IO;
using System.Data;
using System;
using System.Text;
using System.Linq;
using UnityEditor.AnimatedValues;
using System.Reflection;

public class ExcelImporter : EditorWindow
{

    string singleFilePath = "", scrExcelPath = "", outScriptPath = "", saveAssetPath = "";
    static ExcelImporter instance;

    [MenuItem("Tools/Excel2Assets")]
    public static void Open()
    {
        instance = CreateInstance<ExcelImporter>();
        instance.Init();
        instance.Show();
    }

    List<Cfg> cfgs = new List<Cfg>();

    Dictionary<string, ExcelDataConverter> Converters = new Dictionary<string, ExcelDataConverter>();

    void Init()
    {
        scrExcelPath = EditorPrefs.GetString("scrExcelPath", "");
        outScriptPath = EditorPrefs.GetString("outScriptPath", "");
        saveAssetPath = EditorPrefs.GetString("saveAssetPath", "");
        InitConverters();
    }

    void InitConverters()
    {
        var types = GetType().Assembly.GetTypes();
        var basetype = typeof(ExcelDataConverter);
        var attr = typeof(ConverterAttribute);
        types = (from t in types where basetype.IsAssignableFrom(t) && t != basetype select t).ToArray();

        foreach (var t in types)
        {
            var ass = t.GetCustomAttributes(attr, false);
            foreach (var a in ass)
            {
                var aa = a as ConverterAttribute;
                Converters[aa.type] = Activator.CreateInstance(t) as ExcelDataConverter;
            }
        }
    }


    object Convert(string type, string value)
    {
        if (Converters.Count == 0) InitConverters();
        if (Converters.ContainsKey(type))
        {
            return Converters[type].Convert(value);
        }
        return null;
    }

    bool LoadExcel()
    {
        if (isSingleFile && !File.Exists(singleFilePath) || !isSingleFile && !Directory.Exists(scrExcelPath))
        {
            EditorUtility.DisplayDialog("警告", "Excel路径不存在,请检查路径!", "好的");
            return false;
        }

        string[] files = null;
        if (isSingleFile)
        {
            files = new string[] { singleFilePath };
        }
        else
        {
            files = Directory.GetFiles(scrExcelPath);
        }
        cfgs.Clear();
        foreach (var ff in files)
        {
            var f = ff.Replace("\\", "/");
            string filename = "";
            IExcelDataReader excelReader = null;
            string extension = Path.GetExtension(f);
            using (FileStream stream = File.Open(f, FileMode.Open, FileAccess.Read))
            {
                switch (extension)
                {
                    case ".xlsx":
                        filename = f.Substring(f.LastIndexOf("/") + 1).Replace(".xlsx", "");
                        excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                        break;
                    case ".xls":
                        filename = f.Substring(f.LastIndexOf("/") + 1).Replace("xlx", "");
                        excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                        break;
                    default:
                        break;
                }
                if (excelReader == null) continue;
                Debug.Log(filename);
                DataSet result = excelReader.AsDataSet();

                if (!ParseDataSet(filename, result)) return false;

                //excelReader.Close(); //这里会引起报错不知道为啥
            }
        }
        return true;
    }


    bool ParseDataSet(string filename, DataSet data)
    {
        if (data == null)
        {
            EditorUtility.DisplayDialog("警告", string.Format("{1}读取时出现错误!", filename), "好的");
            return false;
        }
        var table = data.Tables[0];
        if (table == null)
        {
            EditorUtility.DisplayDialog("警告", string.Format("{1}的第一个Table页不存在!", filename), "好的");
            return false;
        }
        var rows = table.Rows;
        if (rows == null)
        {
            EditorUtility.DisplayDialog("警告", string.Format("{1}的数据为空!", filename), "好的");
            return false;
        }
        if (rows.Count < 3)
        {
            EditorUtility.DisplayDialog("警告", string.Format("{1}的行数不正确,请保证至少有三行!", filename), "好的");
            return false;
        }

        Cfg cfg = new Cfg();
        cfg.Name = filename + "Cfg";

        //字段名
        foreach (var n in rows[0].ItemArray)
        {
            cfg.fileNames.Add(n.ToString());
        }

        //类型
        foreach (var n in rows[1].ItemArray)
        {
            string t = n.ToString().ToLower();
            cfg.types.Add(t);
        }

        //注释
        foreach (var n in rows[2].ItemArray)
        {
            cfg.notates.Add(n.ToString());
        }

        //数据
        for (int i = 3; i < rows.Count; i++)
        {
            cfg.items.Add(rows[i]);
        }
        if (cfg.CheckID())
            cfgs.Add(cfg);
        else
        {
            EditorUtility.DisplayDialog("警告", string.Format("{0}的ID字段不存在或者有重复ID,请检查!", cfg.Name), "好的");
            return false;
        }
        return true;
    }

    /// <summary>  </summary>
    /// <param name="path"></param>
    bool WriteCode(string path)
    {
        if (string.IsNullOrEmpty(outScriptPath) || !Directory.Exists(outScriptPath))
        {
            EditorUtility.DisplayDialog("警告", "脚本输出路径不正确,请检查!", "好的");
            return false;
        }
        if (cfgs == null || cfgs.Count == 0)
        {
            EditorUtility.DisplayDialog("警告", "请重新读取数据!", "好的");
            return false;
        }
        string p;
        StringBuilder sb = new StringBuilder();
        ScriptsWriter sw = new ScriptsWriter(sb);

        foreach (var cfg in cfgs)
        {
            p = string.Concat(path, "/", cfg.Name, "Package", ".cs");
            sb.Length = 0;
            var count = cfg.types.Count;
            if (cfg.fileNames.Count == count && count == cfg.notates.Count && count > 0)
            {
                sw.Head();
                sw.Class(cfg.Name + "Package", "ConfigPackage");
                sb.AppendFormat("\tpublic List<{0}> list = new List<{0}>();\n", cfg.Name);
                sb.Append("\tpublic override IEnumerator<BaseConfig> GetEnumerator()\n");
                sb.AppendLine("\t{");
                sb.AppendLine("\t\tforeach (var s in list) yield return s;");
                sb.AppendLine("\t}");
                sw.End();
                sb.AppendLine("[Serializable]");
                sw.Class(cfg.Name, "BaseConfig");
                for (int i = 0; i < cfg.fileNames.Count; i++)
                {
                    if (cfg.fileNames[i].ToLower() == "id") continue;
                    sw.Member(cfg.types[i], cfg.fileNames[i], cfg.notates[i]);
                }
                sw.End();
            }
            Save(p, sb.ToString());
        }
        AssetDatabase.Refresh();
        return true;
    }


    bool SaveAll()
    {
        if (EditorApplication.isCompiling)
        {
            EditorUtility.DisplayDialog("警告", "请在编译完成之后在加载数据!", "好的");
            return false;
        }
        if (string.IsNullOrEmpty(saveAssetPath) || !Directory.Exists(saveAssetPath))
        {
            EditorUtility.DisplayDialog("警告", "保存数据路径不正确!", "好的");
            return false;
        }

        if (cfgs == null || cfgs.Count == 0)
        {
            EditorUtility.DisplayDialog("警告", "请重新读取数据!", "好的");
            return false;
        }
        string path = saveAssetPath.Substring(saveAssetPath.LastIndexOf("Assets"));

        List<ConfigPackage> list = new List<ConfigPackage>();
        FieldInfo field = cc.GetType().GetField("allconfigs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (isSingleFile && cc != null)
        {
            var temp = field.GetValue(cc) as List<ConfigPackage>;
            foreach (var t in temp)
            {
                if (t != null)
                {
                    list.Add(t);
                }
            }
        }
        else
        {
            cc = CreateInstance<ConfigsContainer>();
        }
        foreach (var c in cfgs)
        {
            var typeCfg = typeof(BaseConfig).Assembly.GetType(c.Name);
            var typePackage = typeof(BaseConfig).Assembly.GetType(c.Name + "Package");
            for (int i = 0; i < c.items.Count; i++)
            {
                if (!SaveData(c, i, typePackage, typeCfg)) return false;
            }
            list.Add(GetPackage(path, typePackage, typeCfg));
        }
        field.SetValue(cc, list);
        if (!isSingleFile)
        {
            AssetDatabase.CreateAsset(cc, path + "/ConfigsContainer" + ".asset");
        }
        AssetDatabase.Refresh();
        return true;
    }

    ConfigPackage GetPackage(string path, Type packageType, Type cfgType)
    {
        var field = packageType.GetField("list", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        var methodAdd = field.FieldType.GetMethod("Add");
        var pt = Activator.CreateInstance(packageType) as ConfigPackage;
        pt.type = cfgType.Name;
        var l = field.GetValue(pt);
        foreach (var p in configs[packageType].Values)
        {
            methodAdd.Invoke(l, new object[] { p });
        }
        AssetDatabase.CreateAsset(pt, string.Concat(path, "/", cfgType.Name, ".asset"));
        return pt;
    }


    Dictionary<Type, Dictionary<int, BaseConfig>> configs = new Dictionary<Type, Dictionary<int, BaseConfig>>();

    bool SaveData(Cfg cfg, int id, Type typePackage, Type typeCfg)
    {
        var datas = cfg.items[id];
        var obj = Activator.CreateInstance(typeCfg) as BaseConfig;
        if (obj != null)
        {
            Dictionary<int, BaseConfig> dic;
            if (!configs.ContainsKey(typePackage))
            {
                configs[typePackage] = new Dictionary<int, BaseConfig>();
            }
            dic = configs[typePackage];

            var items = datas.ItemArray;
            if (items.Length == 0)
            {
                EditorUtility.DisplayDialog("警告", string.Format("{0}的第{1}行的数据有误!", cfg.Name, id + 4), "好的");
                return false;
            }
            for (int i = 0; i < cfg.fileNames.Count; i++)
            {
                var name = "m_" + cfg.fileNames[i];
                var f = typeCfg.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (f == null)
                    f = typeCfg.BaseType.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (f == null) continue;
                var str = items[i].ToString();
                if (string.IsNullOrEmpty(str)) continue;
                object value = Convert(cfg.types[i], str);
                if (value != null)
                    f.SetValue(obj, value);
            }
            dic[obj.Id] = obj;
        }
        else
        {
            EditorUtility.DisplayDialog("警告", string.Format("{0}表的类结构并不存在,请检查!", cfg.Name), "好的");
            return false;
        }
        return true;
    }



    void Save(string p, string data)
    {
        var st = File.Open(p, FileMode.OpenOrCreate, FileAccess.Write);
        byte[] buffer = Encoding.UTF8.GetBytes(data);
        st.SetLength(0);
        st.Write(buffer, 0, buffer.Length);
        st.Close();
    }


    bool isSingleFile;
    ConfigsContainer cc;
    private void OnGUI()
    {
        isSingleFile = EditorGUILayout.ToggleLeft("指定单个文件", isSingleFile);
        EditorGUILayout.BeginHorizontal();

        if (isSingleFile)
        {
            EditorGUILayout.LabelField("文件路径", GUILayout.Width(100));
            singleFilePath = EditorGUILayout.TextField(singleFilePath);
            if (GUILayout.Button("浏览"))
            {
                singleFilePath = EditorUtility.OpenFilePanel("Select File", singleFilePath, "");
                EditorPrefs.SetString("scrExcelFile", singleFilePath);
            }
        }
        else
        {
            EditorGUILayout.LabelField("文件夹路径", GUILayout.Width(100));
            scrExcelPath = EditorGUILayout.TextField(scrExcelPath);
            if (GUILayout.Button("浏览"))
            {
                scrExcelPath = EditorUtility.OpenFolderPanel("path", scrExcelPath, "");
                EditorPrefs.SetString("scrExcelPath", scrExcelPath);
            }
        }

        EditorGUILayout.EndHorizontal();



        if (GUILayout.Button("加载数据"))
        {
            if (!LoadExcel())
            {
                cfgs.Clear();
                Debug.Log("加载失败!");
            }
            else
                Debug.Log("加载完成!");
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("脚本输出路径", GUILayout.Width(100));
        outScriptPath = EditorGUILayout.TextField(outScriptPath);
        if (GUILayout.Button("浏览"))
        {
            outScriptPath = EditorUtility.OpenFolderPanel("path", Application.dataPath, "");
            EditorPrefs.SetString("outScriptPath", outScriptPath);
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("生成脚本"))
        {
            if (WriteCode(outScriptPath))
                Debug.Log("生成完成!");
            else
            {
                Debug.Log("生成失败!");
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("保存数据路径", GUILayout.Width(100));
        saveAssetPath = EditorGUILayout.TextField(saveAssetPath);
        if (GUILayout.Button("浏览"))
        {
            saveAssetPath = EditorUtility.OpenFolderPanel("path", Application.dataPath, "");
            EditorPrefs.SetString("saveAssetPath", saveAssetPath);
        }
        EditorGUILayout.EndHorizontal();

        if (isSingleFile)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("保存到:", GUILayout.Width(100));
            cc = EditorGUILayout.ObjectField(cc, typeof(ConfigsContainer)) as ConfigsContainer;
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("保存数据"))
        {
            if (isSingleFile && cc == null)
            {
                EditorUtility.DisplayDialog("警告", "请选择需要更新的目标对象", "好的");
                return;
            }
            if (SaveAll())
                Debug.Log("保存完成!");
            else
                Debug.Log("保存失败!");
        }
    }





    public class Cfg
    {
        public string Name;
        public List<string> fileNames = new List<string>();
        public List<string> types = new List<string>();
        public List<string> notates = new List<string>();


        public List<DataRow> items = new List<DataRow>();

        HashSet<object> idMap = new HashSet<object>();

        public bool CheckID()
        {
            idMap.Clear();
            var index = fileNames.FindIndex((s) => s.ToLower() == "id");
            if (index < 0) return false;
            foreach (var item in items)
            {
                if (item.ItemArray.Length != fileNames.Count) return false;
                if (idMap.Contains(item.ItemArray[index])) return false;
                else
                {
                    idMap.Add(item.ItemArray[index]);
                }
            }
            return true;
        }

    }
}
