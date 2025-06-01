// ScriptCohete.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCohete : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private GameObject explosionPrefab; // Opcional: efecto de explosión
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.velocity = Vector3.forward * speed;

        Destroy(gameObject, 10f);
    }

    void Update()
    {
        if (transform.position.z > 10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cubo") || other.CompareTag("Pared"))
        {
            // Solo para el cubo, activa la lógica de destrucción
            if (other.CompareTag("Cubo"))
            {
                ScriptCube cubeScript = other.GetComponent<ScriptCube>();
                if (cubeScript != null)
                {
                    cubeScript.collisionWithBall("Rocket"); // Asumiendo que esta función gestiona la destrucción del cubo
                }
            }

            // Crear efecto de explosión si existe
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Reproducir sonido de explosión
            if (GameManager.Instance != null && GameManager.Instance.hitRocket != null)
            {
                AudioSource.PlayClipAtPoint(GameManager.Instance.hitRocket, transform.position);
            }

            // Autodestruirse
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cubo"))
        {
            ScriptCube cubeScript = collision.gameObject.GetComponent<ScriptCube>();
            if (cubeScript != null)
            {
                cubeScript.collisionWithBall("Rocket");
            }

            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Reproducir sonido de explosión
            if (GameManager.Instance != null && GameManager.Instance.hitRocket != null)
            {
                AudioSource.PlayClipAtPoint(GameManager.Instance.hitRocket, transform.position);
            }

            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Pared"))
        {
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Reproducir sonido de explosión
            if (GameManager.Instance != null && GameManager.Instance.hitRocket != null)
            {
                AudioSource.PlayClipAtPoint(GameManager.Instance.hitRocket, transform.position);
            }

            Destroy(gameObject);
        }
    }
}