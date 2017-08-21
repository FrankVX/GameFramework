using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConverterAttribute : Attribute
{
    public string type;
    public ConverterAttribute(string type)
    {
        this.type = type;
    }
}

public abstract class ExcelDataConverter
{
    public abstract object Convert(string value);
}

[Converter("int")]
public class IntConverter : ExcelDataConverter
{
    public override object Convert(string value)
    {
        int a = 0;
        int.TryParse(value, out a);
        return a;
    }
}

[Converter("string")]
public class StringConverter : ExcelDataConverter
{
    public override object Convert(string value)
    {
        return value;
    }
}

[Converter("float")]
public class FloatConverter : ExcelDataConverter
{
    public override object Convert(string value)
    {
        float a = 0;
        float.TryParse(value, out a);
        return a;
    }
}

[Converter("long")]
public class LongConverter : ExcelDataConverter
{
    public override object Convert(string value)
    {
        long a = 0;
        long.TryParse(value, out a);
        return a;
    }
}

[Converter("byte")]
public class ByteConverter : ExcelDataConverter
{
    public override object Convert(string value)
    {
        byte a = 0;
        byte.TryParse(value, out a);
        return a;
    }
}
