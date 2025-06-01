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
        [Range(0f, 1f)] // Asegura que el valor estï¿½ entre 0 y 1
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
    [SerializeField] private float nextLevelDropChance = 0.1f; // Probabilidad especï¿½fica para NextLevel

    [Header("Rocket Settings")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private int maxRocketSalvos = 3; // Número de ráfagas por power-up
    [SerializeField] private float rocketFireInterval = 2f; // Tiempo entre ráfagas
    [SerializeField] private float rocketOffset = 0.5f; // Distancia desde los extremos de la paleta

    private bool isRocketActive = false;
    private int currentRocketSalvos;
    private Coroutine rocketCoroutine;

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
            Debug.LogError("Bola no encontrada. Asegï¿½rate de que tenga el tag 'Bola'.");
        }

        GameObject paddle = GameObject.FindGameObjectWithTag("Paleta");
        if (paddle != null)
        {
            paddleScript = paddle.GetComponent<ScriptPaleta>();
        }
        else
        {
            Debug.LogError("Paleta no encontrada. Asegï¿½rate de que tenga el tag 'Paleta'.");
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

        // Lï¿½gica para el Power-Up Next Level (prioritaria si se cumplen las condiciones)
        if (GameManager.Instance != null && GameManager.Instance.CanDropNextLevelPowerUp() && !hasNextLevelPowerUpBeenDroppedThisLevel)
        {
            if (Random.Range(0f, 1f) <= nextLevelDropChance)
            {
                DropPowerUp(position, PowerUpType.NextLevel);
                hasNextLevelPowerUpBeenDroppedThisLevel = true;
                Debug.Log("ï¿½Power-up Next Level dropeado!");
                return;
            }
        }

        // Lï¿½gica para otros Power-Ups
        if (Random.Range(0f, 1f) <= dropChance) // Probabilidad general de que *algo* dropee
        {
            lastDropTime = Time.time; // Actualizar el tiempo del ï¿½ltimo drop
            // Filtrar los power-ups normales (excluir NextLevel de esta selecciï¿½n)
            List<PowerUpDropChance> normalPowerUps = new List<PowerUpDropChance>();
            foreach (var item in individualDropChances)
            {
                if (item.type != PowerUpType.NextLevel)
                {
                    normalPowerUps.Add(item);
                }
            }

            // Calcular el total de las "chances" para la normalizaciï¿½n
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
            GameManager.Instance.addScore(1000, "PowerUp");
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
            case PowerUpType.Rocket:
                ActivateRocket();
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
            Debug.Log("PowerBall duraciï¿½n reiniciada!");
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
            Debug.Log("Big Paddle duraciï¿½n reiniciada!");
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
            Debug.Log("Small Paddle duraciï¿½n reiniciada!");
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
        // Forzar la desactivación primero para limpiar el estado
        if (isMagnetActive)
        {
            DeactivateMagnet();
        }

        // Ahora activar limpiamente
        isMagnetActive = true;
        currentMagnetUses = maxMagnetUses;

        Debug.Log($"Magnet activado! Usos restantes: {currentMagnetUses}");

        if (ballScript != null)
        {
            ballScript.SetMagnetPowerUp(true);
        }
    }

    public void UseMagnetCharge()
    {
        if (isMagnetActive)
        {
            currentMagnetUses--;
            Debug.Log($"Uso de imï¿½n restante: {currentMagnetUses}");
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
        currentMagnetUses = 0; // Reset del contador

        if (ballScript != null)
        {
            ballScript.SetMagnetPowerUp(false);
            ballScript.ReleaseBall(); // Asegurar que la bola se libere
        }

        Debug.Log("Magnet desactivado completamente!");
    }

    // --- Extra Life Logic ---
    private void ActivateExtraLife()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.addLife();
            Debug.Log("ï¿½Power-up de vida extra activado!");
        }
    }

    // --- Next Level Logic ---
    private void ActivateNextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToNextLevel();
            Debug.Log("ï¿½Power-up Next Level activado!");
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
        DeactivateRocket();

        DestroyAllActivePowerUps();

        hasNextLevelPowerUpBeenDroppedThisLevel = false;
    }

    // --- Rocket Logic ---
    private void ActivateRocket()
    {
        if (isRocketActive)
        {
            // Si ya está activo, reiniciar el conteo
            if (rocketCoroutine != null) StopCoroutine(rocketCoroutine);
            currentRocketSalvos = maxRocketSalvos;
            rocketCoroutine = StartCoroutine(RocketCoroutine());
            Debug.Log("Rocket power-up reiniciado!");
            return;
        }

        isRocketActive = true;
        currentRocketSalvos = maxRocketSalvos;
        rocketCoroutine = StartCoroutine(RocketCoroutine());
        Debug.Log("Rocket power-up activado!");
    }

    private IEnumerator RocketCoroutine()
    {
        while (currentRocketSalvos > 0 && paddleScript != null)
        {
            // Disparar cohetes desde ambos extremos de la paleta
            FireRockets();
            currentRocketSalvos--;

            if (currentRocketSalvos > 0)
            {
                yield return new WaitForSeconds(rocketFireInterval);
            }
        }

        DeactivateRocket();
    }

    private void FireRockets()
    {
        if (paddleScript == null || rocketPrefab == null) return;

        Vector3 paddlePosition = paddleScript.transform.position;

        // Obtener los límites reales de la paleta usando su Renderer
        Renderer paddleRenderer = paddleScript.GetComponent<Renderer>();
        if (paddleRenderer != null)
        {
            // Usar los bounds del renderer para obtener las posiciones exactas de los bordes
            Bounds paddleBounds = paddleRenderer.bounds;

            // Posiciones exactas en los bordes laterales de la paleta
            Vector3 leftRocketPos = new Vector3(paddleBounds.min.x, paddlePosition.y + 0.2f, paddlePosition.z + 0.3f);
            Vector3 rightRocketPos = new Vector3(paddleBounds.max.x, paddlePosition.y + 0.2f, paddlePosition.z + 0.3f);

            // Crear los cohetes en las posiciones exactas de los bordes
            Instantiate(rocketPrefab, leftRocketPos, Quaternion.identity);
            Instantiate(rocketPrefab, rightRocketPos, Quaternion.identity);

            Debug.Log($"Cohetes disparados desde bordes de paleta! Escala actual: {paddleScript.transform.localScale.x}. Ráfagas restantes: {currentRocketSalvos - 1}");
        }
        else
        {
            // Fallback: si no hay renderer, usar el método anterior como respaldo
            Debug.LogWarning("No se encontró Renderer en la paleta, usando cálculo aproximado");
            float paddleWidth = paddleScript.transform.localScale.x * 2f;
            Vector3 leftRocketPos = paddlePosition + new Vector3(-paddleWidth / 2f, 0.2f, 0.3f);
            Vector3 rightRocketPos = paddlePosition + new Vector3(paddleWidth / 2f, 0.2f, 0.3f);

            Instantiate(rocketPrefab, leftRocketPos, Quaternion.identity);
            Instantiate(rocketPrefab, rightRocketPos, Quaternion.identity);
            Debug.Log($"Cohetes disparados! Ráfagas restantes: {currentRocketSalvos - 1}");
        }
    }

    private void DeactivateRocket()
    {
        isRocketActive = false;
        if (rocketCoroutine != null)
        {
            StopCoroutine(rocketCoroutine);
            rocketCoroutine = null;
        }
        Debug.Log("Rocket power-up desactivado!");
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
    GodMode,
    Rocket
}