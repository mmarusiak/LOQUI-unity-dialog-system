using System.Collections.Generic;

public class DialogNode
{
    public enum nodeType
    {
        PassiveNode, // AI dialog
        ResponseNode // Player dialog - choice
    }
    
    
    public List<DialogNode> NextNode = new List<DialogNode>();
    public DialogNode PreviousChosenNode;
    public nodeType NodeType;
    public string Title, Text;

    public DialogNode(nodeType _nodeType, string title, string text)
    {
        NodeType = _nodeType;
        Title = title;
        Text = text;
    }
}
