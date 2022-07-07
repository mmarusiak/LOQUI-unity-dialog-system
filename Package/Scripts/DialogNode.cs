using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogNode
{
    public enum NodeType
    {
        AINode,
        PlayerNode,
    }
    
    public string Title, Text;
    public int WindowID;
    public Rect NodeRect = new Rect (100, 100, 100, 100);
    public NodeType DialogNodeType;
    public AudioClip DialogTextAudio;
    public string MethodName = "";
    public List<object> MethodArguments = new List<object>();

    public List<int> LinkedNodesChance = new List<int>();

    public List<int> LinkedIds = new List<int>();

    public DialogNode(string _title, string _text, NodeType _dialogNodeType)
    {
        Title = _title;
        Text = _text;
        DialogNodeType = _dialogNodeType;
    }
}