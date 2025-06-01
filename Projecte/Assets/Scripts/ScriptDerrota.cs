using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class ScriptDerrota : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public RectTransform textoDerrota; // Referencia al título de "Derrota"
    public float fadeDuration = 1f;
    public float escalaRebote = 0.8f;
    public float duracionRebote = 1f;

    [Header("Audio")]
    public AudioSource clickSound;
    public AudioSource hoverSound;

    public bool isShowing = false; // Indica si la pantalla de derrota está visible/activa


    void Start()
    {
        // Inicialmente oculto e invisible
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (textoDerrota != null)
            textoDerrota.localScale = Vector3.zero;

        isShowing = false;

    }

    public void MostrarPantallaDerrota()
    {
        isShowing = true;
        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        if (textoDerrota != null)
            StartCoroutine(AnimarTextoDerrota());

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    private IEnumerator AnimarTextoDerrota()
    {
        float t = 0f;
        float duracion = duracionRebote;

        Vector3 escalaFinal = Vector3.one;

        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            float progreso = t / duracion;

            // Animación de rebote: pasa de 0 → un poco más grande → rebota y se asienta en 1
            float curva = RebotePersonalizado(progreso);
            textoDerrota.localScale = escalaFinal * curva;

            yield return null;
        }

        textoDerrota.localScale = escalaFinal;
    }

    // Simula una curva de rebote: sube rápido, pasa el valor final y baja un poco
    private float RebotePersonalizado(float t)
    {
        // t entre 0 y 1
        float exceso = 0.15f; // cuánto se pasa del tamaño final
        float rebote = 0.05f; // cuánto baja después

        if (t < 0.7f)
        {
            // fase de expansión (hasta 1.15x)
            return Mathf.Lerp(0f, 1f + exceso, t / 0.7f);
        }
        else
        {
            // fase de rebote hacia abajo (hasta 0.95x)
            return Mathf.Lerp(1f + exceso, 1f - rebote, (t - 0.7f) / 0.3f);
        }
    }


    public void Inicio()
    {
        isShowing = false;
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.Instance.Reset();
        StartCoroutine(PlaySoundAndLoadScene("MainMenu"));
    }

    public void Sortir()
    {
        isShowing = false;
        EventSystem.current.SetSelectedGameObject(null);
        GameManager.Instance.Reset();
        Application.Quit();
    }

    public void OnPointerEnter()
    {
        if (hoverSound != null)
            hoverSound.Play();
    }
    private IEnumerator PlaySoundAndLoadScene(string sceneName)
    {
        if (clickSound != null)
            clickSound.Play();

        yield return new WaitForSeconds(clickSound.clip.length);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        
    }
}
