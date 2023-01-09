using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// To be used for intro scene. Display text VN-style
/// </summary>
public class IntroUI : MonoBehaviour
{
    // Inspector time
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Button nextButton;
    [SerializeField] private TransitionProperties[] transitions;

    [Header("Lightning settings")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private float maxDelay;
    [SerializeField] private float lightningLifeTime;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;

    [Header("Ship settings")]
    [SerializeField] private GameObject ship;
    [SerializeField] private float maxHeight;
    [SerializeField] private float maxRotation;

    [Header("Intro texts")]
    [TextArea(2, 20)] public string[] sentences;

    // vars
    private TextMeshProUGUI textComponent;
    private int currentSentence = 0;
    private Transition transition;
    private float yStartPos;
    private float xStartPos;
    private float zStartPos;
    private float counter = 0;

    private void Awake()
    {
        yStartPos = ship.transform.position.y;
        xStartPos = ship.transform.position.x;
        zStartPos = ship.transform.position.z;
    }

    void Start()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = sentences[currentSentence];

        transition = GetComponent<Transition>();
        transition.StartTransition(transitions[0]);

        directionalLight.GetComponent<Light>();

        Invoke(nameof(LightningSimulator), 0);
    }

    private void Update()
    {
        counter += Time.deltaTime;
        float yPosSine = Mathf.Sin(counter) * maxHeight;
        float zRotSine = Mathf.Cos(counter) * maxRotation;

        ship.transform.position = new Vector3(xStartPos, yPosSine + yStartPos, zStartPos);
        ship.transform.rotation = Quaternion.Euler(ship.transform.rotation.x, ship.transform.rotation.y, zRotSine);
    }

    /// <summary>
    /// Go to the next sentence. If no more sentence, go to the next scene
    /// </summary>
    public void NextSentence()
    {
        currentSentence++;
        if (currentSentence >= sentences.Length)
        {
            //if (SceneManager.GetActiveScene().buildIndex == 1)
            //    SceneManager.LoadScene(2);
            //gameObject.SetActive(false);
            nextButton.interactable = false;
            transition.StartTransition(transitions[1]);
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
        transition.StartTransition(transitions[1]);
    }

    // Definitely not the most efficient code ever
    IEnumerator TransitionText()
    {
        nextButton.interactable = false;

        float elapsedTime = 0;
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

    private void LightningSimulator()
    {
        float intensity = Random.Range(minIntensity, maxIntensity);
        StartCoroutine(LightningTransition(intensity));
    }

    IEnumerator LightningTransition(float intensity)
    {
        float timeElapsed = 0;

        while (timeElapsed < lightningLifeTime)
        {
            timeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0, intensity, (1 - timeElapsed / lightningLifeTime));
            directionalLight.intensity = alpha;
            yield return null;
        }
        float randomDelay = Random.Range(1, maxDelay);
        Invoke(nameof(LightningSimulator), randomDelay);
    }

}
