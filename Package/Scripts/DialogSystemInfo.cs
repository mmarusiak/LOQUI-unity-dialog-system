using UnityEngine;

public class DialogSystemInfo : MonoBehaviour
{
    [SerializeField]
    public Texture2D ArrowTexture;
    [SerializeField]
    public LayerMask PlayerMask;

    public KeyCode ActivationKey = KeyCode.E;
    public bool Is3D = true;
    public bool InDialog = false;
    public float ArrowSize;

}
