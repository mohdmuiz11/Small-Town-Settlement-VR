using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Side event types available for SideEvent
/// </summary>
public enum SideEventType { Forest, Fishing, Start, Obstacle }

/// <summary>
/// Side events that the player can partake.
/// </summary>
public class SideEvent : XRBaseInteractable, IGridCoordinate
{
    // Inspector usage
    [Header("Event details")]
    [SerializeField] private SideEventType eventType;
    [SerializeField] [TextArea(5, 20)] private string description;

    [Header("Object instance")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float sizePrefab = 1.5f;
    [SerializeField] private int minSpawn = 2;
    [SerializeField] private int maxSpawn = 5;
    [SerializeField] [Range(0, 0.2f)] private float marginSlot = 0.2f;

    [Header("Misc")]
    [SerializeField] private Transform playerTravelPos;
    [SerializeField] private int actualX;
    [SerializeField] private int actualZ;

    // Private vars
    private GridSystem gridSystem;
    private BoxCollider boxCollider;
    private GameUI gameUI;
    private bool isCompleted;

    // Interfaces
    public int PosX { get; private set; }
    public int PosZ { get; private set; }
    public bool HasPlaced { get; private set; }

    // Set coordinate
    public void SetCoordinate(int x, int z)
    {
        PosX = x;
        PosZ = z;
        // Debug purposes
        actualX = x;
        actualZ = z;
    }

    protected override void Awake()
    {
        base.Awake();

        // Default state
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        gameUI = GameObject.Find("Game UI").GetComponent<GameUI>();
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        // scale to fit inside a slot
        transform.localScale = Vector3.one * gridSystem.WidthGrid;
        GenerateRandomObjects();
        HasPlaced = true;
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        
        if (gridSystem.interactionMode == 2)
        {
            //tableUI.gameObject.SetActive(true);
            //tableUI.setText(eventType.ToString(), description, null);
            Debug.Log("TODO: Fix this");
        }
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        if (gridSystem.interactionMode == 2 && !isCompleted)
        {
            gridSystem.ResizeWorld(playerTravelPos);
        }
    }

    private void DisableBoxCollider(bool condition)
    {
        boxCollider.isTrigger = condition;
    }

    // Since event is based on XRBaseInteractable, grab is enabled by default, so need to disable it first
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return false;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (gridSystem.hasTraveled)
            DisableBoxCollider(true);
        else
            DisableBoxCollider(false);
    }

    // Everything in randomized
    private void GenerateRandomObjects()
    {
        // Random spawn and place to store pos
        int spawnSize = Random.Range(minSpawn, maxSpawn);
        float setDistance = 1f / (spawnSize + 1f) * (1f - marginSlot);
        //Debug.Log(setDistance + " " + spawnSize);
        List<Vector2> posList = new List<Vector2>();
        posList.Add(Vector2.zero);

        for (int i = 0; i < spawnSize; i++)
        {
            // Spawn object first
            int ri = Random.Range(0, prefabs.Length);
            GameObject prefab = Instantiate(prefabs[ri], transform);

            // Spare some distance between prefabs
            Vector2 correctPos;
            bool canSpawn = false;

            do // check pos if it is valid from other pos
            {
                correctPos = RandomLocation();
                foreach (Vector2 _pos in posList)
                {
                    // if distance is less, false altogether, also what the 
                    if (Vector2.Distance(correctPos, _pos) < setDistance)
                    {
                        canSpawn = false;
                        break;
                    }
                    else canSpawn = true;
                }
            }
            while (!canSpawn);

            posList.Add(correctPos);

            //for (int p = 0; p < posList.Length; p++)
            //{
            //    while (Vector2.Distance(correctPos, posList[p]) < 0.2f && posList[p] != posList[i])
            //        correctPos = RandomLocation();
            //    posList[i] = correctPos;
            //}

            // Define rotation and size factor
            float rot = Random.Range(0, 360f);
            float sizeFactor = Random.Range(0.75f, 1.25f);

            // Apply rotation, position, and size
            prefab.transform.localPosition = new Vector3(posList[i+1].x, 0, posList[i+1].y);
            prefab.transform.eulerAngles = new Vector3(0, rot, 0);
            prefab.transform.localScale = Vector3.one * (sizePrefab / 10) * sizeFactor;
        }
    }

    // Define random position in Vector2 (x=x, y=z)
    private Vector2 RandomLocation()
    {
        float posX = Random.Range(-0.5f + marginSlot, 0.5f - marginSlot);
        float posZ = Random.Range(-0.5f + marginSlot, 0.5f - marginSlot);

        return new Vector2(posX, posZ);
    }
}
