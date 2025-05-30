using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class ScriptPaleta : MonoBehaviour
{
    private float xMax;
    private float xMin;

    private float speed;
    private Vector3 originalScale; // Guardar la escala original
    public bool playing = false;

    // Start is called before the first frame update
    void Start()
    {
        xMax = 14.41f; // Ajusta estos valores si tu paleta original es diferente
        xMin = -14.41f; // o si quieres que se mueva por un rango diferente
        speed = 15f;

        originalScale = transform.localScale; // Guarda la escala inicial de la paleta
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing || !GameManager.Instance.controlHabilitado) return;

        float horizontalInput = Input.GetAxis("Horizontal");

        float movement = horizontalInput * speed * Time.deltaTime;

        Vector3 novaPos = transform.position + new Vector3(movement, 0, 0);
        novaPos.x = Mathf.Clamp(novaPos.x, xMin, xMax);

        transform.position = novaPos;

    }

    // Nuevo m�todo para establecer la escala de la paleta
    public void SetPaddleScale(float scaleFactor)
    {
        // Multiplica la escala original por el factor
        transform.localScale = new Vector3(originalScale.x * scaleFactor, originalScale.y, originalScale.z);

        float extraWidthHalf = (transform.localScale.x - originalScale.x) / 2f;

        xMin = -14.41f + extraWidthHalf; // Ajusta los l�mites de movimiento seg�n la nueva escala
        xMax = 14.41f - extraWidthHalf; // Ajusta los l�mites de movimiento seg�n la nueva escala
        // Aseg�rate de que los l�mites de movimiento tambi�n se ajusten si la paleta es muy grande
        // Esto es opcional y depende de c�mo quieras que se maneje el movimiento con paletas grandes.
        // Por ahora, solo cambiar� la escala, pero podr�a salirse de los l�mites visualmente.
    }

    // Nuevo m�todo para resetear la escala de la paleta a su tama�o original
    public void ResetPaddleScale()
    {
        transform.localScale = originalScale;
        xMin = -14.41f; // Resetea los l�mites si es necesario
        xMax = 14.41f; // Resetea los l�mites si es necesario
    }
}