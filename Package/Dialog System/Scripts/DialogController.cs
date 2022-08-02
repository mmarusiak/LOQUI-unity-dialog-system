using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogController : MonoBehaviour
{
    public List<DialogNode> DialogNodes = new List<DialogNode>(){};
    private List<NodeConnection> _connections = new List<NodeConnection>();
    private int currentNodeID;

    [Tooltip("Provide actor's name that will be displayed on dialog (only if show speaker's name option is set to true)")]
    public string ActorName = "Bobby bob";
    public float DialogActivationRange = 5f;
    [Tooltip("Determines how fast (in seconds) next letters will appear on screen (formula: 1/value). If set on zero or less text will appear without any \"letters effect\".")]
    public float TextDisplaySpeed;
    [Tooltip("If you want to \"talk\" with AI multiple times (the same dialog that you set) you should set it to true")]
    public bool MultipleInteractions = true;
    
    [HideInInspector]
    public DialogSystemInfo DialogSystemInfo;

    private GameObject mainUIParent;
    private GameObject choiceUIParent;
    private bool textShown = false;
    private bool inThisDialog = false;
    private bool interactable = true;

    
    // if isConnected is false and isLinked is true, then it's the start node
    // also easy to catch end node with that class - isConnected true and isLinked false - but there can exist many end points in dialog
    public class NodeConnection
    {
        // is some node connected/linked TO THIS NODE?
        public bool isConnected;
        // is THIS NODE connected/linked to some node?
        public bool isLinked;
        public int windowID;
        public NodeConnection(bool _isConnected, bool _isLinked, int _windowID)
        {
            isConnected = _isConnected;
            isLinked = _isLinked;
            windowID = _windowID;
        }
    }
    void Start()
    {
        DialogSystemInfo = GameObject.FindWithTag("GameController").GetComponent<DialogSystemInfo>();
     
        // parent of Game Objects (UI) that doesn't "belong" to choice system
        mainUIParent = GameObject.Find("MainDialog");
        // parent of Game Objects (UI) that "belongs" to choice system
        choiceUIParent = GameObject.Find("PlayerChoiceButtons");

        // get windowID of first node in dialog
        try
        {
            foreach (var node in DialogNodes)
            {
                bool isConnected = false;
                bool isLinked = node.LinkedIds.Count > 0;
                foreach (var parentNode in DialogNodes)
                {
                    if (parentNode.LinkedIds.Contains(node.WindowID))
                    {
                        isConnected = true;
                        break;
                    }
                }
                _connections.Add(new NodeConnection(isConnected, isLinked, node.WindowID));
            }

            foreach (var connection in _connections)
            {
                if (!connection.isConnected && connection.isLinked)
                {
                    currentNodeID = connection.windowID;
                }
            }
        }
        catch
        {
            currentNodeID = 0;
            Debug.LogWarning("[DIALOG WARNING] Dialog on actor not created");
            interactable = false;
            inThisDialog = false;
        }

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
        if (interactable)
        {
            if (DialogSystemInfo.Is3D)
            {
                bool isInRange =
                    Physics.OverlapSphere(transform.position, DialogActivationRange, DialogSystemInfo.PlayerLayerMask)
                        .Length >
                    0;
                if (isInRange && Input.GetKeyDown(DialogSystemInfo.DialogActionKey) && !DialogSystemInfo.InDialog)
                {
                    Debug.Log("ADSAD");
                    DialogSystemInfo.InDialog = true;
                    StartDialog();
                    inThisDialog = true;
                }
            }
            else
            {
                bool isInRange =
                    Physics2D.OverlapCircle(transform.position, DialogActivationRange,
                        DialogSystemInfo.PlayerLayerMask) != null;
                if (isInRange && Input.GetKeyDown(DialogSystemInfo.DialogActionKey) && !DialogSystemInfo.InDialog)
                {
                    Debug.Log("aAA");
                    DialogSystemInfo.InDialog = true;
                    StartDialog();
                    inThisDialog = true;
                }
            }
        }

        // input only works when choice buttons are not shown
        if (Input.GetKeyDown(DialogSystemInfo.DialogActionKey) && inThisDialog &&
            (FindNodeByWindowID(currentNodeID).LinkedIds.Count <= 1 || 
             FindNodeByWindowID(currentNodeID).DialogNodeType != DialogNode.NodeType.AINode))
        {
            
            // skip text display effect
            if (DialogSystemInfo.IsTextDisplayEffectSkippable && !textShown)
            {
                textShown = true;
            }
            
            else
            {

                // one ai answer, not random, just single
                if (textShown &&
                    FindNodeByWindowID(currentNodeID).LinkedIds.Count == 1)
                {
                    currentNodeID = FindNodeByWindowID(currentNodeID).LinkedIds[0];
                    NextNode();
                }

                // random ai answer if more than one
                else if (textShown &&
                         FindNodeByWindowID(currentNodeID).DialogNodeType != DialogNode.NodeType.AINode &&
                          FindNodeByWindowID(currentNodeID).LinkedIds.Count > 1)
                {
                    float random = Random.Range(0, 100);
                    float percents = 0;

                    for (int i = 0; i < FindNodeByWindowID(currentNodeID).LinkedNodesChance.Count; i++)
                    {
                        if (i == 0 && random == 0)
                        {
                            currentNodeID = FindNodeByWindowID(currentNodeID).LinkedIds[i];
                            NextNode();
                            return;
                        }

                        if (i > percents && i <= FindNodeByWindowID(currentNodeID).LinkedNodesChance[i])
                        {
                            currentNodeID = FindNodeByWindowID(currentNodeID).LinkedIds[i];
                            NextNode();
                            return;
                        }

                        percents += FindNodeByWindowID(currentNodeID).LinkedNodesChance[i];
                    }
                }
            }
            
            // if no more linked ids, then end the dialog
            if (FindNodeByWindowID(currentNodeID).LinkedIds.Count < 1)
            {
                if (!MultipleInteractions)
                    interactable = false;
                mainUIParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(1000,1000);
                choiceUIParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(2000,1000);
                DialogSystemInfo.InDialog = false;
                inThisDialog = false;
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
                mainUIParent.GetComponent<RectTransform>().Find("DialogLineBackground").GetComponent<Image>().sprite = DialogSystemInfo.DialogLineBackground;
            }
            
            if (DialogSystemInfo.TextFont != null)
            {
                // set correct font
                mainUIParent.GetComponent<RectTransform>().Find("DialogLineText").GetComponent<Text>().font = DialogSystemInfo.TextFont;
                mainUIParent.GetComponent<RectTransform>().Find("ActorName").GetComponent<Text>().font = DialogSystemInfo.TextFont;
            }
            DialogSystemInfo.FirstRun = false;
        }
        
        mainUIParent.GetComponent<RectTransform>().anchoredPosition = Vector3.zero; 
        NextNode();
    }
    
    void NextNode()
    {
        // if display effect is set, then play it
        if (TextDisplaySpeed > 0)
        {
            mainUIParent.GetComponent<RectTransform>().Find("DialogLineText").GetComponent<Text>().text = "";
            IEnumerator coroutine = DisplayText(1/TextDisplaySpeed);
            textShown = false;
            StartCoroutine(coroutine);
        }
        
        // just show who is speaking, show player's name (set in inspector DialogSystemInfo) or AI' name (DialogController)
        if (DialogSystemInfo.ShowWhoIsSpeaking)
        {
            // set text to name of speaker
            mainUIParent.GetComponent<RectTransform>().Find("ActorName").GetComponent<Text>().text = 
                FindNodeByWindowID(currentNodeID).DialogNodeType == DialogNode.NodeType.PlayerNode ? DialogSystemInfo.PlayerName : ActorName;
        }
        // clear text if option not checked
        else
        {
            mainUIParent.GetComponent<RectTransform>().Find("ActorName").GetComponent<Text>().text = "";
        }
        
        // set choice options buttons
        if (FindNodeByWindowID(currentNodeID).DialogNodeType == DialogNode.NodeType.AINode &&
            FindNodeByWindowID(currentNodeID).LinkedIds.Count > 1)
        {
            choiceUIParent.GetComponent<RectTransform>().position = Vector3.zero;
            // find correct buttons holder - buttons holders exist in combinations that are different
            // by number of buttons and are named just by number of buttons
            // (they are empty Game Objects)
            GameObject buttonsHolder =
                choiceUIParent.GetComponent<RectTransform>().Find(FindNodeByWindowID(currentNodeID).LinkedIds.Count.ToString()).gameObject;
            // creating button for each of linked node with correct appearance,
            // function and function arguments
            for (int i = 0; i < FindNodeByWindowID(currentNodeID).LinkedIds.Count; i++)
            {
                var newButton = DefaultControls.CreateButton(new DefaultControls.Resources());
                newButton.transform.SetParent(GameObject.Find("Canvas").transform, false);
                newButton.transform.position = buttonsHolder.transform.Find(i.ToString()).position;
                
                if (DialogSystemInfo.ButtonBackground != null)
                    newButton.GetComponent<Image>().sprite = DialogSystemInfo.ButtonBackground;

                newButton.transform.GetChild(0).GetComponent<Text>().text =
                    FindNodeByWindowID(FindNodeByWindowID(currentNodeID).LinkedIds[i]).Title;

                newButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    MakeChoice(FindNodeByWindowID(currentNodeID).LinkedIds[i]);
                });
            }
        }
    }

    // Display text effect, works only if text isn't shown, text is shown if it finished, or if action key is pressed and skip display effect
    private IEnumerator DisplayText(float waitTime)
    {
        foreach (char letter in FindNodeByWindowID(currentNodeID).Text) 
        {
            if (!textShown)
            {
                mainUIParent.transform.Find("DialogLineText").GetComponent<Text>().text += letter;
                yield return new WaitForSeconds(waitTime);
            }
        }

        textShown = true;
    }

    // choice button on click function
    void MakeChoice(int buttonID)
    {
        // TO DO: destroy current set of buttons
        choiceUIParent.transform.position = new Vector2(2000, 1000);
        CallMethod(FindNodeByWindowID(currentNodeID).MethodName, FindNodeByWindowID(currentNodeID).MethodArguments);
        currentNodeID = buttonID;
        NextNode();
    }
    
    public DialogNode FindNodeByWindowID(int windowID)
    {
        return DialogNodes.FirstOrDefault(dialogNode => dialogNode.WindowID == windowID);
    }

    public void CallMethod(string methodName, params object[] arguments)
    {
        if (methodName == "") return;
        if(arguments.Length > 0)
            SendMessage(methodName, arguments);
        else
            SendMessage(methodName);
    }
}
