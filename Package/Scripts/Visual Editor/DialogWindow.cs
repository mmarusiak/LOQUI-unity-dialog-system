using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogWindow : EditorWindow
{
    private bool _creatingNode = false;
    string _newNodeTitle = "", _newNodeText = "";
    private int _selectedNodeType = -1, _lastTouchedWindow;

    public List<DialogNode> DialogNodes = new List<DialogNode>();

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
        // Create new node
        if (GUI.Button(new Rect(0, 0, position.width / 2, 60), "ADD NODE"))
        {
            _creatingNode = !_creatingNode;

            if (_selectedNodeType >= 0 && _newNodeTitle != "" && _newNodeText != "")
            {
                DialogNodes.Add(new DialogNode(_newNodeTitle, _newNodeText, (DialogNode.NodeType)_selectedNodeType));
            }

            _selectedNodeType = -1;
            _newNodeTitle = "";
            _newNodeText = "";
        }

        // Remove existing node
        if (GUI.Button(new Rect(position.width / 2, 0, position.width / 2, 60), "REMOVE NODE"))
        {
            DialogNodes.RemoveAt(_lastTouchedWindow);
        }

        if (_creatingNode)
        {
            // Title "box"
            GUI.Label(new Rect(0, 60, position.width, 40), 
                "Enter dialog node title", EditorStyles.boldLabel);
            _newNodeTitle = GUI.TextField(new Rect(position.width / 2, 60,
                        position.width / 2, 40), _newNodeTitle, 10);
            
            // Text "box"
            GUI.Label(new Rect(0, 100, position.width, 40), 
                "Enter dialog node text", EditorStyles.boldLabel);
            _newNodeText = GUI.TextField(new Rect(position.width / 2, 100,
                position.width / 2, 40), _newNodeText, 40);
            
            // Dialog type popup
            _selectedNodeType = EditorGUI.Popup(new Rect(3*position.width/8, 140, position.width/4, 40),
                _selectedNodeType, new string[]
                {
                    DialogNode.NodeType.Passive.ToString(),
                    DialogNode.NodeType.MultipleChoice.ToString()
                });
        }
        
        BeginWindows();
        for (int i = 0; i < DialogNodes.Count; i++)
        {
            GUI.color = _colorPallete[(int) DialogNodes[i].DialogNodeType];
            DialogNodes[i].NodeRect = GUI.Window(i, DialogNodes[i].NodeRect,
                    WindowFunction, DialogNodes[i].Title);
        }
        EndWindows();

        
    }
    
    void WindowFunction (int windowID)
    {
        GUI.DragWindow();
    }
}
