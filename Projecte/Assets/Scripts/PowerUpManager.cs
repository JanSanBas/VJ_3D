using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    // Nuevo: Clase Serializable para las probabilidades de Power-Up
    [System.Serializable]
    public class PowerUpDropChance
    {
        public PowerUpType type;
        [Range(0f, 1f)] // Asegura que el valor est� entre 0 y 1
        public float chance;
    }

    private List<GameObject> activePowerUpItems = new List<GameObject>();

    [Header("Power-Up General Settings")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private float dropChance = 0.1f; // Probabilidad general de que *cualquier* power-up dropee
    [SerializeField] private float powerUpSpeed = 2f;
    [SerializeField] private float powerUpDuration = 10f;
    [SerializeField] private float dropCooldown = 0.5f; // Tiempo entre drops de power-ups
    private float lastDropTime = 0f;

    [Header("Individual Power-Up Drop Chances")]
    [SerializeField] private List<PowerUpDropChance> individualDropChances; // Lista de probabilidades individuales

    [Header("PowerBall Settings")]
    [SerializeField] private Material powerBallMaterial;
    [SerializeField] private Material normalBallMaterial;

    [Header("Paddle Scale Settings")]
    [SerializeField] private float bigPaddleScale = 1.5f;
    [SerializeField] private float smallPaddleScale = 0.5f;

    [Header("Magnet Power-Up Settings")]
    [SerializeField] private int maxMagnetUses = 6;

    [Header("God Mode Settings")]
    [SerializeField] private float godModeDuration = 5f;

    private bool hasNextLevelPowerUpBeenDroppedThisLevel = false;
    [SerializeField] private float nextLevelDropChance = 0.1f; // Probabilidad espec�fica para NextLevel

    // --- Variables de estado y coroutines (reducidas para God Mode) ---
    private bool isPowerBallActive = false;
    private Coroutine powerBallCoroutine;

    private bool isBigPaddleActive = false;
    private Coroutine bigPaddleCoroutine;

    private bool isSmallPaddleActive = false;
    private Coroutine smallPaddleCoroutine;

    private bool isMagnetActive = false;
    private int currentMagnetUses;

    private ScriptBola ballScript;
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
        GameObject ball = GameObject.FindGameObjectWithTag("Bola");
        if (ball != null)
        {
            ballScript = ball.GetComponent<ScriptBola>();
        }
        else
        {
            Debug.LogError("Bola no encontrada. Aseg�rate de que tenga el tag 'Bola'.");
        }

        GameObject paddle = GameObject.FindGameObjectWithTag("Paleta");
        if (paddle != null)
        {
            paddleScript = paddle.GetComponent<ScriptPaleta>();
        }
        else
        {
            Debug.LogError("Paleta no encontrada. Aseg�rate de que tenga el tag 'Paleta'.");
        }
        hasNextLevelPowerUpBeenDroppedThisLevel = false;
    }

    public void TryDropPowerUp(Vector3 position)
    {
        
        activePowerUpItems.RemoveAll(item => item == null); // Limpiar la lista de objetos nulos

        if (Time.time < lastDropTime + dropCooldown)
        {
            return; // No dropear si no ha pasado el tiempo de cooldown
        }

        // L�gica para el Power-Up Next Level (prioritaria si se cumplen las condiciones)
        if (GameManager.Instance != null && GameManager.Instance.CanDropNextLevelPowerUp() && !hasNextLevelPowerUpBeenDroppedThisLevel)
        {
            if (Random.Range(0f, 1f) <= nextLevelDropChance)
            {
                DropPowerUp(position, PowerUpType.NextLevel);
                hasNextLevelPowerUpBeenDroppedThisLevel = true;
                Debug.Log("�Power-up Next Level dropeado!");
                return;
            }
        }

        // L�gica para otros Power-Ups
        if (Random.Range(0f, 1f) <= dropChance) // Probabilidad general de que *algo* dropee
        {
            lastDropTime = Time.time; // Actualizar el tiempo del �ltimo drop
            // Filtrar los power-ups normales (excluir NextLevel de esta selecci�n)
            List<PowerUpDropChance> normalPowerUps = new List<PowerUpDropChance>();
            foreach (var item in individualDropChances)
            {
                if (item.type != PowerUpType.NextLevel)
                {
                    normalPowerUps.Add(item);
                }
            }

            // Calcular el total de las "chances" para la normalizaci�n
            float totalChance = 0f;
            foreach (var item in normalPowerUps)
            {
                totalChance += item.chance;
            }

            // Si no hay power-ups normales definidos o su suma es 0, no dropear nada
            if (normalPowerUps.Count == 0 || totalChance == 0f)
            {
                return;
            }

            // Seleccionar un Power-Up basado en las probabilidades individuales
            float randomValue = Random.Range(0f, totalChance);
            float cumulativeChance = 0f;

            foreach (var item in normalPowerUps)
            {
                cumulativeChance += item.chance;
                if (randomValue <= cumulativeChance)
                {
                    DropPowerUp(position, item.type);
                    return;
                }
            }
        }
    }

    private void DropPowerUp(Vector3 position, PowerUpType type)
    {
        if (powerUpPrefab != null)
        {
            GameObject powerUp = Instantiate(powerUpPrefab, position, Quaternion.identity);

            PowerUpItem powerUpItem = powerUp.GetComponent<PowerUpItem>();
            if (powerUpItem != null)
            {
                powerUpItem.Initialize(type, powerUpSpeed);
                activePowerUpItems.Add(powerUp);
            }
        }
    }

    public void ActivatePowerUp(PowerUpType type)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.addScore(1000);
        }

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
            case PowerUpType.ExtraLife:
                ActivateExtraLife();
                break;
            case PowerUpType.NextLevel:
                ActivateNextLevel();
                break;
            case PowerUpType.GodMode:
                ActivateGodMode();
                break;
        }
    }

    // --- PowerBall Logic ---
    private void ActivatePowerBall()
    {
        if (isPowerBallActive)
        {
            if (powerBallCoroutine != null) StopCoroutine(powerBallCoroutine);
            powerBallCoroutine = StartCoroutine(DeactivatePowerBallAfterTime());
            Debug.Log("PowerBall duraci�n reiniciada!");
            return;
        }

        isPowerBallActive = true;
        if (ballScript != null)
        {
            ballScript.SetPowerBall(true);
            ballScript.GetComponent<Renderer>().material = powerBallMaterial;
        }

        powerBallCoroutine = StartCoroutine(DeactivatePowerBallAfterTime());
        Debug.Log("PowerBall activado!");
    }

    private IEnumerator DeactivatePowerBallAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);
        DeactivatePowerBall();
    }

    public void DeactivatePowerBall()
    {
        if (!isPowerBallActive) return;

        isPowerBallActive = false;
        if (ballScript != null)
        {
            ballScript.SetPowerBall(false);
            ballScript.GetComponent<Renderer>().material = normalBallMaterial;
        }
        Debug.Log("PowerBall desactivado!");
    }

    // --- Big Paddle Logic ---
    private void ActivateBigPaddle()
    {
        if (isSmallPaddleActive) DeactivateSmallPaddle();
        if (isBigPaddleActive)
        {
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
        if (isBigPaddleActive) DeactivateBigPaddle();
        if (isSmallPaddleActive)
        {
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
            DeactivatePowerBall();
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
        if (!isMagnetActive)
        {
            isMagnetActive = true;
            currentMagnetUses = maxMagnetUses;
            Debug.Log($"Magnet activado! Usos restantes: {currentMagnetUses}");

            if (ballScript != null)
            {
                ballScript.SetMagnetPowerUp(true);
            }
        }
        else
        {
            currentMagnetUses = maxMagnetUses;
            Debug.Log($"Magnet ya estaba activo, usos reiniciados: {currentMagnetUses}");
        }
    }

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
        if (!isMagnetActive) return;

        isMagnetActive = false;
        if (ballScript != null)
        {
            ballScript.SetMagnetPowerUp(false);
            ballScript.ReleaseBall();
        }
        Debug.Log("Magnet desactivado!");
    }

    // --- Extra Life Logic ---
    private void ActivateExtraLife()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.addLife();
            Debug.Log("�Power-up de vida extra activado!");
        }
    }

    // --- Next Level Logic ---
    private void ActivateNextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToNextLevel();
            Debug.Log("�Power-up Next Level activado!");
        }
    }

    // --- God Mode Logic (simplificado) ---
    private void ActivateGodMode()
    {
        if (ballScript != null)
        {
            ballScript.ActivateGodMode(godModeDuration);
        }
    }

    public bool IsPowerBallActive()
    {
        return isPowerBallActive;
    }

    public bool IsMagnetActive()
    {
        return isMagnetActive;
    }

    public bool HasNextLevelPowerUpBeenDropped()
    {
        return hasNextLevelPowerUpBeenDroppedThisLevel;
    }

    public void ResetPowerUps()
    {
        if (powerBallCoroutine != null) StopCoroutine(powerBallCoroutine);
        DeactivatePowerBall();

        if (bigPaddleCoroutine != null) StopCoroutine(bigPaddleCoroutine);
        DeactivateBigPaddle();

        if (smallPaddleCoroutine != null) StopCoroutine(smallPaddleCoroutine);
        DeactivateSmallPaddle();

        DeactivateMagnet();

        hasNextLevelPowerUpBeenDroppedThisLevel = false;
    }

    public void DestroyAllActivePowerUps()
    {
        activePowerUpItems.RemoveAll(item => item == null); // Limpiar la lista de objetos nulos

        foreach (GameObject powerUp in activePowerUpItems)
        {
            if (powerUp != null)
            {
                Destroy(powerUp);
            }
        }
        activePowerUpItems.Clear();
    }
}

public enum PowerUpType
{
    PowerBall,
    BigPaddle,
    SmallPaddle,
    NormalBall,
    Magnet,
    ExtraLife,
    NextLevel,
    GodMode
}