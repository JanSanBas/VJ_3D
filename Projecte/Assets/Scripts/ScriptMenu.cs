using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptMenu : MonoBehaviour
{
    // Start is called before the first frame update
    public void Jugar()
    {
        SceneManager.LoadScene("Nivel1", LoadSceneMode.Single);
    }

    // Update is called once per frame
    public void Sortir()
    {
        Application.Quit();
    }
}
