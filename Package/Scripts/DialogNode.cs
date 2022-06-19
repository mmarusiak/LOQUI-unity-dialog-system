using System.Collections.Generic;
using UnityEngine;

public class DialogNode
{
    public enum NodeType
    {
        Passive,
        MultipleChoice,
    }
    
    public string Title, Text;
    public Rect NodeRect = new Rect (100, 100, 100, 100);
    public NodeType DialogNodeType;

    public DialogNode Root;
    public List<DialogNode> NextNodes;

    public DialogNode(string _title, string _text, NodeType _dialogNodeType)
    {
        Title = _title;
        Text = _text;
        DialogNodeType = _dialogNodeType;
    }
}
