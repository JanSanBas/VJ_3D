using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private int score;
    private int lives;
    private int level;

    [SerializeField] private TextMeshProUGUI numeroPuntosText;
    [SerializeField] private TextMeshProUGUI numeroVidasText;
    [SerializeField] private TextMeshProUGUI numeroNivelText;

    private void Awake()
    {
        score = 0;
        lives = 3;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        getUIElements();
        determineLevelNum();
        updateUI();

    }

    // Update is called once per frame
    void Update()
    {
        level = SceneManager.GetActiveScene().buildIndex + 1;
        updateUI();
    }
    public void addScore(int score)
    {
        this.score += score;
        updateUI();
        // Implement your score logic here
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
        updateUI();
    }

    public void reduceLives()
    {
        lives--;
        if (lives <= 0)
        {
            // Implement your game over logic here
            Debug.Log("Game Over");
        }
        else updateUI();
    }

    public void loadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    private void getUIElements()
    {
        if (numeroPuntosText == null)
        {
            numeroPuntosText = GameObject.Find("NumeroPuntos").GetComponent<TextMeshProUGUI>();
        }

        if (numeroVidasText == null)
        {
            numeroVidasText = GameObject.Find("NumeroVidas").GetComponent<TextMeshProUGUI>();
        }

        if (numeroNivelText == null)
        {
            numeroNivelText = GameObject.Find("NumeroNivel").GetComponent<TextMeshProUGUI>();
        }
    }

    private void determineLevelNum()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.StartsWith("Nivel"))
        {
            string levelStr = sceneName.Substring(5);
            if (int.TryParse(levelStr, out int currentLevel))
            {
                level = currentLevel;
                Debug.Log("Nivel detectado: " + level + " de escena: " + sceneName);
            }
            else
            {
                level = SceneManager.GetActiveScene().buildIndex;
                Debug.Log("No se pudo extraer nivel de " + sceneName + ", usando buildIndex: " + level);
            }
        }
        else
        {
            // Si el nombre de la escena no comienza con "Nivel", usamos el buildIndex
            level = SceneManager.GetActiveScene().buildIndex + 1;
            Debug.Log("Usando buildIndex para nivel: " + level + " de escena: " + sceneName);
        }
    }
}
