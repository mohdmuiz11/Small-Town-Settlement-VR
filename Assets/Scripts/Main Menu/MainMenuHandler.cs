using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private LensFlareComponentSRP lensFlare;
    [SerializeField] private TransitionProperties[] transitions;
    private Transition transition;

    private void Awake()
    {
        transition = GetComponent<Transition>();
        lensFlare.intensity = 0;
        StartCoroutine(LensFlareDelay(true, 0));
        transition.StartTransition(transitions[0]);
    }

    public void ContinueGame()
    {
        StartCoroutine(LensFlareDelay(false, 2));
        transition.StartTransition(transitions[2]);
    }
    public void NewGame()
    {
        StartCoroutine(LensFlareDelay(false, 1));
        transition.StartTransition(transitions[1]);
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

    IEnumerator LensFlareDelay(bool start, int sceneNumber)
    {
        yield return new WaitForSeconds(transitions[0].delay);
        float elapsedTime = 0;
        while (elapsedTime < transitions[0].fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha;
            if (start)
                alpha = elapsedTime / transitions[0].fadeDuration;
            else
                alpha = 1 - (elapsedTime / transitions[0].fadeDuration);
            lensFlare.intensity = alpha;
            yield return null;
        }
    }
}
