using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptBola : MonoBehaviour
{
    public bool gameStarted;
    public bool gameFinished;

    public bool godMode;

    public float speed = 10f;
    public float maxBounceAngle = 75f;

    private float lastBounceTime;
    private float bounceCooldown = 0.03f; // Cooldown time in seconds

    private Vector3 direction;
    private Rigidbody rb;

    private Transform paleta;

    private bool isPowerBallActive = false; // Estado del PowerBall en la bola

    // --- Nuevas variables para el im�n ---
    private bool isMagnetPowerUpActive = false;
    private bool isBallAttached = false;
    private Vector3 offsetFromPaddle;

    // --- Nuevas variables para la liberaci�n controlada ---
    private bool isReleasing = false; // Nuevo: indica si la bola est� en proceso de liberaci�n
    [SerializeField] private float releaseImmunityDuration = 0.1f; // Nuevo: tiempo que ignora colisiones con la paleta al liberarse

    [SerializeField] private Coroutine godModeDuration;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0.68f, -7.74f);
        gameStarted = false;
        gameFinished = false;
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
        if (gameFinished || !GameManager.Instance.controlHabilitado)
        {
            if (paleta != null)
            {
                Vector3 posPaleta = paleta.position;
                transform.position = new Vector3(posPaleta.x, transform.position.y, posPaleta.z + 0.75f);
            }
            return;
        }
        if (!gameStarted)
        {
            if (paleta != null)
            {
               transform.position = new Vector3(paleta.position.x, transform.position.y, paleta.position.z + 0.76f);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                gameStarted = true;
                GameManager.Instance.HabilitarControl();
                ApplyVelocity();
            }
        }
        else if (isBallAttached) // Si la bola est� enganchada por el im�n
        {
            transform.position = paleta.position + offsetFromPaddle;
            rb.velocity = Vector3.zero; // Asegurar que no se mueva por su cuenta

            if (Input.GetKeyDown(KeyCode.Space)) {
                ReleaseBall(); // Liberar la bola
            }
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
                GameManager.Instance.reduceLives(); // Llama al GameManager para reducir una vida
                gameRestart(); // Reinicia la posici�n de la bola para la siguiente vida
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
        if (isReleasing && collision.gameObject.CompareTag("Paleta"))
        {
            return;
        }

        if (gameStarted == false)
            return;

        if (Time.time - lastBounceTime < bounceCooldown)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Rocket"))
        {

            return;
        }

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
        // Si el imán está activo y la bola no está ya enganchada, engancharla
        if (isMagnetPowerUpActive && !isBallAttached)
        {
            AttachBall(collision);
            PowerUpManager.Instance.UseMagnetCharge(); // Notificar al PowerUpManager que se usó una carga
            return;
        }

        ContactPoint contact = collision.contacts[0];
        Vector3 paddleCenter = collision.gameObject.transform.position;

        // Calcular la posición del impacto relativa al centro de la paleta
        float hitPointX = contact.point.x - paddleCenter.x;
        // Obtener el ancho de la paleta dinámicamente, considerando su escala
        float paddleWidth = collision.collider.bounds.size.x;
        // Normalizar la posición del impacto (-1 para el extremo izquierdo, 0 para el centro, 1 para el extremo derecho)
        float normalizedHitPointX = hitPointX / (paddleWidth / 2f);

        // Calcular el ángulo de rebote. Multiplicamos la posición normalizada por el ángulo máximo.
        // Esto significa que si golpea en el centro (0), el ángulo será 0 (recto).
        // Si golpea en el extremo (1 o -1), el ángulo será 'maxBounceAngle' o '-maxBounceAngle'.
        float bounceAngle = normalizedHitPointX * maxBounceAngle;

        // Establecer la nueva dirección de la bola usando el ángulo calculado.
        // Quaternion.Euler(0, bounceAngle, 0) rota el vector 'Vector3.forward' alrededor del eje Y.
        direction = Quaternion.Euler(0, bounceAngle, 0) * Vector3.forward;

        // Asegurarse de que la bola siempre se mueva hacia adelante en el eje Z (hacia arriba en el juego)
        if (direction.z < 0)
        {
            direction.z *= -1;
        }

        // Normalizar la dirección para asegurar que la magnitud del vector sea 1
        direction.Normalize();

        ApplyVelocity(); // Aplicar la velocidad en la nueva dirección
        lastBounceTime = Time.time; // Actualizar el tiempo del último rebote

        GameManager.Instance.OnBallHitsPaleta(); // Notificar al GameManager
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

        GameManager.Instance.OnBallHitsPaleta();
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

            if (!isPowerBallActive) // Si PowerBall NO est� activo, la bola rebota.
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 normal = contact.normal;
                direction = Vector3.Reflect(direction, normal).normalized;
                float randomAngleOffset = Random.Range(-5f, 5f);
                direction = Quaternion.Euler(0, randomAngleOffset, 0) * direction;
                direction.Normalize();
             }
            collision.gameObject.GetComponent<ScriptCube>().collisionWithBall();
            ApplyVelocity();
            lastBounceTime = Time.time;
        }
    }



    public void SetPowerBall(bool active)
    {
        isPowerBallActive = active;
    }

    public void SetMagnetPowerUp(bool active) // M�todo para establecer el estado del im�n
    {
        isMagnetPowerUpActive = active;
        if (!active && isBallAttached) // Si el im�n se desactiva y la bola est� enganchada, liberarla
        {
            ReleaseBall();
        }
    }

    private void AttachBall(Collision collision) // L�gica para enganchar la bola
    {
        isBallAttached = true;
        rb.isKinematic = true; // Desactivar f�sica para que no se mueva
        rb.velocity = Vector3.zero; // Asegurarse de que se detiene

        // Calcular el offset para mantener la posici�n relativa a la paleta
        offsetFromPaddle = transform.position - paleta.position;

        Debug.Log("Bola enganchada a la paleta!");
    }

    public void ReleaseBall() // L�gica para liberar la bola
    {
        if (!isBallAttached) return; // Si no est� enganchada, no hacer nada

        isBallAttached = false;
        rb.isKinematic = false; // Reactivar f�sica

        // Iniciar el per�odo de inmunidad
        StartCoroutine(ReleaseImmunityRoutine());

        // La direcci�n de liberaci�n puede ser un poco m�s din�mica.
        // Podr�as usar el movimiento de la paleta, o simplemente el centro de la paleta.
        // Aqu� vamos a usar la l�gica de rebote de la paleta para darle una direcci�n inicial.
        // Para esto necesitamos simular los puntos de contacto, o hacer un rebote simple hacia arriba.
        // Para un inicio consistente, usaremos una direcci�n ligeramente aleatoria o basada en la posici�n actual en la paleta.

        // Opci�n 2: Usar la l�gica de rebote de la paleta (m�s realista)
        // Necesitamos calcular el punto de impacto como si acabara de chocar.
        // Una aproximaci�n simple es usar la posici�n x relativa de la bola sobre la paleta.
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

    private IEnumerator ReleaseImmunityRoutine() // Coroutine para inmunidad tras liberaci�n
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
        isMagnetPowerUpActive = false; // Asegurarse de que el im�n se desactiva en la bola
        isBallAttached = false; // Asegurarse de que no est� enganchada
        rb.isKinematic = false; // Reactivar la f�sica si estaba desactivada

        // Asegurarse de que el estado de liberaci�n tambi�n se resetee
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

