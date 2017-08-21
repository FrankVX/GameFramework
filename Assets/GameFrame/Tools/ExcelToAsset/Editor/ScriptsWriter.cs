using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ScriptsWriter
{
    public enum MemberType
    {
        Filed,
        Property,
    }
    StringBuilder sb;
    public ScriptsWriter()
    {
        sb = new StringBuilder();
    }
    public ScriptsWriter(StringBuilder sb)
    {
        this.sb = sb;
    }


    public void Head()
    {
        sb.AppendLine("//注意!此类为工具生成,所有的改动都会在下次生成后消失!");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine();
    }

    public void Class(string className, string parent = null)
    {
        if (string.IsNullOrEmpty(parent))
            sb.AppendLine(string.Format("public class {0}", className));
        else sb.AppendLine(string.Format("public class {0} : {1}", className, parent));
        sb.AppendLine("{");
    }

    public void Member(string type, string name, string note)
    {
        if (!string.IsNullOrEmpty(note))
        {
            sb.AppendLine(string.Format("\t/// <summary> {0} </summary>", note));
        }
        sb.AppendLine(string.Format("\tpublic {0} {1} {{ get {{ return m_{1}; }} }}", type, name));
        sb.AppendLine("\t[SerializeField]");
        sb.AppendLine(string.Format("\tprivate {0} m_{1};", type, name));
        sb.AppendLine();
    }

    public void End()
    {
        sb.AppendLine("}\n\n");
    }

}
