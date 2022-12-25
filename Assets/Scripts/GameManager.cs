using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keep track of resources, effects, status, and NPCs
/// </summary>
public class GameManager : MonoBehaviour
{
    // Game manager said "It's Inspector time" and inspecting all over my brain
    [Header("Current community status")]
    [SerializeField] private int dayElapsed = 0; // keep track of time
    [SerializeField] private int maxActionPoint = 3; // action points that player need to spend per day
    [SerializeField] private int npcCount = 7; // how many NPC's are alive
    [SerializeField] [Range(-1, 1)] private float mood = 0.5f;
    [SerializeField] [Range(0, 1)] private float wellbeing = 0.8f;
    [SerializeField] [Range(0, 1)] private float hunger = 0.5f;
    [SerializeField] [Range(0, 1)] private float moodDecayRate = 0.02f;
    [SerializeField] [Range(0, 1)] private float wellbeingDecayRate = 0.01f;
    [SerializeField] [Range(0, 1)] private float hungerDecayRate = 0.2f;

    [Header("Starting resources")]
    [SerializeField] private int cookedFood = 15;
    [SerializeField] private int wood = 25; // depending on player wanted to collect

    [Header("Misc")]
    [SerializeField] private GameUI gameUI;

    // private vars
    private int actionPoint;
    private Dictionary<ResourceType, int> currentResources = new Dictionary<ResourceType, int>();

    void Awake()
    {
        // Add resources
        currentResources.Add(ResourceType.Wood, 15);
        currentResources.Add(ResourceType.Stone, 0);
        currentResources.Add(ResourceType.Reed, 10);
        currentResources.Add(ResourceType.Herb, 0);
        currentResources.Add(ResourceType.Raw_Food, 0);
        currentResources.Add(ResourceType.Cooked_Food, cookedFood);
        currentResources.Add(ResourceType.Delicious_Food, 0);
        currentResources.Add(ResourceType.Leather, 5);

        // Instantiate stuff
        //Debug.Log(CheckResources(ResourceType.Wood) + " " + ResourceType.Wood.ToString());
        actionPoint = maxActionPoint;
    }

    /// <summary>
    /// Update everything after 1 action point spent
    /// </summary>
    public void NextAction()
    {
        Debug.Log("Next action!");
    }

    /// <summary>
    /// Calculate everything for next day when this method is called
    /// </summary>
    public void NextDay()
    {
        mood -= moodDecayRate;
        wellbeing -= wellbeingDecayRate;
        hunger -= hungerDecayRate;

        if (mood <= -100f || wellbeing <= 0)
            Debug.Log("Game over!");
        actionPoint = maxActionPoint;
    }

    /// <summary>
    /// Check current resources by resource type
    /// </summary>
    /// <param name="resourceType">Type of resources</param>
    /// <returns>Amount of current resources</returns>
    public int CheckResources(ResourceType resourceType)
    {
        if (currentResources.TryGetValue(resourceType, out int amount))
        {
            Debug.Log(amount + " " + resourceType.ToString());
            return amount;
        }
        else
        {
            Debug.LogError("Error checking resources for " + resourceType);
            return -1;
        }
    }
}

/// <summary>
/// Type of resources exists in this game
/// </summary>
public enum ResourceType
{ 
    Wood,
    Stone,
    Reed,
    Herb,
    Raw_Food,
    Cooked_Food,
    Delicious_Food,
    Leather
}

/// <summary>
/// Types of building available
/// </summary>
public enum BuildingType {
    Campsite,
    Town_Hall,
    Workshop,
    Lumber_yard,
    Hunter_hut,
    Fishing_hut,
    Food_hall,
    Clinic
}
