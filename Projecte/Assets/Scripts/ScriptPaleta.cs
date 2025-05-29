using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class ScriptPaleta : MonoBehaviour
{
    private float xMax;
    private float xMin;

    private float speed;
    public bool playing = false;

    // Start is called before the first frame update
    void Start()
    {
        xMax = 14.41f;
        xMin = -14.41f;
        speed = 10f;
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
}
