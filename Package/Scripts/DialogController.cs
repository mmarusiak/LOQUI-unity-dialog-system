using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    public List<DialogNode> DialogNodes = new List<DialogNode>(){};


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
