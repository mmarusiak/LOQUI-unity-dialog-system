using UnityEngine;
using UnityEngine.UI;

public class DialogSystemInfo : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    public Texture2D ArrowTexture;
    [HideInInspector]
    public float ArrowSize;
    
    [Space(10)]
    [Header("Dialog appearance")]
    [Tooltip("Player choice button background - look of button graphics")]
    public Image ButtonBackground;
    [Tooltip("Background for somebody' line that is displayed")]
    public Image DialogLineBackground;
    public Font TextFont;
    public float FontSize;
    
    [Space(10)]
    [Header("Project info")]
    [SerializeField]
    [Tooltip("Select the layer that is on the player (you can find the layer mask in inspector - if player has no layer please add layer to player (preferred unique layer for player only))")]
    public LayerMask PlayerLayerMask;
    [Tooltip("Select key for entering the dialog")]
    public KeyCode ActivationKey = KeyCode.E;
    [Tooltip("Check true if your project is 3D or false if is 2D")]
    public bool Is3D = true;
    [Tooltip("That shows the name of the AI and name of the player in dialog if checked")]
    public bool ShowWhoIsSpeaking;
    [Tooltip("If set on true player will be allowed to not wait until text will show letter by letter, after pressing the activation key the text will be just show immediately.")]
    public bool IsTextDisplayEffectSkippable = true;
    
    [HideInInspector]
    public bool InDialog = false;

}
