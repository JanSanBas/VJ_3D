using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [Header("Power-Up General Settings")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private float dropChance = 0.3f; // 30% de probabilidad
    [SerializeField] private float powerUpSpeed = 2f;
    [SerializeField] private float powerUpDuration = 10f; // Duraci�n general para power-ups temporales (PowerBall, Paddle Scale)

    [Header("PowerBall Settings")]
    [SerializeField] private Material powerBallMaterial;
    [SerializeField] private Material normalBallMaterial;

    [Header("Paddle Scale Settings")]
    [SerializeField] private float bigPaddleScale = 1.5f; // Factor de escala para paleta grande
    [SerializeField] private float smallPaddleScale = 0.5f; // Factor de escala para paleta peque�a

    [Header("Magnet Power-Up Settings")] // Nuevas configuraciones para el im�n
    [SerializeField] private int maxMagnetUses = 5; // N�mero m�ximo de veces que la bola se puede enganchar

    // Estados de power-ups activos y sus coroutines
    private bool isPowerBallActive = false;
    private Coroutine powerBallCoroutine;

    private bool isBigPaddleActive = false;
    private Coroutine bigPaddleCoroutine;

    private bool isSmallPaddleActive = false;
    private Coroutine smallPaddleCoroutine;

    private bool isMagnetActive = false; // Estado del im�n
    private int currentMagnetUses; // Usos restantes del im�n

    private ScriptBola ballScript; // Referencia a la bola principal
    private ScriptPaleta paddleScript;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Obtener referencia a la bola principal y la paleta
        GameObject ball = GameObject.FindGameObjectWithTag("Bola");
        if (ball != null)
        {
            ballScript = ball.GetComponent<ScriptBola>();
        }

        GameObject paddle = GameObject.FindGameObjectWithTag("Paleta");
        if (paddle != null)
        {
            paddleScript = paddle.GetComponent<ScriptPaleta>();
        }
    }

    public void TryDropPowerUp(Vector3 position)
    {
        if (Random.Range(0f, 1f) <= dropChance)
        {
            DropPowerUp(position);
        }
    }

    private void DropPowerUp(Vector3 position)
    {
        if (powerUpPrefab != null)
        {
            GameObject powerUp = Instantiate(powerUpPrefab, position, Quaternion.identity);

            // Asignar tipo de power-up aleatoriamente entre los tipos disponibles
            PowerUpType[] availablePowerUpTypes = { PowerUpType.PowerBall, PowerUpType.BigPaddle, PowerUpType.SmallPaddle, PowerUpType.NormalBall, PowerUpType.Magnet };
            PowerUpType type = availablePowerUpTypes[Random.Range(0, availablePowerUpTypes.Length)];

            PowerUpItem powerUpItem = powerUp.GetComponent<PowerUpItem>();
            if (powerUpItem != null)
            {
                powerUpItem.Initialize(type, powerUpSpeed);
            }
        }
    }

    public void ActivatePowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.PowerBall:
                ActivatePowerBall();
                break;
            case PowerUpType.BigPaddle:
                ActivateBigPaddle();
                break;
            case PowerUpType.SmallPaddle:
                ActivateSmallPaddle();
                break;
            case PowerUpType.NormalBall:
                ActivateNormalBall();
                break;
            case PowerUpType.Magnet:
                ActivateMagnet();
                break;
        }
    }

    // --- PowerBall Logic ---
    private void ActivatePowerBall()
    {
        if (isPowerBallActive)
        {
            // Si ya est� activo, reinicia la duraci�n
            if (powerBallCoroutine != null) StopCoroutine(powerBallCoroutine);
            powerBallCoroutine = StartCoroutine(DeactivatePowerBallAfterTime());
            Debug.Log("PowerBall duraci�n reiniciada!");
            return;
        }

        isPowerBallActive = true;
        if (ballScript != null)
        {
            ballScript.SetPowerBall(true); // Informar a la bola que PowerBall est� activo
            ballScript.GetComponent<Renderer>().material = powerBallMaterial; // Cambiar material
        }

        powerBallCoroutine = StartCoroutine(DeactivatePowerBallAfterTime());
        Debug.Log("PowerBall activado!");
    }

    private IEnumerator DeactivatePowerBallAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);
        DeactivatePowerBall();
    }

    private void DeactivatePowerBall()
    {
        if (!isPowerBallActive) return;

        isPowerBallActive = false;
        if (ballScript != null)
        {
            ballScript.SetPowerBall(false); // Informar a la bola que PowerBall se desactiva
            ballScript.GetComponent<Renderer>().material = normalBallMaterial; // Volver al material normal
        }
        Debug.Log("PowerBall desactivado!");
    }

    // --- Big Paddle Logic ---
    private void ActivateBigPaddle()
    {
        if (isSmallPaddleActive) DeactivateSmallPaddle(); // Desactivar peque�o si grande se activa
        if (isBigPaddleActive)
        {
            // Si ya est� activo, reinicia la duraci�n
            if (bigPaddleCoroutine != null) StopCoroutine(bigPaddleCoroutine);
            bigPaddleCoroutine = StartCoroutine(DeactivateBigPaddleAfterTime());
            Debug.Log("Big Paddle duraci�n reiniciada!");
            return;
        }

        isBigPaddleActive = true;

        if (paddleScript != null)
        {
            paddleScript.SetPaddleScale(bigPaddleScale);
        }

        bigPaddleCoroutine = StartCoroutine(DeactivateBigPaddleAfterTime());
        Debug.Log("Big Paddle activado!");
    }

    private IEnumerator DeactivateBigPaddleAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);
        DeactivateBigPaddle();
    }

    private void DeactivateBigPaddle()
    {
        if (!isBigPaddleActive) return;

        isBigPaddleActive = false;
        if (paddleScript != null)
        {
            paddleScript.ResetPaddleScale();
        }
        Debug.Log("Big Paddle desactivado!");
    }

    // --- Small Paddle Logic ---
    private void ActivateSmallPaddle()
    {
        if (isBigPaddleActive) DeactivateBigPaddle(); // Desactivar grande si peque�o se activa
        if (isSmallPaddleActive)
        {
            // Si ya est� activo, reinicia la duraci�n
            if (smallPaddleCoroutine != null) StopCoroutine(smallPaddleCoroutine);
            smallPaddleCoroutine = StartCoroutine(DeactivateSmallPaddleAfterTime());
            Debug.Log("Small Paddle duraci�n reiniciada!");
            return;
        }

        isSmallPaddleActive = true;

        if (paddleScript != null)
        {
            paddleScript.SetPaddleScale(smallPaddleScale);
        }

        smallPaddleCoroutine = StartCoroutine(DeactivateSmallPaddleAfterTime());
        Debug.Log("Small Paddle activado!");
    }

    private IEnumerator DeactivateSmallPaddleAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);
        DeactivateSmallPaddle();
    }

    private void DeactivateSmallPaddle()
    {
        if (!isSmallPaddleActive) return;

        isSmallPaddleActive = false;
        if (paddleScript != null)
        {
            paddleScript.ResetPaddleScale();
        }
        Debug.Log("Small Paddle desactivado!");
    }

    // --- Normal Ball Logic ---
    private void ActivateNormalBall()
    {
        if (isPowerBallActive)
        {
            DeactivatePowerBall(); // Desactiva PowerBall, lo que restablece el material a normalBallMaterial
            Debug.Log("Normal Ball activado (PowerBall desactivado)!");
        }
        else
        {
            Debug.Log("Normal Ball recogido, pero PowerBall no estaba activo.");
        }
    }

    // --- Magnet Logic ---
    private void ActivateMagnet()
    {
        if (!isMagnetActive) // Activar solo si no est� ya activo
        {
            isMagnetActive = true;
            currentMagnetUses = maxMagnetUses; // Resetear usos al activar
            Debug.Log($"Magnet activado! Usos restantes: {currentMagnetUses}");

            if (ballScript != null)
            {
                ballScript.SetMagnetPowerUp(true); // Informar a la bola que el im�n est� activo
            }
        }
        else
        {
            // Si ya est� activo, puedes optar por reiniciar los usos o sumarlos.
            // Aqu�, lo reiniciaremos para que el jugador siempre obtenga el m�ximo de usos.
            currentMagnetUses = maxMagnetUses;
            Debug.Log($"Magnet ya estaba activo, usos reiniciados: {currentMagnetUses}");
        }
    }

    // M�todo para que la bola informe que us� un enganche del im�n
    public void UseMagnetCharge()
    {
        if (isMagnetActive)
        {
            currentMagnetUses--;
            Debug.Log($"Uso de im�n restante: {currentMagnetUses}");
            if (currentMagnetUses <= 0)
            {
                DeactivateMagnet();
            }
        }
    }

    public void DeactivateMagnet()
    {
        if (!isMagnetActive) return; // Ya est� desactivado

        isMagnetActive = false;
        if (ballScript != null)
        {
            ballScript.SetMagnetPowerUp(false); // Informar a la bola que el im�n se desactiv�
            ballScript.ReleaseBall(); // Asegurarse de liberar la bola si est� enganchada
        }
        Debug.Log("Magnet desactivado!");
    }

    public bool IsPowerBallActive()
    {
        return isPowerBallActive;
    }

    public bool IsMagnetActive() // Nuevo m�todo para que otros scripts consulten el estado del im�n
    {
        return isMagnetActive;
    }

    // M�todo para reiniciar power-ups cuando se reinicia el juego (importante para vidas)
    public void ResetPowerUps()
    {
        if (powerBallCoroutine != null) StopCoroutine(powerBallCoroutine);
        DeactivatePowerBall();

        if (bigPaddleCoroutine != null) StopCoroutine(bigPaddleCoroutine);
        DeactivateBigPaddle();

        if (smallPaddleCoroutine != null) StopCoroutine(smallPaddleCoroutine);
        DeactivateSmallPaddle();

        DeactivateMagnet(); // Asegurarse de desactivar el im�n tambi�n
    }
}

public enum PowerUpType
{
    PowerBall,
    BigPaddle,
    SmallPaddle,
    NormalBall,
    Magnet // �Nuevo tipo!
}