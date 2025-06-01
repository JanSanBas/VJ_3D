using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptTextoTitulo : MonoBehaviour
{
    [Header("Animación de escala (inicio)")]
    public float tiempoEscalado = 1f;
    public Vector3 escalaFinal = Vector3.one;

    [Header("Animación de balanceo (loop)")]
    public float amplitudRotacion = 5f; // grados
    public float velocidadRotacion = 2f;

    private Vector3 escalaInicial;

    void Start()
    {
        escalaInicial = escalaFinal * 0.1f; // Escala inicial muy pequeña
        transform.localScale = escalaInicial;

        StartCoroutine(AnimarEscala());
    }

    System.Collections.IEnumerator AnimarEscala()
    {
        float t = 0f;
        while (t < tiempoEscalado)
        {
            t += Time.deltaTime;
            float progreso = t / tiempoEscalado;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progreso);
            yield return null;
        }

        transform.localScale = escalaFinal;
    }

    void Update()
    {

        float angulo = Mathf.Sin(Time.time * velocidadRotacion) * amplitudRotacion;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }
}
