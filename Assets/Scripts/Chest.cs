using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple chest system
/// </summary>
public class Chest : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WoodScraps"))
        {
            gameManager.ChangeValueResource(ResourceType.Wood, 1);
            Destroy(other.gameObject);
        }
    }
}
