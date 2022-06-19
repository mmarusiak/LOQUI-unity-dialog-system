using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class DialogUINode
{
    public DialogNode Node;
    public Vector2 Position;
    public Rect Graphic;

    public DialogUINode(DialogNode node, Vector2 position, Rect graphic)
    {
        Node = node;
        Position = position;
        Graphic = graphic;
    }
}
public class DialogWindow : EditorWindow
{
    public Color32 responseColor = new Color32(172, 57, 49, 255);
    public Color32 passiveColor = new Color32(83, 125, 141, 255);
    
    Rect windowRect = new Rect (100 + 100, 100, 100, 100);
    Rect windowRect2 = new Rect (100, 100, 100, 100);

    public List<List<DialogUINode>> Dialogs = new List<List<DialogUINode>>();

    public bool creatorState = false;

    [MenuItem("Window/Dialog system window")]
    public static void Init()
    {
        DialogWindow window = (DialogWindow) GetWindow(typeof(DialogWindow));
        window.Show();


        GUIContent content = new GUIContent("Dialog editor");
        window.titleContent = content;
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(Vector2.zero, new Vector2(150, 50)),
            "Create new dialog node"))
        {
            creatorState = !creatorState;
        }

        if (creatorState)
        {
            EditorGUILayout.TextField("Enter node name", "");
        }
        
        Handles.BeginGUI();
        Handles.DrawBezier(windowRect.center, 
            windowRect2.center, 
            new Vector2(windowRect.xMax + 50f,windowRect.center.y), 
            new Vector2(windowRect2.xMin - 50f,windowRect2.center.y), 
            Color.red,null,5f);
        
        Handles.EndGUI();

        BeginWindows();
        windowRect = GUI.Window (0, windowRect, WindowFunction, "Box1");
        windowRect2 = GUI.Window (1, windowRect2, WindowFunction, "Box2");

        EndWindows();
    }
    
    void WindowFunction (int windowID) 
    {
        GUI.DragWindow();
    }
}
