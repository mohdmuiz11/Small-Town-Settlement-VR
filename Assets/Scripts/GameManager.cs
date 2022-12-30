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
    [SerializeField] private GameObject[] interiorDesign;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private Transform locationBuilding;
    [SerializeField] private bool isTalkingToNPC; //useful toggle to focus on NPC dialogues later

    // private vars
    private int actionPoint;
    private Dictionary<ResourceType, int> _currentResources = new();
    public Dictionary<ResourceType, int> currentResources { get { return _currentResources; } }

    // Object references
    private SlotManager slotManager;

    void Awake()
    {
        // Object reference
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();

        // Add resources
        _currentResources.Add(ResourceType.Wood, 25);
        _currentResources.Add(ResourceType.Stone, 15);
        _currentResources.Add(ResourceType.Reed, 10);
        _currentResources.Add(ResourceType.Herb, 0);
        _currentResources.Add(ResourceType.Raw_Food, 0);
        _currentResources.Add(ResourceType.Cooked_Food, cookedFood);
        _currentResources.Add(ResourceType.Delicious_Food, 0);
        _currentResources.Add(ResourceType.Leather, 5);

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
        gameUI.UpdateNextAction();
    }

    /// <summary>
    /// Calculate everything for next day when this method is called
    /// </summary>
    public void NextDay()
    {
        // Update current stats
        mood -= moodDecayRate;
        wellbeing -= wellbeingDecayRate;
        hunger -= hungerDecayRate;

        if (mood <= -100f || wellbeing <= 0)
            Debug.Log("Game over!");

        // Refresh action points
        actionPoint = maxActionPoint;

        // Update for other classes
        slotManager.UpdateNextDay();
    }

    /// <summary>
    /// Check current resources by resource type
    /// </summary>
    /// <param name="resourceType">Type of resources</param>
    /// <returns>Amount of current resources</returns>
    public int CheckResources(ResourceType resourceType)
    {
        if (_currentResources.TryGetValue(resourceType, out int amount))
        {
            //Debug.Log(amount + " " + resourceType.ToString());
            return amount;
        }
        else
        {
            Debug.LogError("Error checking resources for " + resourceType);
            return -1;
        }
    }

    /// <summary>
    /// Check if the building can be build with enough resources
    /// </summary>
    /// <returns>True or false</returns>
    public bool CheckBuild(Building building)
    {
        // Initial setup
        ResourceType[] resourceRequired = building.resourceRequired;
        int[] resourceAmount = building.resourceAmount;
        bool check = false;

        // If number length are not same, give an error
        if (resourceRequired.Length == resourceAmount.Length)
        {
            for (int i = 0; i < resourceRequired.Length; i++)
            {
                // if current resource are not sufficient, break the loop, check will still false
                if (resourceAmount[i] > CheckResources(resourceRequired[i]))
                    break;
                check = true;
            }
        }
        else
            Debug.LogError("Resource type and amount arrays are not in same length for" + building.buildingType);
        return check;
    }

    /// <summary>
    /// Spent resources, then spawn a building
    /// </summary>
    /// <param name="building">Building to spawn</param>
    public void SpawnBuilding(Building building)
    {
        // Initial setup
        ResourceType[] resourceRequired = building.resourceRequired;
        int[] resourceAmount = building.resourceAmount;

        // If number length are not same, give an error
        if (resourceRequired.Length == resourceAmount.Length)
        {

            for (int i = 0; i < resourceRequired.Length; i++)
            {
                // get amount of current resource by the resource type from the building, then deduct from the resource amoun
                // needed from the building (resourceAmount), then assign to currentResource[resourceType]
                _currentResources[resourceRequired[i]] = CheckResources(resourceRequired[i]) - resourceAmount[i];
            }
        }
        else
            Debug.LogError("Resource type and amount arrays are not in same length for" + building.buildingType);


        // BUG: the building scale is too small, for now manually set the position and tranform parent
        var spawned = Instantiate(building);
        spawned.transform.position = locationBuilding.position;
        spawned.transform.SetParent(locationBuilding);

        NextAction();
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
