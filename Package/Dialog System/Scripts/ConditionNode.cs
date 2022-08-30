using System.Linq;
using System.Reflection;
using UnityEngine;

public class ConditionNode
{
    public string FieldName;
    public string ComponentName;
    public int TargetGameObjectID;

    public FieldInfo Field;
    public Component Component;
    public GameObject TargetGameObject;
    
    
    public ConditionNode(string fieldName, string cmpName, int goID)
    {
        FieldName = fieldName;
        ComponentName = cmpName;
        TargetGameObjectID = goID;
        
        Init();
    }

    public void Init()
    {
        TargetGameObject = (GameObject)typeof(UnityEngine.Object)
            .GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { TargetGameObjectID });
        Component = TargetGameObject.GetComponent(ComponentName);

        var field = Component.GetType().GetFields().Where((field) => field.IsPublic & field.Name == FieldName);
        Field = field.ToArray()[0];
        Debug.Log($"{TargetGameObject.name} : {Component} : {Field} : {Field.GetValue(Component)}");
    }

}
