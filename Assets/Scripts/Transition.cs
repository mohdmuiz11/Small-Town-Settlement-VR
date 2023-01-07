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
    private GameObject instance;

    /// <summary>
    /// Start the transition
    /// </summary>
    /// <param name="fadeType"></param>
    /// <param name="color"></param>
    /// <param name="fadeDuration"></param>
    /// <param name="delay"></param>
    public void StartTransition(TransitionProperties prop)
    {
        var camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        instance = Instantiate(transitionCanvas, camera.transform);
        instance.transform.SetParent(camera.transform);
        instance.GetComponent<Canvas>().worldCamera = camera;
        Image image = instance.GetComponentInChildren<Image>();

        if (prop.fadeType == FadeType.FadeOutIn)
            StartCoroutine(FadeBothTransition(image, prop.fadeDuration, prop.delay));
        else
            StartCoroutine(FadeIndividualTransition(prop.fadeType, image, prop.fadeDuration, prop.delay, prop.nextScene));
    }

    IEnumerator FadeIndividualTransition(FadeType fadeType, Image image, float fadeDuration, float delay, int scene)
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
            SceneManager.LoadScene(scene);
        }
        else
            Destroy(instance);
    }

    IEnumerator FadeBothTransition(Image image, float fadeDuration, float delay)
    {
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / fadeDuration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        yield return new WaitForSeconds(delay);

        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / fadeDuration;
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        Destroy(instance);
    }
}

public enum FadeType
{
    FadeIn,
    FadeOut,
    FadeOutIn
}