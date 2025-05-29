using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCube : MonoBehaviour
{
    private Rigidbody rb;
    private int puntuacionCubo = 500;
    private bool constraintsApplied = false;
    public GameObject particulasPrefab;

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

        if (particulasPrefab != null)
        {
            GameObject efecto = Instantiate(particulasPrefab, transform.position, Quaternion.identity);
            Destroy(efecto, 2f);
        }

        GameManager.Instance.addScore(puntuacionCubo);

        Destroy(gameObject,0.01f);
    }
}