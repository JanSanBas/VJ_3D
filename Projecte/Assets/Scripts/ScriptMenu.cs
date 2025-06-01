﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro; // Asegúrate de usar esto si usas TextMeshPro

public class ScriptMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource clickSound;
    public AudioSource hoverSound;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI highScoreText;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(DesactivarSeleccionInicial());

        // Leer la puntuación máxima desde PlayerPrefs
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (highScoreText != null)
        {
            highScoreText.text = "Puntuacion Maxima: " + highScore;
        }
    }

    private IEnumerator DesactivarSeleccionInicial()
    {
        yield return null; // Esperar 1 frame
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Jugar()
    {
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(PlaySoundAndLoadScene("Nivel1"));
    }

    private IEnumerator PlaySoundAndLoadScene(string sceneName)
    {
        if (clickSound != null)
            clickSound.Play();

        yield return new WaitForSeconds(clickSound.clip.length);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void Sortir()
    {
        EventSystem.current.SetSelectedGameObject(null);
        Application.Quit();
    }

    public void OnPointerEnter()
    {
        if (hoverSound != null)
            hoverSound.Play();
    }

    public void ResetHighScore()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (clickSound != null)
            clickSound.Play();

        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();

        if (highScoreText != null)
            highScoreText.text = "Puntuacion Maxima: 0";
    }

}
