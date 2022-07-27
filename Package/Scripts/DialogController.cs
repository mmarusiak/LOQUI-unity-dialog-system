using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    public List<DialogNode> DialogNodes = new List<DialogNode>(){};
    
    public float DialogActivationRange = 5f;
    [Tooltip("Determines how fast (in seconds) next letters will appear on screen (formula: 1/value). If set on zero or less text will appear without any \"letters effect\".")]
    public float TextDisplaySpeed;
    [HideInInspector]
    public DialogSystemInfo DialogSystemInfo;
    
    void Start()
    {
        DialogSystemInfo = GameObject.FindWithTag("GameController").GetComponent<DialogSystemInfo>();
        
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

    void Update()
    {
        if (DialogSystemInfo.Is3D)
        {
            bool isInRange =
                Physics.OverlapSphere(transform.position, DialogActivationRange, DialogSystemInfo.PlayerLayerMask).Length >
                0;
            if (isInRange && Input.GetKeyDown(DialogSystemInfo.DialogActionKey) && !DialogSystemInfo.InDialog)
            {
                DialogSystemInfo.InDialog = true;
                StartDialog();
            }
        }
        else
        {
            bool isInRange =
                Physics2D.OverlapCircle(transform.position, DialogActivationRange, DialogSystemInfo.PlayerLayerMask) != null;
            if (isInRange && Input.GetKeyDown(DialogSystemInfo.DialogActionKey) && !DialogSystemInfo.InDialog)
            {
                DialogSystemInfo.InDialog = true;
                StartDialog();
            }
        }
    }

    void StartDialog()
    {
        
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
