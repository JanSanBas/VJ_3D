using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    public AudioSource destroyCube;
    public AudioSource fail;
    public AudioSource hitPaleta;

    public static GameManager Instance;

    [Header("Pantalla de derrota")]
    [SerializeField] private CanvasGroup panelDerrota;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private RectTransform textoDerrota; // para escalar el texto

    // Singleton estatico para datos persistentes
    private static int persistentScore = 0;
    private static int persistentLives = 3;
    private static int persistentLevel = 1;
    private static bool isInitialized = false;

    // Variables de instancia para la escena actual
    private int score;
    private int lives;
    private int level;

    [SerializeField] private ScriptPaleta scriptPaleta;
    [SerializeField] private ScriptBola scriptBola;

    // Referencias a la UI
    [SerializeField] private TextMeshProUGUI numeroPuntosText;
    [SerializeField] private TextMeshProUGUI numeroVidasText;
    [SerializeField] private TextMeshProUGUI numeroNivelText;

    [Header("Derrota")]
    [SerializeField] private ScriptDerrota derrotaUI;
    [SerializeField] private AudioSource musicaJuego;
    [SerializeField] private AudioSource musicaDerrota;


    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;

        // Inicializar si es la primera vez
        if (!isInitialized)
        {
            persistentScore = 0;
            persistentLives = 3;
            isInitialized = true;
        }

        // Cargar los datos persistentes
        score = persistentScore;
        lives = persistentLives;
        level = persistentLevel;

        determineLevelNum(); // Determinar el nivel basado en la escena actual
    }

    private void Start()
    {
        // Obtener referencias a los elementos de UI y actualizar
        getUIElements();
        updateUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.childCount <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
        {
            loadScene("Nivel1");
        }
        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            loadScene("Nivel2");
        }
        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyUp(KeyCode.Keypad3))
        {
            loadScene("Nivel3");
        }
        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.Keypad4))
        {
            loadScene("Nivel4");
        }
        if (Input.GetKeyUp(KeyCode.Alpha5) || Input.GetKeyUp(KeyCode.Keypad5))
        {
            loadScene("Nivel5");
        }
    }

    public void addScore(int points)
    {
        if (destroyCube != null)
            destroyCube.Play();
        score += points;
        persistentScore = score; // Guardar en datos persistentes
        updateUI();
    }

    void updateUI()
    {
        if (numeroPuntosText != null)
        {
            numeroPuntosText.text = score.ToString();
        }

        if (numeroVidasText != null)
        {
            numeroVidasText.text = lives.ToString();
        }

        if (numeroNivelText != null)
        {
            numeroNivelText.text = level.ToString();
        }
    }

    public void setLevel(int level)
    {
        this.level = level;
        persistentLevel = level; // Guardar en datos persistentes
        updateUI();
    }

    public void reduceLives()
    {
        if (fail != null)
            fail.Play();

        lives--;
        persistentLives = lives;

        if (lives <= 0)
        {
            updateUI();
            GuardarPuntuacionMaxima(score);
            StartCoroutine(FinDelJuego());
        }
        else
        {
            updateUI();
        }
    }

    public void loadScene(string scene)
    {
        // Guardar datos antes de cambiar escena
        persistentScore = score;
        persistentLives = lives;

        // Cargar la nueva escena
        SceneManager.LoadScene(scene);
    }

    private void getUIElements()
    {
        // Buscar los elementos de UI en la escena actual
        GameObject puntosObj = GameObject.Find("NumeroPuntos");
        if (puntosObj != null)
        {
            numeroPuntosText = puntosObj.GetComponent<TextMeshProUGUI>();
        }

        GameObject vidasObj = GameObject.Find("NumeroVidas");
        if (vidasObj != null)
        {
            numeroVidasText = vidasObj.GetComponent<TextMeshProUGUI>();
        }

        GameObject nivelObj = GameObject.Find("NumeroNivel");
        if (nivelObj != null)
        {
            numeroNivelText = nivelObj.GetComponent<TextMeshProUGUI>();
        }
    }

    private void determineLevelNum()
    {
        level = (SceneManager.GetActiveScene().buildIndex) % 6;
        persistentLevel = level; // Guardar en datos persistentes
        Debug.Log("Nivel actual: " + level + " (basado en buildIndex)");
    }

    public void OnBallHitsPaleta()
    {
        if (hitPaleta != null)
            hitPaleta.Play();
        else
            Debug.LogWarning("El AudioSource 'hitPaleta' no está asignado en el GameManager.");
    }

    void GuardarPuntuacionMaxima(int puntuacionActual)
    {
        int puntuacionGuardada = PlayerPrefs.GetInt("HighScore", 0);
        if (puntuacionActual > puntuacionGuardada)
        {
            PlayerPrefs.SetInt("HighScore", puntuacionActual);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator FinDelJuego()
    {
        if (scriptPaleta != null)
            scriptPaleta.playing = false;

        if (scriptBola != null)
            scriptBola.gameFinished = true;

        // Pausar música del juego
        if (musicaJuego != null)
            musicaJuego.Pause();

        // Reproducir música de derrota
        if (musicaDerrota != null && !musicaDerrota.isPlaying)
            musicaDerrota.Play();

        // Mostrar pantalla de derrota
        if (derrotaUI != null)
            derrotaUI.MostrarPantallaDerrota();
        else
            Debug.LogWarning("ScriptDerrota no asignado al GameManager");

        // Esperar un tiempo antes de congelar (para que dé tiempo a mostrarse la UI)
        yield return new WaitForSecondsRealtime(1f);
    }


    private IEnumerator MostrarPantallaDerrota()
    {
        // Asegurarse de que la UI derrota comienza oculta
        if (panelDerrota != null)
        {
            panelDerrota.alpha = 0;
            panelDerrota.interactable = true;
            panelDerrota.blocksRaycasts = true;
        }

        if (textoDerrota != null)
            textoDerrota.localScale = Vector3.zero;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            if (panelDerrota != null)
                panelDerrota.alpha = Mathf.Lerp(0, 1, t / fadeDuration);

            if (textoDerrota != null)
            {
                float scaleFactor = Mathf.Sin((t / fadeDuration) * Mathf.PI);
                textoDerrota.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, scaleFactor);
            }

            yield return null;
        }

        if (panelDerrota != null)
            panelDerrota.alpha = 1;

        if (textoDerrota != null)
            textoDerrota.localScale = Vector3.one;
    }

    public void Reset()
    {
        // Reiniciar los datos persistentes
        persistentScore = 0;
        persistentLives = 3;
        persistentLevel = 1;
        isInitialized = false;
    }

}