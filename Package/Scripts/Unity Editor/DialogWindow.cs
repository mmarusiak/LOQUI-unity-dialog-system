using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DialogWindow : EditorWindow
{
    private bool _creatingNode = false, _actorCreated = false, _inspectorShown = true, 
        _linkingMode = false, _destroyLinksMode = false, _customDropDownShown;
    private string _newNodeTitle = "", _newNodeText = "";
    private int _selectedNodeType = -1, _selectedActor = -1, _lastTouchedWindow = -1;
    private float _inspectorWidth = 140;
    private GUIStyle _centeredStyle = null;
    
    private DialogController _dialogController;
    private DialogSystemInfo _dialogSystemInfo;
    public GameObject dialogActor;

    private Color32[] _colorPallete = new[]
    {
        new Color32(38, 70, 83, 255),
        new Color32(231, 111, 81, 255),
        new Color32(233, 196, 106, 255)
    };

    [MenuItem("Window/Dialog system window")] 
    public static void Init()
    {
        DialogWindow window = GetWindow<DialogWindow>("Dialog editor");
        window.Show();
    }

    private void OnGUI()
    {
        if (_dialogSystemInfo == null)
        {
            try
            {
                _dialogSystemInfo = GameObject.FindWithTag("GameController").GetComponent<DialogSystemInfo>();
            }
            catch
            {
                Debug.LogWarning("Please add dialog system info script to game object with tag GameController");
            }

        }
        
        if (_centeredStyle == null)
        {
            _centeredStyle = GUI.skin.label;
            _centeredStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        _inspectorWidth = position.width / 4;
        DrawStaticGroup();
        
        if (_actorCreated)
        {
            DrawMainPanel();
            
            if (_inspectorShown)
            {
                DrawInspector();
            }
        }
        else
        {
            DrawActorPanel();
        }

        if (_customDropDownShown)
        {
            ShowDropDown(_lastTouchedWindow);
        }
    }

    void DrawStaticGroup()
    {
        // Game object popup
        var allGameObjects = Object.FindObjectsOfType<GameObject>();
        _selectedActor = EditorGUI.Popup(new Rect(0, 0, 140, 20), _selectedActor,allGameObjects.Select(
            (gameobject) => gameobject.name).ToArray());

        if (_selectedActor >= 0 && _selectedActor < allGameObjects.Length)
        {
            if (dialogActor != allGameObjects[_selectedActor])
            {
                dialogActor = allGameObjects[_selectedActor];
                AssignDialogController();
            }
        }
        
        // Show inspector button
        float xPos = position.width;
        
        string showButtonText = "<";
        if (_inspectorShown)
        {
            showButtonText = ">";
            xPos -= _inspectorWidth;
        }
        
        if(_actorCreated)
            if (GUI.Button(new Rect(xPos - 20, 0, 20, 20), showButtonText))
            {
                _inspectorShown = !_inspectorShown;
            }
    }
    
    void DrawMainPanel()
    {
        // Create new node
        if (GUI.Button(new Rect(0, 20, 
            position.width - _inspectorWidth * Convert.ToInt32(_inspectorShown), 60), "ADD NODE"))
        {
            _creatingNode = !_creatingNode;

            if (_selectedNodeType >= 0 && _newNodeTitle != "" && _newNodeText != "")
            {
                CreateNewNode(_selectedNodeType, _newNodeTitle, _newNodeText);
            }

            _selectedNodeType = -1;
            _newNodeTitle = "";
            _newNodeText = "";
        }
        
        if (_creatingNode)
        {
            // Title "box"
            GUI.Label(new Rect(0, 80, position.width, 40), 
                "Enter dialog node title", EditorStyles.boldLabel);
            _newNodeTitle = GUI.TextField(
                new Rect(
                    position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 80, 
                    position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 40),
                _newNodeTitle, 10);
            
            // Text "box"
            GUI.Label(new Rect(0, 120, position.width, 40), 
                "Enter dialog node text", EditorStyles.boldLabel);
            _newNodeText = GUI.TextField(
                new Rect(
                    position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 120,
                position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 40),
                _newNodeText, 40);
            
            // Dialog type popup
            GUI.Label(new Rect(0, 160, position.width, 40), 
                "Select node type", EditorStyles.boldLabel);
            _selectedNodeType = EditorGUI.Popup(new Rect(3*position.width/8, 170, position.width/4, 20),
                _selectedNodeType, new[]
                {
                    DialogNode.NodeType.AINode.ToString(),
                    DialogNode.NodeType.PlayerNode.ToString()
                });
        }
        
        BeginWindows();
        for (int i = 0; i < _dialogController.DialogNodes.Count; i++)
        {
            _dialogController.DialogNodes[i].WindowID = i;
            Color oldColor = GUI.color;
            GUI.color = _colorPallete[(int) _dialogController.DialogNodes[i].DialogNodeType];
            _dialogController.DialogNodes[i].NodeRect = 
                GUI.Window(i, _dialogController.DialogNodes[i].NodeRect,
                    WindowFunction, "");
            GUI.color = oldColor;
        }
        EndWindows();
        
        Handles.BeginGUI();
        List<DialogNode[]> linkednodes = LinkedNodes();
        foreach (var pair in linkednodes)
        { 
            Handles.DrawBezier(
                new Vector2(pair[0].NodeRect.x, pair[0].NodeRect.center.y), 
                new Vector2(pair[1].NodeRect.x + pair[1].NodeRect.width, pair[1].NodeRect.center.y),
                 new Vector2(pair[0].NodeRect.xMax + 50f, pair[0].NodeRect.center.y),
                 new Vector2(pair[1].NodeRect.xMax + 50f, pair[1].NodeRect.center.y),
                 _colorPallete[2], null, 6f);
            
            GUI.DrawTexture(
                new Rect
                (
                    pair[1].NodeRect.x + pair[1].NodeRect.width - _dialogSystemInfo.ArrowSize, 
                    pair[1].NodeRect.center.y - _dialogSystemInfo.ArrowSize / 2, 
                    _dialogSystemInfo.ArrowSize,
                    _dialogSystemInfo.ArrowSize), _dialogSystemInfo.ArrowTexture, ScaleMode.ScaleToFit);
        }
        Handles.EndGUI();
    }

    void DrawActorPanel()
    {
        GUI.Label(new Rect(position.width/4, 40, position.width/2, 40), 
            "Selected Game Object is not Dialog Actor!", _centeredStyle);
        if(GUI.Button(new Rect(position.width / 4, 80, position.width / 2, 40),
            "Create new dialog actor"))
        {
            dialogActor.AddComponent<DialogController>();
            _actorCreated = true;
        }
    }

    void DrawInspector()
    {
        // Background for inspector
        Color oldColor = GUI.color;
        Color32 inspectorColor = new Color32(92, 92, 92, 255);
        GUIStyle inspectorStyle = new GUIStyle(GUI.skin.box);
        inspectorStyle.normal.background = MakeTex( 2, 2, inspectorColor);

        GUI.Box(new Rect(position.width - _inspectorWidth, 0, _inspectorWidth, position.height), "", inspectorStyle);
        GUI.color = oldColor;

        if (_lastTouchedWindow == -1 || _dialogController.DialogNodes.Count - 1 < _lastTouchedWindow)
        {
            GUI.Label(new Rect(position.width - _inspectorWidth, 20, _inspectorWidth, 20), 
                "Please select node to enter inspector!");
        }
        else
        {
            // Game object name
            GUIStyle goNameStyle = _centeredStyle;
            goNameStyle.fontStyle = FontStyle.Bold;
            GUI.Label(new Rect(position.width - _inspectorWidth, 40, _inspectorWidth, 40), 
               dialogActor.name, goNameStyle);
            
            // Title info
            GUI.Label(new Rect(position.width - _inspectorWidth, 80, _inspectorWidth/2, 20),
                "Node title");
            _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 80, _inspectorWidth/2, 20),
                    _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title);
            
            // Text info
            GUI.Label(new Rect(position.width - _inspectorWidth, 110, _inspectorWidth/2, 20),
                "Node text");
           _dialogController.FindNodeByWindowID(_lastTouchedWindow).Text =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 110, _inspectorWidth/2, 20),
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).Text);
            
            // Node type info
            GUI.Label(new Rect(position.width - _inspectorWidth, 140, _inspectorWidth/2, 20),
                "Type of node");

            if (GUI.Button(new Rect(position.width - _inspectorWidth / 2, 140, _inspectorWidth / 2, 20),
                "Change to " + ((DialogNode.NodeType) ((int) _dialogController
                    .FindNodeByWindowID(_lastTouchedWindow).DialogNodeType * -1 + 1))))
            {
                _dialogController.FindNodeByWindowID(_lastTouchedWindow).
                    DialogNodeType = (DialogNode.NodeType)((int)_dialogController.FindNodeByWindowID(_lastTouchedWindow).
                    DialogNodeType * -1 + 1);
            }

        }
    }
    
    void WindowFunction (int windowID)
    {
        // TO CENTER - centered style doesnt work
        GUI.Label(
            new Rect(0,-30, 100, 100),
            _dialogController.FindNodeByWindowID(windowID).Title, _centeredStyle);
        
        Rect windowRect = _dialogController.FindNodeByWindowID(windowID).NodeRect;

        Rect relativeRect = GUIUtility.GUIToScreenRect(new Rect(
            0, 
            0,
            windowRect.width,
            windowRect.height
            ));
        
        Event e = Event.current;

        if(e.type == EventType.MouseDown && e.button == 0 && 
           relativeRect.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)))
        {
            _customDropDownShown = false;
            if (windowID != _lastTouchedWindow)
            {

                if (_linkingMode)
                {
                    _linkingMode = false;
                    if (!_dialogController.FindNodeByWindowID(windowID).LinkedIds.Contains(_lastTouchedWindow)
                        && !_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Contains(windowID)
                        && _dialogController.FindNodeByWindowID(windowID).DialogNodeType 
                        != _dialogController.FindNodeByWindowID(_lastTouchedWindow).DialogNodeType)
                    {
                        _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Add(windowID);
                    }
                }

                if (_destroyLinksMode)
                {
                    _destroyLinksMode = false;
                    if (_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Contains(windowID))
                    {
                        _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Remove(windowID);
                    }
                }
            }
            
            _lastTouchedWindow = windowID;
        }
        if (e.type == EventType.MouseDown && e.button == 1 &&
            relativeRect.Contains(GUIUtility.GUIToScreenPoint(e.mousePosition)))
        {
            _customDropDownShown = true;
            _lastTouchedWindow = windowID;
        }
        
        if(!_customDropDownShown || windowID != _lastTouchedWindow)
            GUI.DragWindow();
    }

    void ShowDropDown(int windowID)
    {
        Rect windowRect = _dialogController.FindNodeByWindowID(windowID).NodeRect;
        Rect firstButtonPos = new Rect(windowRect.x + windowRect.width, windowRect.y, 200, 50);
        
        if (GUI.Button(new Rect(firstButtonPos.x, firstButtonPos.y - firstButtonPos.height,
            firstButtonPos.width, firstButtonPos.height), "Destroy node"))
        {
            _destroyLinksMode = false;
            _linkingMode = false;
            _customDropDownShown = false;

            foreach (var node in _dialogController.DialogNodes)
            {
                foreach (var linkedId in node.LinkedIds)
                {
                    if (linkedId == windowID)
                    {
                        node.LinkedIds.Remove(windowID);
                        break;
                    }
                }
            }
            
            _dialogController.DialogNodes.Remove(_dialogController.FindNodeByWindowID(windowID));
        }
        
        if (GUI.Button(firstButtonPos, "Make new link"))
        {
            _linkingMode = true;
            _destroyLinksMode = false;
            _customDropDownShown = false;
        }
        if (GUI.Button(new Rect(firstButtonPos.x, firstButtonPos.y + firstButtonPos.height,
                firstButtonPos.width, firstButtonPos.height), "Destroy old link"))
        {
            _destroyLinksMode = true;
            _linkingMode = false;
            _customDropDownShown = false;
        }

        if (GUI.Button(new Rect(firstButtonPos.x, firstButtonPos.y + 2 * firstButtonPos.height,
            firstButtonPos.width, firstButtonPos.height), "Back"))
        {
            _customDropDownShown = false;
        }
    }

    void CreateNewNode(int __selectedNodeType, string __newNodeTitle, string __newNodeText)
    {
        _dialogController.DialogNodes.Add(new DialogNode(__newNodeTitle, __newNodeText, (DialogNode.NodeType)__selectedNodeType));
    }
    
    private List<DialogNode[]> LinkedNodes()
    {
        List<DialogNode[]> result = new List<DialogNode[]>();

        foreach (var node in _dialogController.DialogNodes)
        {
            foreach (var nextID in node.LinkedIds)
            {
                result.Add(new []{node, _dialogController.FindNodeByWindowID(nextID)});
            }
        }
        
        
        return result;
    }

    void AssignDialogController()
    {
        _dialogController = dialogActor.GetComponent<DialogController>();
        _actorCreated = true;
        _creatingNode = false;
        
        if (_dialogController == null)
        {
            _actorCreated = false;
        }
    }
    private Texture2D MakeTex( int width, int height, Color col )
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        Texture2D result = new Texture2D( width, height );
        result.SetPixels( pix );
        result.Apply();
        return result;
    }

}
