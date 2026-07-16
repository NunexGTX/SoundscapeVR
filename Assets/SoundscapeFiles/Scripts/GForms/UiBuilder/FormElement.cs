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
    public const string IMAGE = "IMAGE";
    public const string GRID_RADIO = "GRID_RADIO";
    public const string GRID_CHECKBOX = "GRID_CHECKBOX";
    public string id;
    public string type;
    public string description;
    public bool required = false;
    public List<string> extra;
    public bool shuffle = false;
    public override string ToString()
    {
        string extraElements = (extra != null) ? string.Join(", ", extra) : "null";
        
        return $"[FormElement] ID: {id} | Type: {type} | Required: {required} | Shuffle: {shuffle}\n" +
               $"Description: \"{description}\"\n" +
               $"Extra: [{extraElements}]";
    }
}