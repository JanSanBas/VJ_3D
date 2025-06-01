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

    [System.Serializable]
    public class PowerUpPrefabEntry
    {
        public PowerUpType type;
        public GameObject prefab;
        public Vector3 rotationOffset = Vector3.zero;
    }


    [Header("Power-Up General Settings")]
    [SerializeField] private float dropChance = 0.1f; // Probabilidad general de que *cualquier* power-up dropee
    [SerializeField] private float powerUpSpeed = 2f;
    [SerializeField] private float powerUpDuration = 10f;
    [SerializeField] private float dropCooldown = 0.5f; // Tiempo entre drops de power-ups
    private float lastDropTime = 0f;

    [Header("Power-Up Prefabs")]
    [SerializeField] private List<PowerUpPrefabEntry> powerUpPrefabs;

    [Header("Individual Power-Up Drop Chances")]
    [SerializeField] private List<PowerUpDropChance> individualDropChances; // Lista de probabilidades individuales

    [Header("PowerBall Settings")]
    [SerializeField] private Material powerBallMaterial;
    [SerializeField] private Material normalBallMaterial;

    [Header("Paddle Scale Settings")]
    [SerializeField] private float bigPaddleScale = 1.5f;
    [SerializeField] private float smallPaddleScale = 0.5f;

    [Header("Magnet Power-Up Settings")]
    [SerializeField] private int maxMagnetUses = 5;

    [Header("God Mode Settings")]
    [SerializeField] private float godModeDuration = 5f;

    private bool hasNextLevelPowerUpBeenDroppedThisLevel = false;
    [SerializeField] private float nextLevelDropChance = 0.1f; // Probabilidad espec�fica para NextLevel

    [Header("Rocket Settings")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private int maxRocketSalvos = 3; // N�mero de r�fagas por power-up
    [SerializeField] private float rocketFireInterval = 2f; // Tiempo entre r�fagas

    private bool isRocketActive = false;
    private int currentRocketSalvos;
    private Coroutine rocketCoroutine;

    private List<GameObject> activePowerUpItems = new List<GameObject>();

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
    public ScriptUI ui;

    public float tPowerBall;
    public float tBigPaddle;
    public float tSmallPaddle;

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
        GameObject uiGO = GameObject.Find("UI");
        if (uiGO != null)
        {
            ui = uiGO.GetComponent<ScriptUI>();
        }
        tPowerBall = 0;
        tBigPaddle = 0;
        tSmallPaddle = 0;
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
        GameObject prefabToInstantiate = null;
        Vector3 currentRotationOffset = Vector3.zero; // Offset de rotaci�n por defecto

        // Buscar el prefab correspondiente al tipo
        foreach (var entry in powerUpPrefabs)
        {
            if (entry.type == type)
            {
                prefabToInstantiate = entry.prefab;
                currentRotationOffset = entry.rotationOffset; // Obtener el offset de rotaci�n del prefab
                break;
            }
        }

        if (prefabToInstantiate != null)
        {

            Vector3 spawnPosition = position;
            if (type == PowerUpType.PowerBall || type == PowerUpType.SmallPaddle || type == PowerUpType.BigPaddle || type == PowerUpType.NormalBall) // Añade aquí los tipos que necesiten ajuste
            {
                spawnPosition.y += 0.8f;
            }
            else if (type == PowerUpType.ExtraLife || type == PowerUpType.Magnet)
            {
                spawnPosition.y += 0.4f; // Ajuste para evitar clipping al dropear Power-Ups
            }
            else if (type == PowerUpType.GodMode || type == PowerUpType.NextLevel)
            {
                spawnPosition.y += 0.75f; // Ajuste para evitar clipping al dropear GodMode
            }

            GameObject powerUp = Instantiate(prefabToInstantiate, spawnPosition, Quaternion.identity); // prefabToInstantiate.transform.rotation

            powerUp.transform.localEulerAngles = currentRotationOffset;

            PowerUpItem powerUpItem = powerUp.GetComponent<PowerUpItem>();
            if (powerUpItem != null)
            {
                // Inicializar el PowerUpItem con su tipo y velocidad
                powerUpItem.Initialize(type, powerUpSpeed);
                activePowerUpItems.Add(powerUp);
            }
            else
            {
                Debug.LogWarning($"El prefab para el PowerUpType {type} no tiene un componente PowerUpItem.");
                Destroy(powerUp); // Destruir el objeto si no tiene el script correcto
            }
        }
        else
        {
            Debug.LogWarning($"No se encontr� un prefab asignado para el PowerUpType: {type}");
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
        tPowerBall = powerUpDuration;
        while (tPowerBall > 0)
        {
            if (ui != null) ui.SetTiempoPowerBall(tPowerBall);
            yield return null;
            tPowerBall -= Time.deltaTime;
        }

        if (ui != null) ui.SetTiempoPowerBall(0);
        DeactivatePowerBall();
    }

    public void DeactivatePowerBall()
    {
        tPowerBall = 0;
        ui.SetTiempoPowerBall(0);
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
        tBigPaddle = powerUpDuration;
        while (tBigPaddle > 0)
        {
            if (ui != null) ui.SetTiempoAgrandar(tBigPaddle);
            yield return null;
            tBigPaddle -= Time.deltaTime;
        }

        if (ui != null) ui.SetTiempoAgrandar(0);
        DeactivateBigPaddle();
    }

    private void DeactivateBigPaddle()
    {
        tBigPaddle = 0;
        ui.SetTiempoAgrandar(0);
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
        tSmallPaddle = powerUpDuration;
        while (tSmallPaddle > 0)
        {
            if (ui != null) ui.SetTiempoReducir(tSmallPaddle);
            yield return null;
            tSmallPaddle -= Time.deltaTime;
        }

        if (ui != null) ui.SetTiempoReducir(0);
        DeactivateSmallPaddle();
    }

    private void DeactivateSmallPaddle()
    {
        tSmallPaddle = 0;
        ui.SetTiempoReducir(0);
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
        ui.SetTiempoPowerBall(0); // Asegurar que el UI de PowerBall se actualice
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
        // Forzar la desactivaci�n primero para limpiar el estado
        if (isMagnetActive)
        {
            DeactivateMagnet();
        }

        // Ahora activar limpiamente
        isMagnetActive = true;
        if (ballScript.isBallAttached)
        {
            currentMagnetUses = maxMagnetUses - 1; // Si la bola ya est�� unida, restar un uso
        }
        else currentMagnetUses = maxMagnetUses;

        Debug.Log($"Magnet activado! Usos restantes: {currentMagnetUses}");

        if (ballScript != null)
        {
            ballScript.SetMagnetPowerUp(true);
        }
        if (ui != null) ui.SetUsosIman(currentMagnetUses);
    }

    public void UseMagnetCharge()
    {
        if (isMagnetActive)
        {
            currentMagnetUses--;
            if (ui != null) ui.SetUsosIman(currentMagnetUses);
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
        currentMagnetUses = 0; // Reset del contador

        if (ballScript != null)
        {
            ballScript.SetMagnetPowerUp(false);
            //ballScript.ReleaseBall(); // Asegurar que la bola se libere
        }

        Debug.Log("Magnet desactivado completamente!");
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
        DeactivateRocket();

        DestroyAllActivePowerUps();

        if (ui != null)
        {
            ui.SetTiempoPowerBall(0);
            ui.SetTiempoAgrandar(0);
            ui.SetTiempoReducir(0);
            ui.SetTiempoGodMode(0);
            ui.SetUsosIman(0);
            ui.SetUsosCohetes(0);
        }

        hasNextLevelPowerUpBeenDroppedThisLevel = false;
    }

    // --- Rocket Logic ---
    private void ActivateRocket()
    {
        if (isRocketActive)
        {
            // Si ya est� activo, reiniciar el conteo
            if (rocketCoroutine != null) StopCoroutine(rocketCoroutine);
            currentRocketSalvos = maxRocketSalvos;
            rocketCoroutine = StartCoroutine(RocketCoroutine());
            Debug.Log("Rocket power-up reiniciado!");
            return;
        }

        isRocketActive = true;
        currentRocketSalvos = maxRocketSalvos;
        if (ui != null) ui.SetUsosCohetes(currentRocketSalvos);
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
            if (ui != null) ui.SetUsosCohetes(currentRocketSalvos);
        }

        DeactivateRocket();
    }

    private void FireRockets()
    {
        if (paddleScript == null || rocketPrefab == null) return;

        Vector3 paddlePosition = paddleScript.transform.position;

        // Obtener los l�mites reales de la paleta usando su Renderer
        Renderer paddleRenderer = paddleScript.GetComponent<Renderer>();
        if (paddleRenderer != null)
        {
            // Usar los bounds del renderer para obtener las posiciones exactas de los bordes
            Bounds paddleBounds = paddleRenderer.bounds;

            // Posiciones exactas en los bordes laterales de la paleta
            Vector3 leftRocketPos = new Vector3(paddleBounds.min.x, paddlePosition.y + 0.2f, paddlePosition.z + 0.3f);
            Vector3 rightRocketPos = new Vector3(paddleBounds.max.x, paddlePosition.y + 0.2f, paddlePosition.z + 0.3f);

            // Crear los cohetes en las posiciones exactas de los bordes
            Instantiate(rocketPrefab, leftRocketPos, rocketPrefab.transform.rotation);
            Instantiate(rocketPrefab, rightRocketPos, rocketPrefab.transform.rotation);

            Debug.Log($"Cohetes disparados desde bordes de paleta! Escala actual: {paddleScript.transform.localScale.x}. R�fagas restantes: {currentRocketSalvos - 1}");
        }
        else
        {
            // Fallback: si no hay renderer, usar el m�todo anterior como respaldo
            Debug.LogWarning("No se encontr� Renderer en la paleta, usando c�lculo aproximado");
            float paddleWidth = paddleScript.transform.localScale.x * 2f;
            Vector3 leftRocketPos = paddlePosition + new Vector3(-paddleWidth / 2f, 0.2f, 0.3f);
            Vector3 rightRocketPos = paddlePosition + new Vector3(paddleWidth / 2f, 0.2f, 0.3f);

            Instantiate(rocketPrefab, leftRocketPos, Quaternion.identity);
            Instantiate(rocketPrefab, rightRocketPos, Quaternion.identity);
            Debug.Log($"Cohetes disparados! R�fagas restantes: {currentRocketSalvos - 1}");
        }
    }

    private void DeactivateRocket()
    {
        isRocketActive = false;
        if (rocketCoroutine != null)
        {
            if (ui != null) ui.SetUsosCohetes(0);
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