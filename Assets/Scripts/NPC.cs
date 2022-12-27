using UnityEngine;

/// <summary>
/// What is NPC's current tasks?
/// </summary>
public enum CurrentTask { 
    Idle,
    Building,
    Gather_Wood,
    Gather_Stone,
    Gather_Reeds,
    Gather_Meat,
    Gather_Fish,
    Cooking,
    Healing
}

/// <summary>
/// Details of an NPC
/// </summary>
public class NPC : MonoBehaviour
{
    [SerializeField] private string _npcName;
    public string npcName { get { return _npcName; } }
    public BuildingType currentLocation;
    public CurrentTask currentTask;
    public int taskDuration = 0;
    [Range(0,1)] public float wellbeing = 0.8f;
    public bool isSick = false;
}
