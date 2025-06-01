using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class ScriptPausa : MonoBehaviour
{
    [Header("UI Elementos")]
    [SerializeField] private CanvasGroup panelPausa; // El Canvas Group del PanelPausa

    private bool juegoEnPausa = false;

    [SerializeField] private GameObject primerBotonSeleccionado; // Para seleccionar el primer bot�n al abrir la pausa

    // Opcional: Audio para botones
    public AudioSource clickSound;
    public AudioSource hoverSound;

    private string[] escenasExcluidas = { "MainMenu", "Credits" }; 


    [SerializeField] private ScriptDerrota scriptDerrota; // Referencia al script de derrota, si es necesario


    void Start()
    {
        // --- LA CORRECCI�N CLAVE EST� AQU� ---
        // Asegurarse de que el panel de pausa est� oculto e inactivo al inicio
        // Esto es crucial para que no se muestre al cargar la escena
        if (panelPausa != null)
        {
            panelPausa.alpha = 0; // Lo hace completamente transparente
            panelPausa.interactable = false; // Desactiva la interacci�n (clics de rat�n, etc.)
            panelPausa.blocksRaycasts = false; // Evita que bloquee los clics en los elementos de debajo
        }
        // ------------------------------------

        // Intentar encontrar ScriptDerrota si no se asign� en el Inspector
        if (scriptDerrota == null)
        {
            scriptDerrota = FindObjectOfType<ScriptDerrota>();
            if (scriptDerrota == null)
            {
                Debug.LogWarning("ScriptDerrota no encontrado en la escena. Aseg�rate de que est� asignado o presente.");
            }
        }
    }

    void Update()
    {
        // --- L�GICA DE VERIFICACI�N DE ESCENA Y ESTADO DE DERROTA ---
        string nombreEscenaActual = SceneManager.GetActiveScene().name;

        // Comprobar si la escena actual est� en la lista de escenas excluidas (MenuPrincipal, Creditos)
        bool esEscenaExcluidaPorNombre = false;
        foreach (string escenaExcluida in escenasExcluidas)
        {
            if (nombreEscenaActual == escenaExcluida)
            {
                esEscenaExcluidaPorNombre = true;
                break;
            }
        }

        // Comprobar si la pantalla de derrota est� activa (si tenemos la referencia)
        bool esPantallaDerrotaActiva = (scriptDerrota != null && scriptDerrota.isShowing);


        // Si es una escena excluida por nombre O la pantalla de derrota est� activa, no procesar la pausa
        if (esEscenaExcluidaPorNombre || esPantallaDerrotaActiva)
        {
            // Opcional: Asegurarse de que si por alguna raz�n la pausa estaba activa, se desactive.
            if (juegoEnPausa)
            {
                ReanudarJuego();
            }
            return; // Salir del Update para no procesar la entrada de pausa
        }

        // Detectar si se pulsa la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoEnPausa)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    public void PausarJuego()
    {
        if (juegoEnPausa) return; // Ya est� pausado, no hacer nada

        juegoEnPausa = true;
        Time.timeScale = 0f; // Congelar el juego
        // Deshabilitar el control del jugador a trav�s del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.controlHabilitado = false;
            // No reseteamos los power-ups activos ni los destruimos al pausar,
            // simplemente se congelar�n porque Time.timeScale es 0.
        }

        // Mostrar la pantalla de pausa de golpe
        if (panelPausa != null)
        {
            panelPausa.alpha = 1;
            panelPausa.interactable = true;
            panelPausa.blocksRaycasts = true;
        }

        // Seleccionar el primer bot�n para navegaci�n con teclado/gamepad
        if (primerBotonSeleccionado != null)
        {
            EventSystem.current.SetSelectedGameObject(primerBotonSeleccionado);
        }
    }

    public void ReanudarJuego()
    {
        if (!juegoEnPausa) return; // No est� pausado, no hacer nada

        juegoEnPausa = false;
        Time.timeScale = 1f; // Reanudar el juego
        // Habilitar el control del jugador a trav�s del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.controlHabilitado = true;
        }

        // Ocultar la pantalla de pausa de golpe
        if (panelPausa != null)
        {
            panelPausa.alpha = 0;
            panelPausa.interactable = false;
            panelPausa.blocksRaycasts = false;
        }

        // Resetear la selecci�n del EventSystem
        EventSystem.current.SetSelectedGameObject(null);
    }

    // M�todos para los botones de la UI
    public void OnClickContinuar()
    {
        clickSound?.Play(); // Reproducir sonido de click si est� configurado
        ReanudarJuego(); // Simplemente reanuda el juego
    }

    public IEnumerator OnClickSalirMenu()
    {

        clickSound?.Play(); // Reproducir sonido de click si est� configurado
        // IMPORTANTE: Resetear los datos persistentes SOLO al salir al men�
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Reset();
        }
        Time.timeScale = 1f; // Asegurarse de que el tiempo se reanude antes de cargar la escena

        yield return new WaitForSeconds(clickSound.clip.length);

        SceneManager.LoadScene("MainMenu"); // Carga tu escena de men� principal
    }

    public void OnClickSalirJuego()
    {

        // IMPORTANTE: Resetear los datos persistentes SOLO al salir del juego
        clickSound?.Play(); // Reproducir sonido de click si est� configurado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Reset();
        }
        Application.Quit(); // Cierra la aplicaci�n
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnPointerEnter()
    {
        if (hoverSound != null)
            hoverSound.Play();
    }
}