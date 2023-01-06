using UnityEngine;

/// <summary>
/// What is NPC's current tasks?
/// </summary>
public enum TaskType { 
    Idle,
    Building,
    Gather_Wood,
    Gather_Stone,
    Gather_Reeds,
    Gather_Herbs,
    Gather_Meat,
    Gather_Fish,
    Cooking,
    Healing
}

public enum NPCType
{
    Woodcutter,
    Blacksmith,
    Hunter,
    Fisherman,
    Chef,
    Healer,
    Player
}

/// <summary>
/// Details of an NPC
/// </summary>
public class NPC : MonoBehaviour
{
    [SerializeField] private string _npcName;
    [SerializeField] private NPCType _npcType;
    public string npcName { get { return _npcName; } }
    public NPCType npcType { get { return _npcType; } }
    public BuildingType currentLocation;
    public TaskType currentTask;
    public TaskType[] capableTasks;
    public int taskDuration = 0;
    [Range(0,1)] public float wellbeing = 0.8f;
    public bool isSick = false;

    /// <summary>
    /// Check if the NPC is capable at this task
    /// </summary>
    /// <param name="taskType">TaskType</param>
    /// <returns>True or false</returns>
    public bool IsCapable(TaskType taskType)
    {
        for (int i = 0; i < capableTasks.Length; i++)
        {
            if (capableTasks[i] == taskType)
                return true;
        }
        return false;
    }
}
