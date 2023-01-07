using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Keep track of resources, effects, status, and NPCs
/// </summary>
public class GameManager : MonoBehaviour
{
    // Game manager said "It's Inspector time" and inspecting all over my brain
    [Header("Current community status")]
    [SerializeField] private int maxActionPoint = 3; // action points that player need to spend per day
    [SerializeField] [Range(0, 0.5f)] private float hungerIncreaseRate = 0.2f;
    [SerializeField] private Status[] _listStatus;

    [Header("Starting resources")]
    [SerializeField] private int cookedFood = 15;
    [SerializeField] private int wood = 25; // depending on player wanted to collect
    [SerializeField] private NPC[] listNPCs;

    [Header("Misc")]
    [SerializeField] private TransitionProperties[] transitions;
    [SerializeField] private GameObject[] interiorDesign;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private Transform locationBuilding;
    [SerializeField] private bool isTalkingToNPC; //useful toggle to focus on NPC dialogues later

    // private vars
    private Dictionary<ResourceType, int> _currentResources = new();
    private Dictionary<ResourceType, int> pendingResources = new();
    public int dayElapsed { get; private set; }
    public int actionPoint { get; private set; }
    public Dictionary<ResourceType, int> currentResources { get { return _currentResources; } }
    public Status[] listStatus { get { return _listStatus; } }

    // Object references
    private SlotManager slotManager;
    private GridSystem gridSystem;
    private Transition transition;

    void Awake()
    {
        // Object reference
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();
        gridSystem = slotManager.GetComponent<GridSystem>();

        // Add resources
        _currentResources.Add(ResourceType.Wood, 25);
        _currentResources.Add(ResourceType.Stone, 15);
        _currentResources.Add(ResourceType.Reed, 10);
        _currentResources.Add(ResourceType.Herb, 0);
        _currentResources.Add(ResourceType.Raw_Food, 0);
        _currentResources.Add(ResourceType.Cooked_Food, cookedFood);
        _currentResources.Add(ResourceType.Delicious_Food, 0);
        _currentResources.Add(ResourceType.Cloth, 5);

        // Set NPC all to idle
        for (int i = 0; i < listNPCs.Length; i++)
        {
            listNPCs[i].currentTask = TaskType.Idle;
        }

        // Instantiate stuff
        //Debug.Log(CheckResources(ResourceType.Wood) + " " + ResourceType.Wood.ToString());
        actionPoint = maxActionPoint;

        transition = GetComponent<Transition>();
        transition.StartTransition(transitions[0]);
    }

    

    /// <summary>
    /// Update next action.
    /// </summary>
    /// <param name="spentAction">Do you need to spent the AP?</param>
    public void NextAction(bool spentAction = true, bool updateEverything = true)
    {
        Debug.Log("Next action!");
        if (spentAction)
            actionPoint--;
        if (updateEverything)
            gameUI.UpdateNextAction();
    }

    /// <summary>
    /// Calculate everything for next day when this method is called
    /// </summary>
    public void NextDay()
    {
        // Consume food
        int consumedDeliciousFood = _listStatus[2].CheckFood(CheckResources(ResourceType.Delicious_Food), listNPCs.Length, hungerIncreaseRate*2);
        _currentResources[ResourceType.Delicious_Food] -= consumedDeliciousFood;

        // if delicious food doesn't cover all NPCs
        if (consumedDeliciousFood < listNPCs.Length)
        {
            _currentResources[ResourceType.Cooked_Food] -= _listStatus[2].CheckFood(CheckResources(ResourceType.Cooked_Food), listNPCs.Length - consumedDeliciousFood, hungerIncreaseRate);
        }

        // Update current stats
        for (int i = 0; i < listStatus.Length; i++)
        {
            _listStatus[i].UpdateStatus();
        }

        //if (CheckResources(ResourceType.Delicious_Food) > 0)
        //    _listStatus[2].CheckFood(CheckResources(ResourceType.Delicious_Food), listNPCs.Length, hungerIncreaseRate*2);

        // Check mood & wellbeing for game over
        if (_listStatus[1].currentAmount <= 0)
            Debug.Log("Game Over: Everyone is dead.");
        else if (_listStatus[0].currentAmount <= -1f)
            Debug.Log("Game Over: Overthrown leader ending.");
        else if (_listStatus[2].currentAmount <= 0)
        {
            _listStatus[1].currentAmount -= 0.2f;
            _listStatus[0].currentAmount -= 0.2f;
        }

        // Add the resources
        foreach (KeyValuePair<ResourceType, int> resource in pendingResources)
        {
            _currentResources[resource.Key] += resource.Value;
        }

        // Clear NPC tasks
        for (int i = 0; i < listNPCs.Length; i++)
        {
            if (CheckNPCCurrentTask(listNPCs[i].npcType) && listNPCs[i].currentTask != TaskType.Building)
                listNPCs[i].currentTask = TaskType.Idle;
        }

        // Refresh action points
        actionPoint = maxActionPoint;

        // Update for other classes
        NextAction(false);
        slotManager.UpdateNextDay();
        dayElapsed++;
    }

    /// <summary>
    /// Run this method to start the game
    /// </summary>
    public IEnumerator StartGame(Transform targetPos)
    {
        // cut delay by half before the player actually notice the change in BTS
        yield return new WaitForSeconds(transitions[0].delay / 2);

        // Teleport player to a desired location
        gridSystem.SetInteractionMode(2);
        gridSystem.ResizeWorld(targetPos);
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
        building.originalPos = locationBuilding.position;
        var spawned = Instantiate(building);
        spawned.transform.position = building.originalPos;
        spawned.transform.SetParent(locationBuilding);


        // Black smith task
        AssignTask(NPCType.Blacksmith, TaskType.Building);

        // State the building is "crafted" but not "placed",
        gameUI.isNotPlaced = true;

        // Go to next action
        NextAction();
    }

    /// <summary>
    /// Check NPC task if they do the certain task
    /// </summary>
    /// <param name="npc">The npc</param>
    /// <param name="taskType">TaskType</param>
    /// <returns>True or false</returns>
    public bool CheckNPCCurrentTask(NPCType npcType, TaskType taskType)
    {
        for (int i = 0; i < listNPCs.Length; i++)
        {
            NPC npc = listNPCs[i];

            if (npc.npcType == npcType && npc.currentTask == taskType)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Check if the NPC is doing something other than idle
    /// </summary>
    /// <param name="npcType">Type of NPC</param>
    /// <returns>Returns true if they are busy</returns>
    public bool CheckNPCCurrentTask(NPCType npcType)
    {
        for (int i = 0; i < listNPCs.Length; i++)
        {
            NPC npc = listNPCs[i];

            if (npc.npcType == npcType && npc.currentTask != TaskType.Idle)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Assign NPC a task
    /// </summary>
    /// <param name="npcType">What NPC</param>
    /// <param name="taskType">Task you want to give</param>
    public void AssignTask(NPCType npcType, TaskType taskType)
    {
        for (int i = 0; i < listNPCs.Length; i++)
        {
            if (listNPCs[i].npcType == npcType)
            {
                listNPCs[i].currentTask = taskType;
                break;
            }
        }
    }

    /// <summary>
    /// Clear specific NPC's current task
    /// </summary>
    /// <param name="npcType">Type of NPC</param>
    public void ClearNPCTask(NPCType npcType)
    {
        for (int i = 0; i < listNPCs.Length; i++)
        {
            if (listNPCs[i].npcType == npcType)
            {
                listNPCs[i].currentTask = TaskType.Idle;
                break;
            }
        }
    }

    /// <summary>
    /// Gain or lose resources
    /// </summary>
    /// <param name="resourceType">Type of resources</param>
    /// <param name="amount">Amount to deduct(-) or add(+)</param>
    public void ChangeValueResource(ResourceType resourceType, int amount)
    {
        if (!pendingResources.TryAdd(resourceType, amount))
            pendingResources[resourceType] += amount;
        foreach (var item in pendingResources)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }

    /// <summary>
    /// List out NPC are capable of this task
    /// </summary>
    /// <param name="taskType">Type of task</param>
    /// <returns>List of NPCs</returns>
    public List<NPCType> CheckNPCCapabilities(TaskType taskType)
    {
        List<NPCType> taskNPC = new();

        for (int i = 0; i < listNPCs.Length; i++)
        {
            NPC npc = listNPCs[i];

            for (int j = 0; j < npc.capableTasks.Length; j++)
            {
                if (npc.capableTasks[j] == taskType)
                {
                    taskNPC.Add(npc.npcType);
                    break;
                }
            }
        }

        return taskNPC;
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
    Cloth
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
    Clinic,
}
/// <summary>
/// Types of community statuses available
/// </summary>
public enum StatusType
{
    Mood,
    Wellbeing,
    Hunger
}

/// <summary>
/// Keeps status of the community
/// </summary>
[System.Serializable]
public class Status
{
    public StatusType statusType;
    public float currentAmount;
    public float minLimit;
    public float maxLimit;
    public float decayRate;
    public float[] indicatorStats;

    public void UpdateStatus()
    {
        currentAmount -= decayRate;
        
        // Stop currentAmount from overflowing
        if (currentAmount < minLimit)
            currentAmount = minLimit;
        else if (currentAmount > maxLimit)
            currentAmount = maxLimit;
    }

    /// <summary>
    /// Check food for each NPC
    /// </summary>
    /// <param name="totalFood">Total of food</param>
    /// <param name="npcAmount">Amount of NPCs</param>
    /// <param name="factor">Rate of hunger increase</param>
    /// <returns>Number of food consumed</returns>
    public int CheckFood(int totalFood, int npcAmount, float factor)
    {
        int foodAmount;

        if (totalFood >= npcAmount)
            foodAmount = npcAmount;
        else
            foodAmount = totalFood;

        currentAmount += foodAmount / npcAmount * factor;

        return foodAmount;
    }
}

/// <summary>
/// ResourceTask pair, because I don't want to deal with dictionary stuff
/// </summary>
[System.Serializable]
public class ResourceTask
{
    public ResourceType resourceType;
    public TaskType taskType;
    public int amount;

    public ResourceTask(ResourceType resourceType, TaskType taskType, int amount)
    {
        this.resourceType = resourceType;
        this.taskType = taskType;
        this.amount = amount;
    }
}