using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue settings")]
    [SerializeField] private TransitionProperties transition;
    [SerializeField] private Canvas dialogueCanvas;

    [Header("Contents")]
    [SerializeField] private Dialogue[] dialogue;

    private void Start()
    {
        
    }
}

[System.Serializable]
public class Dialogue
{
    public NPC npc;
    public Transform location;
    [TextArea(2, 20)] public string[] texts;
    public UnityEvent nextAction;
}
