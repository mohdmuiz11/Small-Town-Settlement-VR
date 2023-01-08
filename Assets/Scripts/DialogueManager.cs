using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue settings")]
    [SerializeField] private TransitionProperties transition;
    [SerializeField] private TransitionProperties transitionFadeIn;
    [SerializeField] private TransitionProperties transitionFadeOut;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private Canvas[] controlScheme;

    [Header("Contents")]
    [SerializeField] private Dialogue[] dialogues;

    // Current context
    private GridSystem gridSystem;
    private Transform playerPosition;
    [SerializeField] private Canvas instance;
    private Dialogue dialogue;
    private Transition transitionComponent;
    private int currentTextIndex;
    private TextMeshProUGUI dialogueText;
    private Button button;
    private InteractionLayerMask currentLayerMask;
    private NPC currentNPC;

    private void Start()
    {
        transitionComponent = GameObject.Find("GameManager").GetComponent<Transition>();
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        playerPosition = GameObject.Find("XR Rig").GetComponent<Transform>();
    }

    /// <summary>
    /// Run a dialogue according to its index
    /// </summary>
    /// <param name="index">Index of dialogue</param>
    public void RunDialogue(int index)
    {
        // Referencing dialogue
        dialogue = dialogues[index];
        currentTextIndex = 0;
        currentLayerMask = GameObject.Find("RightHand Controller").GetComponent<XRBaseControllerInteractor>().interactionLayers;

        // Set controller to not interact with anything
        gridSystem.SetControllerInteractionLayer(0);

        // Set up dialogue
        StartCoroutine(InitateCanvas());
    }

    private void NextText()
    {
        currentTextIndex++;
        // If the dialogue is finished
        if (currentTextIndex >= dialogue.texts.Length)
        {
            button.interactable = false;
            if (!dialogue.doNotDestroyCanvasEnd)
                StartCoroutine(DestroyCanvas());
        }
        else // Insert text to dialogue canvas
            StartCoroutine(TransitionText(transition.fadeDuration / 2, dialogue.texts[currentTextIndex]));
    }

    // Initiate canvas
    IEnumerator InitateCanvas()
    {
        if (!dialogue.disableTransitionStart)
        {
            if (dialogue.overrideFadeType == FadeType.FadeOut)
            {
                transitionComponent.StartTransition(transitionFadeOut);
                yield return null;
            }
            else
            {
                transitionComponent.StartTransition(transition);
                yield return new WaitForSeconds(transition.fadeDuration);
            }
        }
        else
            yield return null;


        if (dialogue.forcePlayerPos != null)
        {
            playerPosition.position = dialogue.forcePlayerPos.position;
            playerPosition.rotation = dialogue.forcePlayerPos.rotation;
        }

        if (!dialogue.doNotInstanceStart)
        {
            instance = Instantiate(dialogueCanvas, dialogue.location, false);
            instance.worldCamera = mainCamera;
            dialogueText = instance.GetComponentInChildren<TextMeshProUGUI>();
            button = instance.GetComponentInChildren<Button>();
            button.onClick.AddListener(NextText);
            if (dialogue.npc != null)
                currentNPC = Instantiate(dialogue.npc, dialogue.npcLocation, false);
        }

        dialogueText.text = dialogue.texts[currentTextIndex];
    }

    // Destroy canvas
    IEnumerator DestroyCanvas()
    {
        if (!dialogue.disableTransitionEnd)
        {
            if (dialogue.overrideFadeType == FadeType.FadeIn)
                transitionComponent.StartTransition(transitionFadeIn);
            else
                transitionComponent.StartTransition(transition);
            yield return new WaitForSeconds(transition.fadeDuration);
        }
        else
            yield return null;
        gridSystem.SetControllerInteractionLayer(currentLayerMask);
        if (currentNPC != null)
        {
            Destroy(currentNPC.gameObject);
            currentNPC = null;
        }
        Destroy(instance.gameObject);
        
        // Run actions if any
        if (dialogue.nextAction != null)
            dialogue.nextAction.Invoke();
    }

    IEnumerator TransitionText(float fadeDuration, string text)
    {
        button.interactable = false;
        float elapsedTime = 0;

        // Fade out
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = 1 - (elapsedTime / fadeDuration);
            dialogueText.alpha = alpha;
            yield return null;
        }
        elapsedTime = 0;

        // Change the text
        dialogueText.text = text;

        // Fade In
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = elapsedTime / fadeDuration;
            dialogueText.alpha = alpha;
            yield return null;
        }
        button.interactable = true;
    }
}

[System.Serializable]
public class Dialogue
{
    [Header("Settings")]
    public NPC npc;
    public Transform npcLocation;
    public Transform location;
    public Transform forcePlayerPos;

    [Header("Custom Settings")]
    public FadeType overrideFadeType;
    public bool disableTransitionStart;
    public bool disableTransitionEnd;
    public bool doNotInstanceStart;
    public bool doNotDestroyCanvasEnd;

    [TextArea(2, 20)] public string[] texts;
    public UnityEvent nextAction;
}
