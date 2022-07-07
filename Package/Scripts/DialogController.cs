using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    public List<DialogNode> DialogNodes = new List<DialogNode>(){};
    
    void Start()
    {
        foreach (DialogNode node in DialogNodes)
        {
            if (node.LinkedNodesChance.Count > 0)
            {
                int result = 0;

                foreach (int i in node.LinkedNodesChance)
                {
                    result += i;
                }

                if (result != 100)
                {
                    Debug.LogError($"[DIALOG ERROR] Please change chances for nodes connected to {node.Title} node in dialog editor! Sum of them needs to be equal 100.");
                    UnityEditor.EditorApplication.isPlaying = false;
                }
            }
        }    
    }
    
    public DialogNode FindNodeByWindowID(int windowID)
    {
        foreach (var dialogNode in DialogNodes)
        {
            if (dialogNode.WindowID == windowID) 
                return dialogNode;
        }
        return null;
    }

    public void CallMethod(string methodName, params object[] arguments)
    {
        SendMessage("", "");
        if (methodName != "")
        {
            if(arguments.Length > 0)
                SendMessage(methodName, arguments);
            else
                SendMessage(methodName);    
        }
    }
}
