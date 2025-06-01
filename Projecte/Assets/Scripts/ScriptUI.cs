using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ScriptUI : MonoBehaviour
{
    [Header("UI PowerUps")]
    [SerializeField] public TextMeshProUGUI tiempoAgrandar;
    [SerializeField] public TextMeshProUGUI tiempoReducir;
    [SerializeField] public TextMeshProUGUI tiempoPowerBall;
    [SerializeField] public TextMeshProUGUI tiempoGodMode;
    [SerializeField] public TextMeshProUGUI usosCohetes;
    [SerializeField] public TextMeshProUGUI usosIman;

    public ScriptBola scriptBola;

    // Start is called before the first frame update
    void Start()
    {
        tiempoAgrandar.text = "0";
        tiempoReducir.text = "0";
        tiempoPowerBall.text = "0";
        tiempoGodMode.text = "0";
        usosCohetes.text = "0";
        usosIman.text = "0";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetTiempoPowerBall(float t)
    {
        tiempoPowerBall.text = t > 0 ? ((int)t+1).ToString() : "0";
    }

    public void SetTiempoAgrandar(float t)
    {
        tiempoAgrandar.text = t > 0 ? ((int)t+1).ToString() : "0";
    }

    public void SetTiempoReducir(float t)
    {
        tiempoReducir.text = t > 0 ? ((int)t+1).ToString() : "0";
    }

    public void SetTiempoGodMode(float t)
    {
        tiempoGodMode.text = t > 0 ? ((int)t+1).ToString() : "0";
    }

    public void SetUsosIman(int usos)
    {
        usosIman.text = usos > 0 ? usos.ToString() : "0";
    }

    public void SetUsosCohetes(int usos)
    {
        usosCohetes.text = usos > 0 ? usos.ToString() : "0";
    }
}
