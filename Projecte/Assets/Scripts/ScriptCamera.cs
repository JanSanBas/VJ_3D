using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCamera : MonoBehaviour
{

    [Header("Target")]
    public Transform target; // Centro del nivel

    [Header("Animación")]
    public float duration;             // Duración fija de la animación
    public float rotationSpeed;       // Velocidad de rotación (grados/segundo)
    public float orbitDistance;       // Distancia desde el centro
    public float cameraHeight = 23.84f;     // Altura constante

    [Header("Final")]
    public Vector3 posicionFinal = new Vector3(0f, 23.84f, -19.52f);
    public Vector3 rotacionFinal = new Vector3(52.845f, 0f, 0f);
    public float fovFinal = 45f;

    private float elapsedTime = 0f;
    private float currentAngle = 0f;
    private Camera cam;
    private bool animando = true;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraIntroOrbit: no se asignó target.");
            enabled = false;
            return;
        }

        cam = GetComponent<Camera>();
        if (cam != null)
            cam.fieldOfView = fovFinal;
    }

    void Update()
    {
        if (!animando) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime < duration)
        {
            // Avanzar ángulo según velocidad
            currentAngle += rotationSpeed * Time.deltaTime;

            float rad = currentAngle * Mathf.Deg2Rad;

            // Calcular posición orbital
            Vector3 offset = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * orbitDistance;
            Vector3 nuevaPos = target.position + offset;
            nuevaPos.y = cameraHeight;

            transform.position = nuevaPos;

            // Rotación: mirar hacia el centro, mantener inclinación
            transform.LookAt(target.position);
            transform.rotation = Quaternion.Euler(rotacionFinal.x, transform.eulerAngles.y, 0);
        }
        else
        {
            // Finalizar animación
            transform.position = posicionFinal;
            transform.rotation = Quaternion.Euler(rotacionFinal);
            animando = false;
            if (GameManager.Instance != null)
                GameManager.Instance.HabilitarControl();
        }
    }
}
