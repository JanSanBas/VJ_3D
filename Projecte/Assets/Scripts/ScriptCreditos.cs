using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptCreditos : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Invoke("WaitToEnd", 8f);
        GameManager.Instance.CompletarJuego();
    }

    public void WaitToEnd()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
