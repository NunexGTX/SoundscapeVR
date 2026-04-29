using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FormElement
{
    public const string SECTION_BREAK = "BREAK";
    public const string HEADER = "TITLE";
    public const string RADIO = "RADIO";
    public const string CHECKBOX= "CHECKBOX";
    public const string DROP_DOWN = "DROP_DOWN";
    public const string SCALE = "SCALE";
    public const string PARAGRAPH = "PARAGRAPH";
    public const string TEXT = "TEXT";
    public string id;
    public string type;
    public string description;
    public bool required = false;
    public List<string> extra;
}