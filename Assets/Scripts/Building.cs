using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Building contains models and attributes needed for playing
/// </summary>
public class Building : XRGrabInteractable
{
    [Header("Building information")]
    [SerializeField] private string buildingName;
    [SerializeField] private BuildingType buildingType;
    [SerializeField] [TextArea(5, 20)] private string buildingDescription;
    [SerializeField] private Texture2D buildingThumbnail;
    [SerializeField] private Transform playerTravelPos;

    [Header("Resource management")]
    [SerializeField] private ResourceType[] resourceRequired;
    [SerializeField] private int[] resourceAmount;
    [SerializeField] private int durationBuild;

    private TableUI tableUI;
    private GridSystem gridSystem;
    private GameManager gameManager;
    private bool hasPlaced;
    private bool socketHovered;
    private bool preventUpdate;
    public float Angle { get; private set; }
    public bool isInConstruction { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // Default state
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        tableUI = GameObject.Find("Table UI").GetComponent<TableUI>();
        Angle = 0;
    }

    private void Start()
    {
        // scale to fit inside a slot
        transform.localScale = Vector3.one * gridSystem.WidthGrid;
    }

    /// <summary>
    /// Check if the building can be build with enough resources
    /// </summary>
    /// <returns>True or false</returns>
    public bool CheckBuild()
    {
        bool check = false;
        // If number length are not same, give an error
        if (resourceRequired.Length == resourceAmount.Length)
        {
            for (int i = 0; i < resourceRequired.Length; i++)
            {
                // if current resource are not sufficient, break the loop, check will still false
                Debug.Log(resourceAmount[i]);
                if (resourceAmount[i] > gameManager.CheckResources(resourceRequired[i]))
                    break;
                check = true;
            }
        }
        else
            Debug.LogError("Resource type and amount arrays are not in same length for" + buildingName);
        return check;
    }

    // When the building is hovered by player's controller or socket
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket"))
            socketHovered = true;

        // Show building's information to the table UI
        else if (interactorObj.gameObject.CompareTag("Player") && hasPlaced)
        {
            tableUI.gameObject.SetActive(true);
            tableUI.setText(buildingName, buildingDescription, buildingThumbnail);
        }
    }

    // Prevent constant update of calculating Angle to save more performance
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket") && hasPlaced)
            preventUpdate = true;
    }

    // To indicate if the building is placed
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket"))
        {
            preventUpdate = false;
            hasPlaced = true;
        }
    }
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        var interactorObj = args.interactorObject as XRBaseInteractor;

        if (interactorObj.gameObject.CompareTag("Socket") && gridSystem.interactionMode != 2)
            hasPlaced = false;
    }

    // Identify current direction of the building by bearings to snap into socket.
    private void CurrentBuildingDirection()
    {
        float angle = transform.eulerAngles.y;
        if (angle >= 315 || angle < 45) Angle = 0;
        else if (angle >= 45 && angle < 135) Angle = 90;
        else if (angle >= 135 && angle < 225) Angle = 180;
        else Angle = 270;
        //Debug.Log(Angle);
    }

    // When travel mode is activated, the player should travel to this building
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        //Debug.Log(gridSystem.interactionMode + " " + hasPlaced);
        if (gridSystem.interactionMode == 2 && hasPlaced)
            gridSystem.ResizeWorld(playerTravelPos);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (socketHovered && !preventUpdate)
            CurrentBuildingDirection();
    }

    // Prevent from grabbing when set to travel, other just be normal. This runs every frame though.
    // BUG: the socket will produce hover mesh, which causes visual glitch
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        if (gridSystem.interactionMode == 2)
            return false;
        else
            return base.IsSelectableBy(interactor);
    }
}
