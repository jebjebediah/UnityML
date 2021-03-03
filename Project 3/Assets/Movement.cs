using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        gameObject.transform.position = new Vector3(Mathf.Clamp(transform.position.x + (x * speed), -5.0f, 5.0f), 0.5f, Mathf.Clamp(transform.position.z + (z * speed), -5.0f, 5.0f));
    }
}
