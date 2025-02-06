using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Force : MonoBehaviour
{
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.S))
        {
            rb.AddTorque(-120, 0, 0, ForceMode.Impulse);
        }*/
    }
}
