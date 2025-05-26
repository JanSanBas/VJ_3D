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
        // Leer la puntuación máxima desde PlayerPrefs
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (highScoreText != null)
        {
            highScoreText.text = "Puntuacion Maxima: " + highScore;
        }
    }

    public void Jugar()
    {
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
        Application.Quit();
    }

    public void OnPointerEnter()
    {
        if (hoverSound != null)
            hoverSound.Play();
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();

        if (highScoreText != null)
            highScoreText.text = "Puntuacion Maxima: 0";
    }

}
