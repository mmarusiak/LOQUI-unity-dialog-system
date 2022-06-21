using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DialogWindow : EditorWindow
{
    private bool _creatingNode = false, _actorCreated = false, _inspectorShown = true, 
        _linkingMode = false, _destroyLinksMode = false;
    string _newNodeTitle = "", _newNodeText = "";
    private int _selectedNodeType = -1, _selectedActor = -1, _lastTouchedWindow = -1;
    private float _inspectorWidth = 140;
    private GUIStyle _centeredStyle = null;

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
        if (_centeredStyle == null)
        {
            _centeredStyle = GUI.skin.GetStyle("Label");
            _centeredStyle.alignment = TextAnchor.UpperCenter;
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
    }

    void DrawStaticGroup()
    {
        // Game object popup
        var allGameObjects = Object.FindObjectsOfType<GameObject>();
        _selectedActor = EditorGUI.Popup(new Rect(0, 0, 140, 20), _selectedActor,allGameObjects.Select(
            (_gameobject) => _gameobject.name).ToArray());

        if (_selectedActor >= 0 && _selectedActor < allGameObjects.Length)
        {
            dialogActor = allGameObjects[_selectedActor];
            _actorCreated = dialogActor.GetComponent<DialogController>() != null;
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
            position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 60), "ADD NODE"))
        {
            _creatingNode = !_creatingNode;

            if (_selectedNodeType >= 0 && _newNodeTitle != "" && _newNodeText != "")
            {
                dialogActor.GetComponent<DialogController>().DialogNodes.
                    Add(new DialogNode(_newNodeTitle, _newNodeText, (DialogNode.NodeType)_selectedNodeType));
            }

            _selectedNodeType = -1;
            _newNodeTitle = "";
            _newNodeText = "";
        }

        // Remove existing node
        if (GUI.Button(new Rect(position.width / 2  - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 
            20, position.width / 2  - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 60), "REMOVE NODE"))
        {
            dialogActor.GetComponent<DialogController>().DialogNodes.RemoveAt(_lastTouchedWindow);
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
                _selectedNodeType, new string[]
                {
                    DialogNode.NodeType.AINode.ToString(),
                    DialogNode.NodeType.PlayerNode.ToString()
                });
        }
        
        Handles.BeginGUI();
        List<DialogNode[]> linkednodes = LinkedNodes();
        foreach (var pair in linkednodes)
        {
            Handles.DrawBezier(pair[0].NodeRect.center, pair[1].NodeRect.center,
                new Vector2(pair[0].NodeRect.xMax + 50f, pair[0].NodeRect.center.y),
                new Vector2(pair[1].NodeRect.xMax + 50f, pair[1].NodeRect.center.y),
                _colorPallete[2], null, 5f);
        }
        Handles.EndGUI();
        
        BeginWindows();
        for (int i = 0; i < dialogActor.GetComponent<DialogController>().DialogNodes.Count; i++)
        {
            dialogActor.GetComponent<DialogController>().DialogNodes[i].WindowID = i;
            Color oldColor = GUI.color;
            GUI.color = _colorPallete[(int) dialogActor.GetComponent<DialogController>().DialogNodes[i].DialogNodeType];
            dialogActor.GetComponent<DialogController>().DialogNodes[i].NodeRect = 
                GUI.Window(i, dialogActor.GetComponent<DialogController>().DialogNodes[i].NodeRect,
                    WindowFunction, dialogActor.GetComponent<DialogController>().DialogNodes[i].Title);
            GUI.color = oldColor;
        }
        EndWindows();
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
        int yPos = 80;
        if (_creatingNode)
            yPos = 160;
        
        // Background for inspector
        Color oldColor = GUI.color;
        Color32 inspectorColor = new Color32(92, 92, 92, 255);
        GUIStyle inspectorStyle = new GUIStyle(GUI.skin.box);
        inspectorStyle.normal.background = MakeTex( 2, 2, inspectorColor);

        GUI.Box(new Rect(position.width - _inspectorWidth, 0, _inspectorWidth, position.height), "", inspectorStyle);
        GUI.color = oldColor;

        if (_lastTouchedWindow == -1 || dialogActor.GetComponent<DialogController>().DialogNodes.Count - 1 < _lastTouchedWindow)
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
            dialogActor.GetComponent<DialogController>().FindNodeByWindowID(_lastTouchedWindow).Title =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 80, _inspectorWidth/2, 20),
                    dialogActor.GetComponent<DialogController>().FindNodeByWindowID(_lastTouchedWindow).Title);
            
            // Text info
            GUI.Label(new Rect(position.width - _inspectorWidth, 110, _inspectorWidth/2, 20),
                "Node text");
            dialogActor.GetComponent<DialogController>().FindNodeByWindowID(_lastTouchedWindow).Text =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 110, _inspectorWidth/2, 20),
                    dialogActor.GetComponent<DialogController>().FindNodeByWindowID(_lastTouchedWindow).Text);
            
            // Node type info
            GUI.Label(new Rect(position.width - _inspectorWidth, 140, _inspectorWidth/2, 20),
                "Type of node");

            if (GUI.Button(new Rect(position.width - _inspectorWidth / 2, 140, _inspectorWidth / 2, 20),
                "Change to " + ((DialogNode.NodeType) ((int) dialogActor.GetComponent<DialogController>()
                    .FindNodeByWindowID(_lastTouchedWindow).DialogNodeType * -1 + 1))))
            {
                dialogActor.GetComponent<DialogController>().FindNodeByWindowID(_lastTouchedWindow).
                    DialogNodeType = (DialogNode.NodeType)((int)dialogActor.GetComponent<DialogController>().FindNodeByWindowID(_lastTouchedWindow).
                    DialogNodeType * -1 + 1);
            }

        }
    }
    
    void WindowFunction (int windowID)
    {
        Rect windowRect = dialogActor.GetComponent<DialogController>().
            FindNodeByWindowID(windowID).NodeRect;

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
            Debug.Log("Touch");
            
            DialogNode[] nodes = new []
            {
                new DialogNode("","",DialogNode.NodeType.PlayerNode),
                new DialogNode("","",DialogNode.NodeType.PlayerNode),
            };
            nodes[0].NodeRect = Rect.zero;
            nodes[0].WindowID = windowID;
            nodes[1].NodeRect = Rect.zero;
            nodes[1].WindowID = _lastTouchedWindow;

            if (_lastTouchedWindow == windowID)
            {
                    
            }
            
            _lastTouchedWindow = windowID;
        }
        GUI.DragWindow();
    }

    private List<DialogNode[]> LinkedNodes()
    {
        List<DialogNode[]> result = new List<DialogNode[]>();
        if (dialogActor.GetComponent<DialogController>().DialogNodes.Count > 0)
        {
            foreach (var node in dialogActor.GetComponent<DialogController>().DialogNodes)
            {
                if (node.NextNodes.Count > 0)
                {
                    foreach (var nextnode in node.NextNodes)
                    {
                        if (!result.Contains(new[]
                        {
                            dialogActor.GetComponent<DialogController>().FindNodeByWindowID(nextnode.WindowID),
                            node   
                        }) && !result.Contains(new[]
                        {
                            node,
                            dialogActor.GetComponent<DialogController>().FindNodeByWindowID(nextnode.WindowID)
                        }))
                        {
                            result.Add(new[] {node, dialogActor.GetComponent<DialogController>().FindNodeByWindowID(nextnode.WindowID)});
                        }
                    }
                }
            }
        }

        return result;
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
