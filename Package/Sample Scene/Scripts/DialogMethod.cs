using UnityEngine;

public class DialogMethod : MonoBehaviour
{
    public int testInt;
    public void Welcome(object args)
    {
        object[] arrargs = (object[]) args;
        
        string message = (string)arrargs[0];
        int number = (int) arrargs[1];
        bool boolean = (bool) arrargs[2];
        
        Debug.Log(message + " " + number + " " + boolean);
    }
}
