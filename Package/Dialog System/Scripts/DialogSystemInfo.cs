using UnityEngine;
using UnityEngine.UI;

public class DialogSystemInfo : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    public Texture2D ArrowTexture;
    [HideInInspector]
    public float ArrowSize;
    [HideInInspector]
    public int actorID;
    
    [Space(10)]
    [Header("Dialog appearance")]
    [Tooltip("Player choice button background - look of button graphics")]
    public Sprite ButtonBackground;
    [Tooltip("Background for somebody' line that is displayed")]
    public Sprite DialogLineBackground;
    public Font TextFont;
    public float FontSize;

    [Space(10)]
    [Header("Project info")]
    [SerializeField]
    [Tooltip(
        "Provide player's name that will be displayed on dialog (only if show speaker's name option is set to true)")]
    public string PlayerName = "You";
    [Tooltip("Select the layer that is on the player (you can find the layer mask in inspector - if player has no layer please add layer to player (preferred unique layer for player only))")]
    public LayerMask PlayerLayerMask;
    [Tooltip("Select key for entering the dialog and playing next line of dialog")]
    public KeyCode DialogActionKey = KeyCode.E;
    public Image ActionKeyGraphic;
    [Tooltip("Check true if your project is 3D or false if is 2D")]
    public bool Is3D = true;
    [Tooltip("That shows the name of the AI and name of the player in dialog if checked")]
    public bool ShowWhoIsSpeaking;
    [Tooltip("If set on true player will be allowed to not wait until text will show letter by letter, after pressing the activation key the text will be just show immediately.")]
    public bool IsTextDisplayEffectSkippable = true;

    public GameObject ChoiceButton;
    
    [HideInInspector]
    public bool InDialog = false;

    [HideInInspector] 
    public bool FirstRun = true;

}
