using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCube : MonoBehaviour
{

    private Rigidbody rb;

    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.97f, rb.velocity.z);
        // Bloquea completamente la rotación en X, Y y Z
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    void Update()
    {

    }
}

