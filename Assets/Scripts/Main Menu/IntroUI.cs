using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// To be used for intro scene. Display text VN-style
/// </summary>
public class IntroUI : MonoBehaviour
{
    // Inspector time
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Button nextButton;

    [Header("Intro texts")]
    [TextArea(2, 20)] public string[] sentences;

    // vars
    private TextMeshProUGUI textComponent;
    private int currentSentence = 0;
    private float elapsedTime = 0;

    void Start()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = sentences[currentSentence];
    }

    /// <summary>
    /// Go to the next sentence. If no more sentence, go to the next scene
    /// </summary>
    public void NextSentence()
    {
        currentSentence++;
        if (currentSentence >= sentences.Length)
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
                SceneManager.LoadScene(2);
            gameObject.SetActive(false);
        }
        else
        {
            StartCoroutine(TransitionText());
        }
    }

    /// <summary>
    /// If the player wants to skip the intro
    /// </summary>
    public void Skip()
    {
        SceneManager.LoadScene(2);
    }

    // Definitely not the most efficient code ever
    IEnumerator TransitionText()
    {
        nextButton.interactable = false;

        // Fade out
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / fadeDuration);
            textComponent.alpha = alpha;
            yield return null;
        }
        elapsedTime = 0;
        
        // Change the text
        textComponent.text = sentences[currentSentence];

        // Fade In
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / fadeDuration;
            textComponent.alpha = alpha;
            yield return null;
        }
        nextButton.interactable = true;
    }
}
