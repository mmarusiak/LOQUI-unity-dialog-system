using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor;
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
    private int _selectedNodeType = -1, _selectedStartDialogType = -1, _selectedActor = -1, _selectedArgumentType = -1, _boolArgument = -1, 
        _lastTouchedWindow = -1, _selectedComponent, _selectedVariable;
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

    [MenuItem("Window/Dialog Visual Editor")] 
    public static void Init()
    {
        DialogWindow window = GetWindow<DialogWindow>("Dialog editor");
        window.Show();
    }

    private void OnGUI()
    {
        if (_dialogController == null && dialogActor != null)
        {
            _dialogController = dialogActor.GetComponent<DialogController>();
        }
        
        _actorCreated = _dialogController != null;
        
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
            _dialogSystemInfo.actorID = dialogActor.GetInstanceID();
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
        foreach (var go in allGameObjects)
        {
            dialogActor = go.GetInstanceID() == _dialogSystemInfo.actorID ? go : dialogActor;
        }

        if (dialogActor != null)
        {
            _selectedActor = allGameObjects.ToList().IndexOf(dialogActor);
        }
        _selectedActor = EditorGUI.Popup(new Rect(0, 0, 140, 20), _selectedActor,allGameObjects.Select(
            (gameobject) => gameobject.name).ToArray());

        if (_selectedActor >= 0 && _selectedActor < allGameObjects.Length)
        {
            if (dialogActor != allGameObjects[_selectedActor])
            {
                dialogActor = allGameObjects[_selectedActor];
                _dialogSystemInfo.actorID = dialogActor.GetInstanceID();
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
            _customDropDownShown = false;

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
                _newNodeTitle, 20);
            
            // Text "box"
            GUI.Label(new Rect(0, 120, position.width, 40), 
                "Enter dialog node text", EditorStyles.boldLabel);
            _newNodeText = GUI.TextField(
                new Rect(
                    position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 120,
                position.width / 2 - _inspectorWidth * Convert.ToInt32(_inspectorShown)/2, 40),
                _newNodeText);
            
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
            
            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.alignment = TextAnchor.MiddleCenter;
            windowStyle.normal.textColor = Color.white;
            windowStyle.fontStyle = FontStyle.Bold;
            GUI.color = _colorPallete[(int) _dialogController.DialogNodes[i].DialogNodeType];

            _dialogController.DialogNodes[i].NodeRect = GUI.Window(i, _dialogController.DialogNodes[i].NodeRect,
                    WindowFunction, _dialogController.DialogNodes[i].Title, windowStyle);
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
                    pair[1].NodeRect.y - 8, 
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
            
            GUI.Label(new Rect(position.width - _inspectorWidth + 10, 20, _inspectorWidth, 40), 
               dialogActor.name, goNameStyle);
            
            // Start of dialog conditions selection
            List<DialogNode> startNodes = new List<DialogNode>();
            List<int> linkedIds = new List<int>();

            foreach (var node in _dialogController.DialogNodes)
            {
                foreach(var id in node.LinkedIds)
                    linkedIds.Add(id);
            }

            foreach (var node in _dialogController.DialogNodes)
            {
                if (!linkedIds.Contains(node.WindowID) && node.LinkedIds.Count > 0)
                {
                    startNodes.Add(node);
                }
            }


            int startY = 0;
            if (startNodes.Count > 1 && startNodes.Contains(_dialogController.FindNodeByWindowID(_lastTouchedWindow)))
            {
                // list of all conditions, that can be serialized
                var allGameObjects = FindObjectsOfType<GameObject>();
                var fieldNodesList = new List<ConditionNode>();
                
                foreach (var go in allGameObjects)
                {
                    foreach (var component in go.GetComponents<Component>())
                    {
                        if (component is MonoBehaviour && component.GetType().ToString() != "DialogSystemInfo")
                        {
                            foreach (var field in component.GetType().GetFields().Where(
                                (field) =>
                                    field.IsPublic &&
                                    (field.FieldType == typeof(int)
                                     || field.FieldType == typeof(double)
                                     || field.FieldType == typeof(float)
                                     || field.FieldType == typeof(bool)
                                     || field.FieldType == typeof(string)
                                     || field.FieldType == typeof(char))).ToArray())
                            {
                                fieldNodesList.Add(new ConditionNode(field.Name,
                                    component.GetType().ToString(), go.GetInstanceID()));
                            }
                        }
                    }
                }

                // make popup of all field nodes in the list and system to select it
                List<string> popupTexts = new List<string>();
                foreach (var fieldNode in fieldNodesList)
                {
                    popupTexts.Add(  fieldNode.FieldName + " = " + 
                                   fieldNode.Field.GetValue(fieldNode.Component) 
                                   + " | " + fieldNode.TargetGameObject.name + " > " + fieldNode.ComponentName);
                }

                // select how the start of dialog will be chosen - by random or by condition
                _selectedStartDialogType = _dialogController.dialogStartType;
                
                GUIStyle centerLabel = new GUIStyle(GUI.skin.label);
                centerLabel.alignment = TextAnchor.MiddleCenter;
                
                GUI.Label(new Rect(position.width - _inspectorWidth, 80, _inspectorWidth, 20),
                    "Choose start of dialog by:", centerLabel);
                _selectedStartDialogType = EditorGUI.Popup(
                    new Rect(position.width - _inspectorWidth/2 - _inspectorWidth/6 + 15, 110, _inspectorWidth / 3 - 30, 20),
                    _selectedStartDialogType, new[] {"Random", "Condition"});

                if (_selectedStartDialogType > -1)
                {
                    // store value in dialog controller
                    _dialogController.dialogStartType = _selectedStartDialogType;

                    // if start will be selected by random
                    if (_dialogController.dialogStartType == 0)
                    {
                        // make panel with all possible starts and text fields to set percentage chance to it
                    }
                    
                    // if start will be selected by condition
                    else
                    {
                        _dialogController.conditionOption = EditorGUI.Popup(new Rect(position.width - _inspectorWidth / 2 - _inspectorWidth / 6, 140, _inspectorWidth / 3, 20),
                            _dialogController.conditionOption, popupTexts.ToArray());
                        if (_dialogController.conditionOption > -1)
                        {
                            _dialogController.selectedCondition = fieldNodesList[_dialogController.conditionOption];
                            var type = _dialogController.selectedCondition.Field.FieldType;
                            if (type == typeof(string) || type == typeof(bool) || type == typeof(char))
                            {
                                GUI.Label(new Rect(position.width - 2 * _inspectorWidth / 3 - 100, 180, 100, 20),
                                    "If is equal to", centerLabel);
                                GUI.Label(new Rect(position.width - _inspectorWidth / 3, 180, 100, 20),
                                    "If isn't equal to", centerLabel);
                                
                                if (type == typeof(bool))
                                {
                                    _dialogController.boolConditionValue = EditorGUI.Popup(new Rect(position.width - _inspectorWidth / 2 - _inspectorWidth / 6, 170, _inspectorWidth / 3, 20),
                                        _dialogController.boolConditionValue, new []{"False","True"});
                                }
                                else if (type == typeof(string))
                                {
                                    _dialogController.strConditionValue = GUI.TextField(new Rect(position.width - _inspectorWidth / 2 - _inspectorWidth / 6, 170, _inspectorWidth / 3, 20),
                                        _dialogController.strConditionValue);
                                }
                                else
                                {
                                    _dialogController.charConditionValue = GUI.TextField(new Rect(position.width - _inspectorWidth / 2 - _inspectorWidth / 6, 170, _inspectorWidth / 3, 20),
                                        _dialogController.charConditionValue.ToString(), 1)[0];
                                }
                            }
                            else
                            {
                                _dialogController.equationType = EditorGUI.Popup(new Rect(position.width - 2*_inspectorWidth / 3 - _inspectorWidth / 6, 170, _inspectorWidth / 3, 20),
                                    _dialogController.equationType, new []{"Equals to: ", "Greater than: ", "Less than:"});

                                GUIStyle symbolStyle = new GUIStyle(GUI.skin.label);
                                symbolStyle.fontStyle = FontStyle.Bold;
                                symbolStyle.fontSize = 18;
                                symbolStyle.alignment = TextAnchor.MiddleCenter;
                                
                                GUI.Label(new Rect(position.width - 2 * _inspectorWidth / 3 - 100, 190, 100, 20),
                                    "✓", symbolStyle);
                                GUI.Label(new Rect(position.width - _inspectorWidth / 3, 190, 100, 20),
                                    "x", symbolStyle);
                                
                                if (type == typeof(float))
                                {
                                    bool isFloat = float.TryParse(GUI.TextField(
                                        new Rect(position.width - _inspectorWidth / 3 - _inspectorWidth / 6, 170,
                                            _inspectorWidth / 3, 20),
                                        _dialogController.floatConditionValue.ToString()), out var newVal);
                                    _dialogController.floatConditionValue = isFloat ? newVal : _dialogController.floatConditionValue;
                                }else if (type == typeof(int))
                                {
                                    bool isInt = int.TryParse(GUI.TextField(
                                        new Rect(position.width - _inspectorWidth / 3 - _inspectorWidth / 6, 170,
                                            _inspectorWidth / 3, 20),
                                        _dialogController.floatConditionValue.ToString()), out var newVal);
                                    _dialogController.intConditionValue = isInt ? newVal : _dialogController.intConditionValue;
                                }else if (type == typeof(double))
                                {
                                    bool isDouble = double.TryParse(GUI.TextField(
                                        new Rect(position.width - _inspectorWidth / 3 - _inspectorWidth / 6, 170,
                                            _inspectorWidth / 3, 20),
                                        _dialogController.floatConditionValue.ToString()), out var newVal);
                                    _dialogController.doubleConditionValue = isDouble ? newVal : _dialogController.doubleConditionValue;
                                }
                                // also "numbers" so we need to choose if we want if something is equal, greater etc.
                            }
                        }

                        // if dev enters first time, make conditions met lists by default by adding nodes to each of the list
                        if (_dialogController.onConditionMet.Count == 0 && _dialogController.onConditionDoesntMet.Count != startNodes.Count
                        || _dialogController.onConditionDoesntMet.Count == 0 && _dialogController.onConditionMet.Count != startNodes.Count)
                        {
                            _dialogController.onConditionMet = new List<DialogNode>();
                            _dialogController.onConditionDoesntMet = new List<DialogNode>();

                            foreach (var node in startNodes)
                            {
                                if(_dialogController.onConditionMet.Count > _dialogController.onConditionDoesntMet.Count)
                                    _dialogController.onConditionDoesntMet.Add(node);
                                else
                                    _dialogController.onConditionMet.Add(node);
                            }
                        }
                        
                        // two rows of nodes, one that met conditions and second one that doesn't
                        // node will be played if condition is met of if isn't
                        // also percent chances should be added if nodes count for one row will be more than one
                      
                        for (int i = 0; i < _dialogController.onConditionMet.Count; i ++)
                        {
                            if (GUI.Button(new Rect(position.width - 2 * _inspectorWidth / 3 - 100, 190 +
                                    (i + 1) * 25, 100, 20),
                                _dialogController.onConditionMet[i].Title + " > "))
                            {
                                _dialogController.onConditionDoesntMet.Add(_dialogController.onConditionMet[i]);
                                _dialogController.onConditionMet.RemoveAt(i);
                            }
                        }
                        
                        
                        foreach (var node in _dialogController.onConditionDoesntMet)
                        {
                            if (GUI.Button(new Rect(position.width - _inspectorWidth / 3, 190 +
                                    (_dialogController.onConditionDoesntMet.IndexOf(node) + 1) * 25, 100, 20),
                                " < " + node.Title))
                            {
                                _dialogController.onConditionMet.Add(node);
                                _dialogController.onConditionDoesntMet.RemoveAt(_dialogController.onConditionDoesntMet.IndexOf(node));
                                break;
                            }
                        }

                        // make two rows of panels, one that meets condition, second that doesn't
                        // if more than two starts of dialogs possible, add percentage set-up option
                        //  to nodes in rows that contains more than one node
                    }
                }

                startY = (_dialogController.onConditionDoesntMet.Count - 1) * 25 + 190;
                if (_dialogController.onConditionMet.Count > _dialogController.onConditionDoesntMet.Count)
                {
                    startY = (_dialogController.onConditionMet.Count - 1) * 25 + 190;
                }
            }




            // Title info
            GUI.Label(new Rect(position.width - _inspectorWidth + 10, 80 + startY, _inspectorWidth/2 - 20, 20),
                "Node title");
            _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 80 + startY, _inspectorWidth/2 - 10, 20),
                    _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title, 20);
            
            // Text info
            GUI.Label(new Rect(position.width - _inspectorWidth + 10, 140 + startY, _inspectorWidth/2 - 20, 20),
                "Node text");
           _dialogController.FindNodeByWindowID(_lastTouchedWindow).Text =
                GUI.TextField(
                    new Rect(position.width - _inspectorWidth/2, 140 + startY, _inspectorWidth/2 - 10, 20),
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
           
           if (_dialogController.FindNodeByWindowID(_lastTouchedWindow).DialogTextAudio != null)
           { 
               boxText = _dialogController.FindNodeByWindowID(_lastTouchedWindow).DialogTextAudio.name;
           }

           GUI.Label(new Rect(position.width - _inspectorWidth + 10, 200 + startY, _inspectorWidth/3 - 30, 20),
               "Audio clip");
           GUI.Box(new Rect(position.width - 2 * _inspectorWidth/3 + 10, 202 + startY, _inspectorWidth/3 - 30, 20), 
               boxText);

           if (GUI.Button(new Rect(position.width - _inspectorWidth / 3 + 10, 202 + startY, _inspectorWidth / 3 - 30, 20),
               "Set") && audioClips.Count > 0)
           {
               _dialogController.FindNodeByWindowID(_lastTouchedWindow).DialogTextAudio = audioClips[0];
           }

           // Percent chance
           int ypos = 0;
           if (_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Count > 1 & _dialogController.FindNodeByWindowID(_lastTouchedWindow).DialogNodeType == DialogNode.NodeType.PlayerNode)
           {
               ypos = 60;
               GUI.Label(new Rect(position.width - _inspectorWidth + 10, 265 + startY, _inspectorWidth - 15, 30), 
                   "Select chance foreach of node \nconnected to: " + _dialogController.FindNodeByWindowID(_lastTouchedWindow).Title);
               
               while (_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Count <
                      _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds.Count)
               {
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Add(10);
               }
               
               for(int i = 0; i < _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance.Count; i++)
               {
                   if (i % 2 == 0)
                   {
                       ypos += 60;
                   }
                   GUI.Label(new Rect(position.width + 10 - _inspectorWidth/2 * (i%2 + 1), 175 + ypos + startY, _inspectorWidth/3, 30), 
                       _dialogController.FindNodeByWindowID(_dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedIds[i]).Title);

                   int.TryParse(GUI.TextField(new Rect(position.width + 10 - _inspectorWidth / 2 * (i % 2 + 1), 205 + ypos + startY,
                           _inspectorWidth / 3, 30), _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance[i].ToString()), 
                       out var newChance);

                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).LinkedNodesChance[i] = newChance;
               }
           }

           // just to make stuff easier, to not put startY in every line that contains y axis related stuff
           ypos += startY;
           
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
                   _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments.Add(new MethodArgument(_argument, _selectedArgumentType));
                   _argument = "";
                   _selectedNodeType = -1;
               }
               
               // arguments list, option to destroy
               if(_dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments.Count > 0)
                   GUI.Label(new Rect(position.width - _inspectorWidth + 10, 420 + ypos, _inspectorWidth/2, 20),
                       "Arguments list");
               
               int currentYPos = 420 + ypos;

               
               // list is off, here it spams errors + list of arguments is not serializable, so it wont "save" data after leaving unity
               for (int i = 0;
                   i < _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments.Count;
                   i++)
               { 
                   float xPos = position.width - _inspectorWidth + 10 + _inspectorWidth * 2 / 3;
                   if (i % 2 == 0) 
                   { 
                       currentYPos += 40; 
                       xPos = position.width - _inspectorWidth + 10 + _inspectorWidth / 3;
                   }
                       
                   if (GUI.Button(new Rect(xPos, currentYPos, _inspectorWidth / 3 - 30, 30),
                       _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments[i].Content().ToString() + " [-]"))
                   {
                       _dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments.
                           Remove(_dialogController.FindNodeByWindowID(_lastTouchedWindow).MethodArguments[i]);
                   }
               }
           }
        }
    }

    void WindowFunction (int windowID)
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;

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
                        try
                        {
                            if (_dialogController.FindNodeByWindowID(linkedId).DialogNodeType ==
                                DialogNode.NodeType.AINode)
                                node.LinkedNodesChance.ToList().RemoveAt(node.LinkedIds.IndexOf(windowID));

                            node.LinkedIds.Remove(windowID);
                            break;
                        }
                        catch{}
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
