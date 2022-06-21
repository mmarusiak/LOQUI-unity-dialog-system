using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DialogWindow : EditorWindow
{
    private bool _creatingNode = false, _actorCreated = false;
    string _newNodeTitle = "", _newNodeText = "";
    private int _selectedNodeType = -1, _selectedActor = -1, _lastTouchedWindow = -1;

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
        DrawGameObjectPopup();
        
        if (_actorCreated)
        {
            DrawMainPanel();
        }
        else
        {
            DrawActorPanel();
        }
    }

    void DrawGameObjectPopup()
    {
        var allGameObjects = Object.FindObjectsOfType<GameObject>();
        _selectedActor = EditorGUI.Popup(new Rect(0, 0, 140, 20), _selectedActor,allGameObjects.Select(
            (_gameobject) => _gameobject.name).ToArray());

        dialogActor = allGameObjects[_selectedActor];
        _actorCreated = dialogActor.GetComponent<DialogController>() != null;
    }
    
    void DrawMainPanel()
    {
        // Create new node
        if (GUI.Button(new Rect(0, 20, position.width / 2, 60), "ADD NODE"))
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
        if (GUI.Button(new Rect(position.width / 2, 20, position.width / 2, 60), "REMOVE NODE"))
        {
            dialogActor.GetComponent<DialogController>().DialogNodes.RemoveAt(_lastTouchedWindow);
        }

        if (_creatingNode)
        {
            // Title "box"
            GUI.Label(new Rect(0, 80, position.width, 40), 
                "Enter dialog node title", EditorStyles.boldLabel);
            _newNodeTitle = GUI.TextField(new Rect(position.width / 2, 80,
                        position.width / 2, 40), _newNodeTitle, 10);
            
            // Text "box"
            GUI.Label(new Rect(0, 120, position.width, 40), 
                "Enter dialog node text", EditorStyles.boldLabel);
            _newNodeText = GUI.TextField(new Rect(position.width / 2, 120,
                position.width / 2, 40), _newNodeText, 40);
            
            // Dialog type popup
            _selectedNodeType = EditorGUI.Popup(new Rect(3*position.width/8, 160, position.width/4, 40),
                _selectedNodeType, new string[]
                {
                    DialogNode.NodeType.AINode.ToString(),
                    DialogNode.NodeType.PlayerNode.ToString()
                });
        }
        
        BeginWindows();
        for (int i = 0; i < dialogActor.GetComponent<DialogController>().DialogNodes.Count; i++)
        {
            dialogActor.GetComponent<DialogController>().DialogNodes[i].WindowID = i;
            
            GUI.color = _colorPallete[(int) dialogActor.GetComponent<DialogController>().DialogNodes[i].DialogNodeType];
            dialogActor.GetComponent<DialogController>().DialogNodes[i].NodeRect = 
                GUI.Window(i, dialogActor.GetComponent<DialogController>().DialogNodes[i].NodeRect,
                    WindowFunction, dialogActor.GetComponent<DialogController>().DialogNodes[i].Title);
        }
        EndWindows();
    }

    void DrawActorPanel()
    {
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        GUI.Label(new Rect(position.width/4, 40, position.width/2, 40), 
            "Selected Game Object is not Dialog Actor!", centeredStyle);
        if(GUI.Button(new Rect(position.width / 4, 80, position.width / 2, 40),
            "Create new dialog actor"))
        {
            dialogActor.AddComponent<DialogController>();
            _actorCreated = true;
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
            _lastTouchedWindow = windowID;
        }
        GUI.DragWindow();
    }
}
