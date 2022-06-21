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
    
    public List<DialogNode> NextNodes = new List<DialogNode>();

    public DialogNode(string _title, string _text, NodeType _dialogNodeType)
    {
        Title = _title;
        Text = _text;
        DialogNodeType = _dialogNodeType;
    }
}