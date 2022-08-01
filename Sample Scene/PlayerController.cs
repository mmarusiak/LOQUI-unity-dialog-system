using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontalAxis, verticalAxis;

    public float speed = .0001f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");

        transform.position = new Vector3(transform.position.x + horizontalAxis * speed,
            transform.position.y, transform.position.z + verticalAxis * speed);
    }
}
