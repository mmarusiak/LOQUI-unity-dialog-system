using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogSystemInfo : MonoBehaviour
{
    [SerializeField]
    public Texture2D ArrowTexture;

    public float ArrowSize;

    
    
    public GUIStyle CenteredLabel()
    {
        GUIStyle centeredStyle = GUI.skin.label;
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        return centeredStyle;
    }
}
