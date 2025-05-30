using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptBola : MonoBehaviour
{
    public bool gameStarted;

    public bool godMode;

    public float speed = 10f;
    public float maxBounceAngle = 75f;

    private float lastBounceTime;
    private float bounceCooldown = 0.05f; // Cooldown time in seconds

    private Vector3 direction;
    private Rigidbody rb;

    private Transform paleta;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0.68f, -7.74f);
        gameStarted = false;
        godMode = false;
        lastBounceTime = 0f;

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.freezeRotation = true;
        rb.velocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        direction = new Vector3(0, 0, 1).normalized;

        paleta = GameObject.FindGameObjectWithTag("Paleta").transform;
        paleta.transform.position = new Vector3(0, 0.68f, -8.5f);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.constraints = RigidbodyConstraints.FreezePositionY;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                gameStarted = true;
                ApplyVelocity();
            }

            if (paleta != null)
            {
                Vector3 posPaleta = paleta.position;
                transform.position = new Vector3(posPaleta.x, transform.position.y, posPaleta.z + 0.75f);
            }
            return;
        }

        if (gameStarted && rb.velocity.magnitude != speed)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }

        if (transform.position.z < -9f)
        {
            if (godMode)
            {
                if (Time.time - lastBounceTime > bounceCooldown)
                {
                    lastBounceTime = Time.time;

                    Vector3 normal = new Vector3(0, 0, 1);
                    direction = Vector3.Reflect(direction, normal).normalized;

                    Vector3 pos = transform.position;
                    pos.z = -9f + 0.2f;
                    transform.position = pos;

                    ApplyVelocity();
                }
            }
            else
            {
                GameManager.Instance.reduceLives();
                gameRestart();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            godMode = !godMode;
            updatePaletaCollision();
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

        else if (collision.gameObject.CompareTag("Pared"))
        {
            collisionWithPared(collision);
        }

        else if (collision.gameObject.CompareTag("Cubo"))
        {
            collisionWithCubo(collision);
        }
    }

    void updatePaletaCollision()
    {
        if (paleta != null)
        {
            Collider paletaCollider = paleta.GetComponent<Collider>();
            if (paletaCollider != null)
            {
                Collider bolaCollider = GetComponent<Collider>();
                if (bolaCollider != null)
                {
                    Physics.IgnoreCollision(bolaCollider, paletaCollider, godMode);
                }
            }
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

        ApplyVelocity();
    }

    void collisionWithPared(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        direction = Vector3.Reflect(direction, normal).normalized;

        ApplyVelocity();
    }

    void collisionWithCubo(Collision collision)
    {
        if (Time.time - lastBounceTime > bounceCooldown)
        {
            if (collision.gameObject == null) return;

            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            direction = Vector3.Reflect(direction, normal).normalized;

            collision.gameObject.GetComponent<ScriptCube>().collisionWithBall();

            ApplyVelocity();

            lastBounceTime = Time.time;
        }
    }

    void gameRestart()
    {
        gameStarted = false;
        transform.position = new Vector3(0, 0.68f, -7.74f);
        rb.velocity = Vector3.zero;
        if (paleta != null)
        {
            paleta.transform.position = new Vector3(0, 0.68f, -8.5f);
        }
        direction = new Vector3(0, 0, 1).normalized;
        ApplyVelocity();
    }

    void ApplyVelocity()
    {
        rb.velocity = direction * speed;
    }
}
