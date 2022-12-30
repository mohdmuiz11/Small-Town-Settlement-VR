using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public void ContinueGame()
    {
        SceneManager.LoadScene(2);
    }
    public void NewGame()
    {
        SceneManager.LoadScene(1);
    }
    public void QuitGame()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}
