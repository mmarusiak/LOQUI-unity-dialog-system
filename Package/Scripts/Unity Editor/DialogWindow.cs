using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

public class DialogWindow : EditorWindow
{
    private bool _creatingNode = false,
        _actorCreated = false,
        _inspectorShown = true,
        _linkingMode = false,
        _destroyLinksMode = false,
        _customDropDownShown = false;
    private string _newNodeTitle = "", _newNodeText = "", _audioClipName = "";
    private int _selectedNodeType = -1, _selectedActor = -1, _selectedArgumentType = -1, _boolArgument = -1, 
        _lastTouchedWindow = -1;
    private float _inspectorWidth = 140;
    private object _argument = "";

    private readonly string[] _argumentTypes =
    {
        "String",
        "Int",
        "Double",
        "Float",
        "Bool"
    };
    private Rect[] _navigationRect;

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
        var allGameObjects = FindObjectsOfType<GameObject>();
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
            _dialogController.DialogNodes[i].NodeRect = GUI.Window(i, _dialogController.DialogNodes[i].NodeRect,
                    WindowFunction, "");
            GUI.color = oldColor;
        }
        EndWindows();
        
        Handles.BeginGUI();
        List<DialogNode[]> linkednodes = LinkedNodes();
        foreach (var pair in linkednodes)
        {
            Vector3 distance = new Vector3(pair[0].NodeRect.x - pair[1].NodeRect.x,
                pair[0].NodeRect.y - pair[0].NodeRect.height - pair[1].NodeRect.y);
            Vector3 middlePoint =
                new Vector3(pair[0].NodeRect.center.x - distance.x / 2, pair[0].NodeRect.y - distance.y / 2);
            Vector3 startPoint = new Vector3(pair[0].NodeRect.center.x, pair[0].NodeRect.y + pair[0].NodeRect.height);
            Vector3 endPoint = new Vector3(pair[1].NodeRect.center.x, pair[1].NodeRect.y);
            
            Vector3[] tangentPoint = 
            {
                new Vector3(pair[0].NodeRect.center.x, middlePoint.y),
                new Vector3(pair[1].NodeRect.center.x, middlePoint.y)
            };

            if (pair[0].NodeRect.y + pair[0].NodeRect.height - pair[1].NodeRect.y < 0)
            {
                Handles.DrawBezier(startPoint, middlePoint, tangentPoint[0], tangentPoint[0], _colorPallete[2], null, 6f);
                Handles.DrawBezier(middlePoint, endPoint, tangentPoint[1], tangentPoint[1], _colorPallete[2], null, 6f);
            }
            
            GUI.DrawTexture(
                new Rect
                (
                    pair[1].NodeRect.center.x - _dialogSystemInfo.ArrowSize/2, 
                    pair[1].NodeRect.y - 2, 
                    _dialogSystemInfo.ArrowSize,
                    _dialogSystemInfo.ArrowSize), _dialogSystemInfo.ArrowTexture, ScaleMode.ScaleToFit);
        }
        Handles.EndGUI();
    }

    void DrawActorPanel()
    {
        GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.fontStyle = FontStyle.Bold;
        infoStyle.fontSize = 22;
        GUI.Label(new Rect(position.width/4, 40, position.width/2, 40), 
            "Selected Game Object is not Dialog Actor!", infoStyle);
        if(GUI.Button(new Rect(position.width / 4, 80, position.width / 2, 40),
            "Create new dialog actor"))
        {
            _dialogController = dialogActor.AddComponent<DialogController>();
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

        GUIStyle paragraph = new GUIStyle(GUI.skin.label);
        paragraph.fontStyle = FontStyle.Italic;
        paragraph.fontSize = 10;
        paragraph.wordWrap = true;
        

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
            GUIStyle goNameStyle = new GUIStyle(GUI.skin.label);
            goNameStyle.alignment = TextAnchor.MiddleCenter;
            goNameStyle.fontStyle = FontStyle.Bold;
            goNameStyle.normal.textColor = Color.white;
            goNameStyle.fontSize = 20;
            
            GUI.Label(new Rect(position.width - _inspectorWidth + 10, 40, _inspectorWidth, 40), 
               dialogActor.name, goNameStyle);

            // Title info
            GUI.Label(new Rect(position.width - _inspectorWidth + 10, 80, _inspectorWidth/2 - 20, 20),
                "Node title");
            _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 80, _inspectorWidth/2 - 10, 20),
                    _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title);
            
            // Text info
            GUI.Label(new Rect(position.width - _inspectorWidth + 10, 140, _inspectorWidth/2 - 20, 20),
                "Node text");
           _dialogController.FindNodeByWindowID(_lastTouchedWindow).Text =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 140, _inspectorWidth/2 - 10, 20),
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).Text);

           // Audio Clip
           
           // https://answers.unity.com/questions/900576/how-to-obtain-selected-files-in-the-project-window.html
           string boxText = "Select clip!";
           List<AudioClip> audioClips = new List<AudioClip>();
           foreach (Object o in Selection.objects)
           {
               if (o is AudioClip clip)
               {
                   audioClips.Add(clip);
                   _audioClipName = audioClips[0].name;
                   boxText = _audioClipName;
               }
           }
           GUI.Label(new Rect(position.width - _inspectorWidth + 10, 200, _inspectorWidth/3 - 30, 20),
               "Audio clip");
           GUI.Box(new Rect(position.width - 2 * _inspectorWidth/3 + 10, 202, _inspectorWidth/3 - 30, 20), 
               boxText);

           if (GUI.Button(new Rect(position.width - _inspectorWidth / 3 + 10, 202, _inspectorWidth / 3 - 30, 20),
               "Set") && audioClips.Count > 0)
           {
               _dialogController.FindNodeByWindowID(_lastTouchedWindow).DialogTextAudio = audioClips[0];
           }

           // Percent chance
           int ypos = 60;
           if (_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Count > 0)
           {
               while (_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Count >
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Count)
               {
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.RemoveAt( _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Count);
               }
               while (_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Count <
                      _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Count)
               {
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Add(10);
               }
               
               for(int i = 0; i < _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Count; i++)
               {
                   GUI.Label(new Rect(position.width + 10 - _inspectorWidth/2 * ((i%2) + 1), 200 + ypos, _inspectorWidth/3, 30), 
                       _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title);
                   
               }
           }
           
           
           // Method Info
           GUI.Label(new Rect(position.width - _inspectorWidth + 10, 
                   260 + ypos,
                   _inspectorWidth/2, 20),
               "Method name");
           _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodName = GUI.TextField(
                   new Rect(position.width - _inspectorWidth/2, 260 + ypos, _inspectorWidth/2 - 10, 20),
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodName);
           GUI.Label(new Rect(position.width - _inspectorWidth/2, 278 + ypos, _inspectorWidth/2 - 10, 40),
               "Leave blank if  there is no method to call after this line of dialog", 
               paragraph);
           
           // Arguments for method
           if (_dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodName != "")
           {
               GUI.Label(new Rect(position.width - _inspectorWidth + 10, 320 + ypos, _inspectorWidth/2, 20),
                   "Method arguments");
               _selectedArgumentType = EditorGUI.Popup(
                   new Rect(position.width - _inspectorWidth/3 + 10,  322 + ypos,_inspectorWidth / 3 - 30, 20),
                   _selectedArgumentType, _argumentTypes);
               switch (_selectedArgumentType)
               {
                   case 0: // String
                       _argument = GUI.TextField(
                           new Rect(position.width - 2 * _inspectorWidth / 3 + 30, 322+ ypos, _inspectorWidth / 3 - 30, 20),
                           _argument.ToString());
                       break;
                   case 1: // Int
                       object oldValue = _argument;
                       bool isInt = int.TryParse(GUI.TextField(
                           new Rect(position.width - 2 * _inspectorWidth / 3 + 30, 322+ ypos, _inspectorWidth / 3 - 30, 20),
                           _argument.ToString()), out var newInt);
                       
                       _argument = isInt ? newInt : oldValue;
                       break;
                   case 2: // Double
                       oldValue = _argument;
                       bool isDouble = Double.TryParse(GUI.TextField(
                           new Rect(position.width - 2 * _inspectorWidth / 3 + 30, 322+ ypos, _inspectorWidth / 3 - 30, 20),
                           _argument.ToString()), out var newDouble);
                       
                       _argument = isDouble ? newDouble : oldValue;
                       break;
                   case 3: // Float
                       oldValue = _argument;
                       bool isFloat = float.TryParse(GUI.TextField(
                           new Rect(position.width - 2 * _inspectorWidth / 3 + 30, 322+ ypos, _inspectorWidth / 3 - 30, 20),
                           _argument.ToString()), out var newFloat);
                       
                       _argument = isFloat ? newFloat : oldValue;
                       break;
                   case 4: // Bool
                       _boolArgument = EditorGUI.Popup(
                           new Rect(position.width - 2 * _inspectorWidth / 3 + 30, 322+ ypos, _inspectorWidth / 3 - 30, 20),
                           _boolArgument, new[] {"False", "True"});
                       _argument = _boolArgument == 1;
                       break;
               }

               if (GUI.Button(new Rect(position.width - _inspectorWidth + 10, 360+ ypos, _inspectorWidth - 20, 30),
                   "Add argument"))
               {
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments.Add(_argument);
                   _argument = null;
                   _selectedNodeType = -1;
               }
           }
           
        }
    }

    void WindowFunction (int windowID)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;

        GUI.Label(
            new Rect(0,-30, 100, 100),
            _dialogController.FindNodeByWindowID(windowID).Title, labelStyle);
        
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
        
        // Destroy node
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
                        node.LinkedNodesChance.ToList().RemoveAt(node.LinkedIds.IndexOf(windowID));
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
        _dialogController.DialogNodes[^1].NodeRect = new Rect(position.width/2, position.height/2, 100, 100);
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
