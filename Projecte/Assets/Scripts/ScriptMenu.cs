using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptMenu : MonoBehaviour
{

    public AudioSource clickSound;

    // Start is called before the first frame update
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

    // Update is called once per frame
    public void Sortir()
    {
        Application.Quit();
    }
}
