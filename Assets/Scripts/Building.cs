using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Building contains models and attributes needed for playing
/// </summary>
public class Building : XRGrabInteractable
{
    [Header("Building information")]
    [SerializeField] private BuildingType _buildingType;
    [SerializeField] [TextArea(5, 20)] private string _buildingDescription;
    [SerializeField] private Sprite _buildingThumbnail;
    [SerializeField] private Transform playerTravelPos;
    [SerializeField] private bool _requiresBuilderHut;

    [Header("Resource management")]
    [SerializeField] private ResourceType[] _resourceRequired;
    [SerializeField] private int[] _resourceAmount;
    [SerializeField] private int durationBuild;

    // private vars
    //private TableUI tableUI;
    private GridSystem gridSystem;
    private GameUI gameUI;
    private GameManager gameManager;
    public Vector3 originalPos;
    private bool hasPlaced;
    private bool socketHovered;
    private bool preventUpdate;
    public int duration { get; private set; }

    // public sort of vars
    public string buildingDescription { get { return _buildingDescription; } }
    public BuildingType buildingType { get { return _buildingType; } }
    public Sprite buildingThumbnail { get { return _buildingThumbnail; } }
    public ResourceType[] resourceRequired { get { return _resourceRequired; } }
    public int[] resourceAmount { get { return _resourceAmount; } }
    public float Angle { get; private set; }
    public bool isInConstruction { get; private set; }
    public bool hasBuild { get; private set; }
    public bool requiresBuilderHut { get { return _requiresBuilderHut; } }

    protected override void Awake()
    {
        base.Awake();

        // Default state
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        gameUI = GameObject.Find("Game UI").GetComponent<GameUI>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //tableUI = GameObject.Find("Table UI").GetComponent<TableUI>();

        // initial vars
        transform.localScale = Vector3.one * gridSystem.WidthGrid;
        Angle = 0;
    }

    /// <summary>
    /// Begin the building process
    /// </summary>
    public void TimeToBuild()
    {
        isInConstruction = true;
        interactionLayers = InteractionLayerMask.GetMask("Constructioned");
        duration = durationBuild;
        gameUI.isNotPlaced = false;
        gameUI.ShowStats(this, duration);
        gameManager.NextAction();
    }

    /// <summary>
    /// return condition of hasPlaced
    /// </summary>
    /// <returns>True or false</returns>
    public bool GetHasPlaced()
    {
        return hasPlaced;
    }
    
    /// <summary>
    /// Update after next day function
    /// </summary>
    public void UpdateNextDay()
    {
        if (duration != 0)
        {
            duration--;
            if (isInConstruction && duration > 0)
            {
                gameUI.UpdateStats(this, duration);
            }
            else if (duration == 0)
            {
                if (buildingType == BuildingType.Town_Hall)
                    gameManager.hasBuildTownHall = true;
                else if (buildingType == BuildingType.Workshop)
                    gameManager.hasBuildBuilderHut = true;
                gameUI.DestroyStats(this);
                
                // Send notification
                if (!hasBuild && isInConstruction)
                    gameManager.displayChanges += "The " + gameUI.EnumToReadableFormat(buildingType) + " has been built!\n";
                hasBuild = true;
                isInConstruction = false;
                gameManager.ClearNPCTask(NPCType.Blacksmith);
            }
        }
    }

    // When the building is hovered by player's controller or socket
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket"))
            socketHovered = true;
    }

    // Prevent constant update of calculating Angle to save more performance
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket"))
            socketHovered = false;
    }

    // To indicate if the building is placed
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket"))
        {
            hasPlaced = true;
            socketHovered = false;

            // Show info in the table UI
            if (!isInConstruction && !hasBuild)
                gameUI.ShowInfo(this);
        }
    }
    
    // When the building is out of socket
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket"))
        {
            gameUI.ShowInfo(false);
            hasPlaced = false;
            socketHovered = false;
        }
        else // if the player accidentally drop something
            StartCoroutine(ResetToOriginalPos());
    }

    // Wait for one second, and reset if not hasplaced
    IEnumerator ResetToOriginalPos()
    {
        yield return new WaitForSeconds(1);
        if (!hasPlaced)
        {
            transform.position = originalPos;
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    // Identify current direction of the building by bearings to snap into socket.
    private void CurrentBuildingDirection()
    {
        float angle = transform.eulerAngles.y;
        if (angle >= 315 || angle < 45) Angle = 0;
        else if (angle >= 45 && angle < 135) Angle = 90;
        else if (angle >= 135 && angle < 225) Angle = 180;
        else Angle = 270;
        //Debug.Log(Angle + " " + hasPlaced);
    }

    // When travel mode is activated, the player should travel to this building
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        //Debug.Log(gridSystem.interactionMode + " " + hasPlaced);
        //if (gridSystem.interactionMode == 2 && hasPlaced)
        //    gridSystem.ResizeWorld(playerTravelPos);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (socketHovered && !hasPlaced) //what 
            CurrentBuildingDirection();
    }

    // Prevent from grabbing when set to travel, other just be normal. This runs every frame though.
    // BUG: the socket will produce hover mesh, which causes visual glitch
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        //if (gridSystem.interactionMode == 2)
        //    return false;
        //else
            return base.IsSelectableBy(interactor);
    }
}
