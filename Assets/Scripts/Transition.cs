using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Transition
{
    private MonoBehaviour monoBehaviour;

    public Transition(MonoBehaviour monoBehaviour)
    {
        this.monoBehaviour = monoBehaviour;
    }

    /// <summary>
    /// Start the transition
    /// </summary>
    /// <param name="fadeType"></param>
    /// <param name="color"></param>
    /// <param name="fadeDuration"></param>
    /// <param name="delay"></param>
    public void StartTransition(FadeType fadeType, Image image, float fadeDuration, float delay)
    {
        Debug.Log("Bruh");
        if (fadeType == FadeType.FadeIn)
            monoBehaviour.StartCoroutine(FadeInTransition(image, fadeDuration, delay));
        else if (fadeType == FadeType.FadeOut)
            monoBehaviour.StartCoroutine(FadeOutTransition(image, fadeDuration, delay));
        else
            monoBehaviour.StartCoroutine(FadeOutInTransition(image, fadeDuration, delay));
    }

    IEnumerator FadeOutTransition(Image image, float fadeDuration, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / fadeDuration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
    IEnumerator FadeInTransition(Image image, float fadeDuration, float delay)
    {
        yield return new WaitForSeconds(delay);
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / fadeDuration;
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
    IEnumerator FadeOutInTransition(Image image, float fadeDuration, float delay)
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
    }
}

public enum FadeType
{
    FadeIn,
    FadeOut,
    FadeOutIn
}