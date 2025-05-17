using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCube : MonoBehaviour
{

    private Rigidbody rb;
    private int puntuacionCubo = 500;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
      
    }
    // Update is called once per frame
    void Update()
    {
        // Mantiene la velocidad de caída constante
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y*0.97f, rb.velocity.z);
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bola"))
        {
            collisionWithBall();
        }
    }

    private void collisionWithBall()
    {
        GameManager.Instance.addScore(puntuacionCubo);
        Destroy(gameObject);
    }
}

