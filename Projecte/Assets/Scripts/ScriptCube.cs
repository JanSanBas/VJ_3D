using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCube : MonoBehaviour
{
    private Rigidbody rb;
    private int puntuacionCubo = 500;
    private bool constraintsApplied = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ApplyConstraints();
    }

    void Update()
    {
        if (rb != null)
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.97f, rb.velocity.z);

            if (!constraintsApplied)
            {
                ApplyConstraints();
            }
        }
    }

    private void ApplyConstraints()
    {
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation |
                            RigidbodyConstraints.FreezePositionX |
                            RigidbodyConstraints.FreezePositionZ;

            constraintsApplied = true;

            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            rb.angularVelocity = Vector3.zero;

            Debug.Log($"Restricciones aplicadas a {gameObject.name}: {rb.constraints}");
        }
    }

    public void collisionWithBall()
    {
        GameManager.Instance.addScore(puntuacionCubo);

        // Intentar dropear power-up antes de destruir
        // PowerUpManager decidirá si puede dropear el Next Level basado en el conteo de cubos del GameManager
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.TryDropPowerUp(transform.position);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bola"))
        {
            collisionWithBall();
        }
    }
}