using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCohete : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private GameObject explosionPrefab; // Opcional: efecto de explosi�n
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configurar el Rigidbody
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Lanzar el cohete hacia adelante
        rb.velocity = Vector3.forward * speed;

        // Destruir el cohete despu�s de un tiempo (por si no golpea nada)
        Destroy(gameObject, 10f);
    }

    void Update()
    {
        // Destruir si sale de los l�mites del juego
        if (transform.position.z > 10f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cubo"))
        {
            // Destruir el cubo
            ScriptCube cubeScript = other.GetComponent<ScriptCube>();
            if (cubeScript != null)
            {
                cubeScript.collisionWithBall(); // Usar el mismo m�todo que usa la bola
            }

            // Crear efecto de explosi�n si existe
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Autodestruirse
            Destroy(gameObject);
        }
        else if (other.CompareTag("Pared"))
        {
            // Opcional: crear efecto y destruirse al golpear paredes
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cubo"))
        {
            // Destruir el cubo
            ScriptCube cubeScript = collision.gameObject.GetComponent<ScriptCube>();
            if (cubeScript != null)
            {
                cubeScript.collisionWithBall();
            }

            // Crear efecto de explosi�n si existe
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            // Autodestruirse
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Pared"))
        {
            // Opcional: crear efecto y destruirse al golpear paredes
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }
            Destroy(gameObject);
        }
    }
}