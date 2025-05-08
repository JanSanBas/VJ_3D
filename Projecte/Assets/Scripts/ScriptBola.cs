using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptBola : MonoBehaviour
{
    public bool gameStarted;

    public bool godMode;

    public float speed = 10f;
    public float maxBounceAngle = 75f;

    private Vector3 direction;
    private Rigidbody rb;

    private Transform paleta;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0.5f, -4f);
        gameStarted = false;
        godMode = false;

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb.gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.freezeRotation = true;
        rb.velocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        direction = new Vector3(0, 0, 1).normalized;

        paleta = GameObject.FindGameObjectWithTag("Paleta").transform;
        paleta.transform.position = new Vector3(0, 0.5f, -4.75f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                gameStarted = true;
                rb.velocity = direction * speed;
            }

            if (paleta != null)
            {
                Vector3 posPaleta = paleta.position;
                transform.position = new Vector3(posPaleta.x, transform.position.y, posPaleta.z + 0.75f);
            }
            return;
        }

        if (transform.position.z < -5.5f)
        {
            if (godMode)
            {

            }
            else
            {
                //Hacer aqui la logica de perder
                gameRestart();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            godMode = !godMode;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (gameStarted == false)
            return;

        if (collision.gameObject.CompareTag("Paleta"))
        {
            collisionWithPaleta(collision);
        }

        if (collision.gameObject.CompareTag("Pared"))
        {
            collisionWithPared(collision);
        }
    }

    void collisionWithPaleta(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 posPaleta = collision.transform.position;
        Vector3 contactPoint = contact.point;

        float medidaPaleta = collision.collider.bounds.size.x;
        float distAlMedio = contactPoint.x - posPaleta.x;

        float distanciaNormalizada = Mathf.Clamp((distAlMedio / (medidaPaleta / 2f)), -1f, 1f);

        float angle = (distanciaNormalizada * maxBounceAngle) * Mathf.Deg2Rad;

        direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;

        rb.velocity = direction * speed;
    }

    void collisionWithPared(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        direction = Vector3.Reflect(direction, normal).normalized;

        rb.velocity = direction * speed;
    }

    void gameRestart()
    {
        Start();
    }
}
