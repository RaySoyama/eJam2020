using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    public string playSceneName = "";
    public FadeManager fadeMan = null;

    public void OnPlay()
    {
        fadeMan.OnPlay(() =>
        {
            SceneManager.LoadScene(playSceneName);
        });
    }

    public void OnQuit()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
#if !UNITY_EDITOR
            Application.Quit();
#endif
    }
}
