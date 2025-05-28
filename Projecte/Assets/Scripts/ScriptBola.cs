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
    private float bounceCooldown = 0.03f; // Cooldown time in seconds

    private Vector3 direction;
    private Rigidbody rb;

    private Transform paleta;

    private bool isPowerBallActive = false; // Estado del PowerBall en la bola

    // --- Nuevas variables para el imán ---
    private bool isMagnetPowerUpActive = false;
    private bool isBallAttached = false;
    private Vector3 offsetFromPaddle;

    // --- Nuevas variables para la liberación controlada ---
    private bool isReleasing = false; // Nuevo: indica si la bola está en proceso de liberación
    [SerializeField] private float releaseImmunityDuration = 0.1f; // Nuevo: tiempo que ignora colisiones con la paleta al liberarse

    [SerializeField] private Coroutine godModeDuration;

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
        // ApplyVelocity(); // La velocidad se aplica solo cuando el juego empieza

        GameObject paddleObj = GameObject.FindGameObjectWithTag("Paleta");
        if (paddleObj != null)
        {
            paleta = paddleObj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Si el juego no ha empezado, la bola se mueve con la paleta
        if (!gameStarted)
        {
            if (paleta != null)
            {
                transform.position = new Vector3(paleta.position.x, transform.position.y, paleta.position.z + 0.76f);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameStarted = true;
                ApplyVelocity();
            }
        }
        else if (isBallAttached) // Si la bola está enganchada por el imán
        {
            transform.position = paleta.position + offsetFromPaddle;
            rb.velocity = Vector3.zero; // Asegurar que no se mueva por su cuenta

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ReleaseBall(); // Liberar la bola
            }
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
                GameManager.Instance.reduceLives(); // Llama al GameManager para reducir una vida
                gameRestart(); // Reinicia la posición de la bola para la siguiente vida
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            godMode = !godMode;
            updatePaletaCollision();
        }
    }

    void ApplyVelocity()
    {
        rb.velocity = direction * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si la bola está en proceso de liberación y colisiona con la paleta, ignorar
        if (isReleasing && collision.gameObject.CompareTag("Paleta"))
        {
            return;
        }

        if (Time.time - lastBounceTime < bounceCooldown)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Pared"))
        {
            collisionWithPared(collision);
        }
        else if (collision.gameObject.CompareTag("Cubo"))
        {
            collisionWithCubo(collision);
        }
        else if (collision.gameObject.CompareTag("Paleta"))
        {
            collisionWithPaleta(collision);
        }
    }

    void collisionWithPaleta(Collision collision)
    {
        // Si el imán está activo y la bola no está ya enganchada, engancharla
        if (isMagnetPowerUpActive && !isBallAttached)
        {
            AttachBall(collision);
            PowerUpManager.Instance.UseMagnetCharge(); // Notificar al PowerUpManager que se usó una carga
            return;
        }

        ContactPoint contact = collision.contacts[0];
        Vector3 paddleCenter = collision.gameObject.transform.position;

        float hitPointX = contact.point.x - paddleCenter.x;
        float paddleWidth = collision.collider.bounds.size.x;
        float normalizedHitPointX = hitPointX / (paddleWidth / 2f);

        float bounceAngle = normalizedHitPointX * maxBounceAngle;


        direction = Quaternion.Euler(0, bounceAngle, 0) * Vector3.forward;

        if (direction.z < 0) direction.z *= -1;

        direction.Normalize();

        ApplyVelocity();
        lastBounceTime = Time.time;
    }

    void collisionWithPared(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;

        direction = Vector3.Reflect(direction, normal).normalized;

        if (Mathf.Abs(direction.z) < 0.1f)
        {
            if (direction.z >= 0)
            {
                direction = new Vector3(direction.x, direction.y, 0.1f).normalized;
            }
            else
            {
                direction = new Vector3(direction.x, direction.y, -0.1f).normalized;
            }
        }

        ApplyVelocity();
        lastBounceTime = Time.time;
    }

    void collisionWithCubo(Collision collision)
    {
        if (Time.time - lastBounceTime > bounceCooldown)
        {
            if (collision.gameObject == null) return;

            ScriptCube cubeScript = collision.gameObject.GetComponent<ScriptCube>();
            if (cubeScript != null)
            {
                cubeScript.collisionWithBall();
            }

            if (!isPowerBallActive) // Si PowerBall NO está activo, la bola rebota.
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 normal = contact.normal;
                direction = Vector3.Reflect(direction, normal).normalized;

                float randomAngleOffset = Random.Range(-5f, 5f);
                direction = Quaternion.Euler(0, randomAngleOffset, 0) * direction;
                direction.Normalize();
            }
            // Si isPowerBallActive es true, la bola no rebota, simplemente destruye el cubo.

            ApplyVelocity();
            lastBounceTime = Time.time;
        }
    }

    public void SetPowerBall(bool active)
    {
        isPowerBallActive = active;
    }

    public void SetMagnetPowerUp(bool active) // Método para establecer el estado del imán
    {
        isMagnetPowerUpActive = active;
        if (!active && isBallAttached) // Si el imán se desactiva y la bola está enganchada, liberarla
        {
            ReleaseBall();
        }
    }

    private void AttachBall(Collision collision) // Lógica para enganchar la bola
    {
        isBallAttached = true;
        rb.isKinematic = true; // Desactivar física para que no se mueva
        rb.velocity = Vector3.zero; // Asegurarse de que se detiene

        // Calcular el offset para mantener la posición relativa a la paleta
        offsetFromPaddle = transform.position - paleta.position;

        Debug.Log("Bola enganchada a la paleta!");
    }

    public void ReleaseBall() // Lógica para liberar la bola
    {
        if (!isBallAttached) return; // Si no está enganchada, no hacer nada

        isBallAttached = false;
        rb.isKinematic = false; // Reactivar física

        // Iniciar el período de inmunidad
        StartCoroutine(ReleaseImmunityRoutine());

        // La dirección de liberación puede ser un poco más dinámica.
        // Podrías usar el movimiento de la paleta, o simplemente el centro de la paleta.
        // Aquí vamos a usar la lógica de rebote de la paleta para darle una dirección inicial.
        // Para esto necesitamos simular los puntos de contacto, o hacer un rebote simple hacia arriba.
        // Para un inicio consistente, usaremos una dirección ligeramente aleatoria o basada en la posición actual en la paleta.

        // Opción 2: Usar la lógica de rebote de la paleta (más realista)
        // Necesitamos calcular el punto de impacto como si acabara de chocar.
        // Una aproximación simple es usar la posición x relativa de la bola sobre la paleta.
        float hitPointX = transform.position.x - paleta.position.x;
        float paddleWidth = paleta.GetComponent<Collider>().bounds.size.x; // Asume que la paleta tiene un collider
        float normalizedHitPointX = hitPointX / (paddleWidth / 2f);
        float bounceAngle = normalizedHitPointX * maxBounceAngle;

        direction = Quaternion.Euler(0, 0, bounceAngle) * Vector3.forward;
        if (direction.z < 0) direction.z *= -1; // Asegura que vaya hacia arriba
        direction.Normalize();

        ApplyVelocity();
        Debug.Log("Bola liberada!");
    }

    private IEnumerator ReleaseImmunityRoutine() // Coroutine para inmunidad tras liberación
    {
        isReleasing = true;
        yield return new WaitForSeconds(releaseImmunityDuration);
        isReleasing = false;
    }

    public void gameRestart()
    {
        gameStarted = false;
        godMode = false;
        isPowerBallActive = false;
        isMagnetPowerUpActive = false; // Asegurarse de que el imán se desactiva en la bola
        isBallAttached = false; // Asegurarse de que no esté enganchada
        rb.isKinematic = false; // Reactivar la física si estaba desactivada

        // Asegurarse de que el estado de liberación también se resetee
        isReleasing = false;
        StopAllCoroutines(); // Detener coroutines pendientes (como ReleaseImmunityRoutine)


        transform.position = new Vector3(0, 0.68f, -7.74f);
        rb.velocity = Vector3.zero;
        if (paleta != null)
        {
            paleta.transform.position = new Vector3(0, 0.68f, -8.5f);
        }
        direction = new Vector3(0, 0, 1).normalized;
        ApplyVelocity();
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

    public void ActivateGodMode(float duration)
    {
        if (godModeDuration != null)
        {
            StopCoroutine(godModeDuration);
        }
        godMode = true;
        updatePaletaCollision();
        godModeDuration = StartCoroutine(GodModeDurationRoutine(duration));
    }

    private IEnumerator GodModeDurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        godMode = false;
        updatePaletaCollision();
        godModeDuration = null; // Resetear la referencia de la coroutine

        Debug.Log("God Mode desactivado.");
    }
}