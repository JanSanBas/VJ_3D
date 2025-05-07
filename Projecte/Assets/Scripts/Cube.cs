using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Mantiene la velocidad de caída constante
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y*0.97f, rb.velocity.z);
    }
}

