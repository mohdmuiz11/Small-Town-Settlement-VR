using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple chest system
/// </summary>
public class Chest : MonoBehaviour
{
    [SerializeField] private SideEvent sideEvent;
    private GameManager gameManager;
    private int count = 0;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WoodScraps"))
        {
            count++;
            gameManager.ChangeValueResource(ResourceType.Wood, 1);
            Destroy(other.gameObject);
            if (count >= 3)
            {
                sideEvent.DeleteAllObjects();
                GameObject.Find("DialogueManager").GetComponent<DialogueManager>().RunDialogue(1);
            }
        }
    }
}
