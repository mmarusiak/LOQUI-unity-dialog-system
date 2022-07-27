using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    public List<DialogNode> DialogNodes = new List<DialogNode>(){};
    private int currentNodeID;

    [Tooltip("Provide actor's name that will be displayed on dialog (only if show speaker's name option is set to true)")]
    public string ActorName = "Bobby bob";
    public float DialogActivationRange = 5f;
    [Tooltip("Determines how fast (in seconds) next letters will appear on screen (formula: 1/value). If set on zero or less text will appear without any \"letters effect\".")]
    public float TextDisplaySpeed;
    [HideInInspector]
    public DialogSystemInfo DialogSystemInfo;

    private GameObject mainUIParent;
    private GameObject choiceUIParent;
    
    void Start()
    {
        DialogSystemInfo = GameObject.FindWithTag("GameController").GetComponent<DialogSystemInfo>();
     
        mainUIParent = GameObject.Find("MainDialog");
        choiceUIParent = GameObject.Find("PlayerChoiceButtons");

        currentNodeID = DialogNodes[0].WindowID;
        
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
        // set appearance of the dialog UI to what the user set
        if (DialogSystemInfo.FirstRun)
        {
            if (DialogSystemInfo.DialogLineBackground != null)
            {
                // set correct image
                mainUIParent.transform.Find("DialogLineBackground").GetComponent<Image>().sprite = DialogSystemInfo.DialogLineBackground;
            }
            
            if (DialogSystemInfo.TextFont != null)
            {
                // set correct font
                mainUIParent.transform.Find("DialogLineText").GetComponent<Text>().font = DialogSystemInfo.TextFont;
                mainUIParent.transform.Find("ActorName").GetComponent<Text>().font = DialogSystemInfo.TextFont;
            }

            if (DialogSystemInfo.ShowWhoIsSpeaking)
            {
                // set text to name of speaker
                if (FindNodeByWindowID(currentNodeID).DialogNodeType == DialogNode.NodeType.PlayerNode)
                    mainUIParent.transform.Find("ActorName").GetComponent<Text>().text = DialogSystemInfo.PlayerName;
                else
                    mainUIParent.transform.Find("ActorName").GetComponent<Text>().text = ActorName;
            }
            else
            {
                mainUIParent.transform.Find("ActorName").GetComponent<Text>().text = "";
            }

            DialogSystemInfo.FirstRun = false;
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
