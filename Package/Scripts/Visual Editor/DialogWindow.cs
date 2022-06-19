using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public class DialogWindow : EditorWindow
{
    Rect windowRect = new Rect (100 + 100, 100, 100, 100);
    Rect windowRect2 = new Rect (100, 100, 100, 100);

    private bool _addingNode = false;
    private Vector2 _scrollPosition = Vector2.zero;
    private int _idOfLastTouhedNode;

    public List<DialogNode> NodeList = new List<DialogNode>();


    [MenuItem("Window/Dialog system window")]
    public static void Init()
    {
        DialogWindow window = GetWindow<DialogWindow>("Dialog editor");
        window.Show();
        
        GUIContent content = new GUIContent("Dialog editor");
        window.titleContent = content;
    }

    private void OnGUI()
    {
        if (_addingNode)
        {
            string _title = "", _text = "";
            GUILayout.BeginHorizontal();
            _title = EditorGUILayout.TextField("Node title", _title);
            _text = EditorGUILayout.TextField("Node text", _text);
            GUILayout.EndHorizontal();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add node"))
        {
            _addingNode = !_addingNode;
            NodeList.Add(new DialogNode("test", "test123,", DialogNode.NodeType.Passive));
        }

        if (GUILayout.Button("Remove node"))
        {
            NodeList.RemoveAt(_idOfLastTouhedNode);
        }
        GUILayout.EndHorizontal();

        _scrollPosition = GUI.BeginScrollView(
            new Rect(0, 0, position.width-1, position.height), 
                _scrollPosition, new Rect(0, 0, position.width, position.height));
        
        Handles.BeginGUI();
        Handles.DrawBezier(windowRect.center, 
            windowRect2.center, 
            new Vector2(windowRect.xMax + 50f,windowRect.center.y), 
            new Vector2(windowRect2.xMin - 50f,windowRect2.center.y), 
            Color.red,null,5f);
        
        Handles.EndGUI();

        BeginWindows();
        for (int i = 0; i < NodeList.Count; i++)
        {
            NodeList[i].NodeRect = 
                KeepInWindow(GUI.Window(i, NodeList[i].NodeRect,
                    WindowFunction, NodeList[i].Title));
        }
        EndWindows();
        GUI.EndScrollView();
    }
    
    void WindowFunction (int windowID)
    {
        GUI.DragWindow();
    }

    Rect KeepInWindow(Rect rect)
    {
        if (rect.position.x > position.width)
            rect.position = new Vector2(position.width, rect.position.y);
        if (rect.position.y > position.height)
            rect.position = new Vector2(rect.position.x, position.height);

        if (rect.position.x < 0)
            rect.position = new Vector2(0, rect.position.y);
        if (rect.position.y < 0)
            rect.position = new Vector2(rect.position.x, 0);
            
        return rect;
    }
    
}
