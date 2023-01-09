using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// Edit properties of Transition class
/// </summary>
[System.Serializable]
public class TransitionProperties
{
    public FadeType fadeType;
    public float fadeDuration;
    public float delay;
    public int nextScene;
}

/// <summary>
/// Class to manage transition
/// </summary>
public class Transition : MonoBehaviour
{
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private GameObject instance;

    private Camera mainCamera;
    private Image image;

    private void Start()
    {
        // Instantiate to main camera
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        instance = Instantiate(transitionCanvas, mainCamera.transform);
        instance.transform.SetParent(mainCamera.transform);
        instance.GetComponent<Canvas>().worldCamera = mainCamera;
        image = instance.GetComponentInChildren<Image>();
    }

    /// <summary>
    /// Start the transition
    /// </summary>
    /// <param name="fadeType"></param>
    /// <param name="color"></param>
    /// <param name="fadeDuration"></param>
    /// <param name="delay"></param>
    public void StartTransition(TransitionProperties prop)
    {

        if (prop.fadeType == FadeType.FadeOutIn)
            StartCoroutine(FadeBothTransition(prop.fadeDuration, prop.delay));
        else
            StartCoroutine(FadeIndividualTransition(prop.fadeType, prop.fadeDuration, prop.delay, prop.nextScene));
    }

    IEnumerator FadeIndividualTransition(FadeType fadeType, float fadeDuration, float delay, int scene)
    {
        if (fadeType == FadeType.FadeOut)
            yield return new WaitForSeconds(delay);
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha;
            if (fadeType == FadeType.FadeIn)
                alpha = elapsedTime / fadeDuration;
            else
                alpha = 1 - (elapsedTime / fadeDuration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        if (fadeType == FadeType.FadeIn)
        {
            yield return new WaitForSeconds(delay);
            if (SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(scene))
                SceneManager.LoadScene(scene);
        }
    }

    IEnumerator FadeBothTransition(float fadeDuration, float delay)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / fadeDuration;
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(delay);

        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / fadeDuration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}

public enum FadeType
{
    FadeIn,
    FadeOut,
    FadeOutIn
}