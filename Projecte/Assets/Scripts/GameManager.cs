using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    // Singleton estatico para datos persistentes
    private static int persistentScore = 0;
    private static int persistentLives = 3;
    private static int persistentLevel = 1;
    private static bool isInitialized = false;

    // Variables de instancia para la escena actual
    private int score;
    private int lives;
    private int level;

    // Referencias a la UI
    [SerializeField] private TextMeshProUGUI numeroPuntosText;
    [SerializeField] private TextMeshProUGUI numeroVidasText;
    [SerializeField] private TextMeshProUGUI numeroNivelText;

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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Carga el siguiente nivel cuando no hay cubos
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
        lives--;
        persistentLives = lives; // Guardar en datos persistentes

        if (lives <= 0)
        {
            // Lógica de Game Over
            Debug.Log("Game Over");
            // Puedes cargar una escena de Game Over o reiniciar el juego
            // SceneManager.LoadScene("GameOverScene"); // Ejemplo: cargar una escena de Game Over
        }
        else updateUI();
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
}