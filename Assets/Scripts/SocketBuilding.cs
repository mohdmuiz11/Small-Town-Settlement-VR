using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A socket to hold building in place. A slot for GRID system using XRSocketInteractor
/// </summary>
public class SocketBuilding : XRSocketInteractor, IGridCoordinate
{
    // For debug purposes
    [SerializeField] private int actualX;
    [SerializeField] private int actualZ;    

    //Store coordinate of the grid
    public int PosX { get; private set; }
    public int PosZ { get; private set; }
    public bool HasPlaced { get; private set; }

    // Private vars
    private GridSystem gridSystem;
    private SlotManager slotManager;
    private Building buildingStats;
    private bool buildingHovered;

    protected override void Awake()
    {
        base.Awake();

        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        slotManager = gridSystem.GetComponent<SlotManager>();
        HasPlaced = false;
    }

    public void SetCoordinate(int x, int z)
    {
        PosX = x;
        PosZ = z;
        actualX = x;
        actualZ = z;
    }

    // get angle information from building angles
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        var interactableObj = args.interactableObject as XRBaseInteractable;

        if (interactableObj.gameObject.CompareTag("Building"))
        {
            buildingStats = interactableObj.gameObject.GetComponent<Building>();
            buildingHovered = true;
        }
    }

    // do something after not hovered
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        buildingHovered = false;
        buildingStats = null;
    }

    // Get status of the building when it enters the socket
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        var interactableObj = args.interactableObject as XRBaseInteractable;

        BuildingIsPlaced(true, interactableObj);
        slotManager.UpdateAllRoads();
        buildingHovered = false;
        buildingStats = null;
    }

    // Get status of the building when it exit from socket
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        var interactableObj = args.interactableObject as XRBaseInteractable;

        BuildingIsPlaced(false, interactableObj);
        slotManager.UpdateAllRoads();
        buildingStats = null;
    }

    // get angle from the current building
    public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractor(updatePhase);


        if (buildingHovered && !buildingStats.GetHasPlaced())
        {
            Debug.Log(buildingStats.GetHasPlaced());
            float angle = buildingStats.Angle - gridSystem.gameObject.transform.eulerAngles.y;
            attachTransform.localEulerAngles = new Vector3(0, angle, 0);
            Debug.Log(angle + " Socket");
        }

    }

    // Set conditions when the building is placed or not
    private void BuildingIsPlaced(bool placed, XRBaseInteractable interactable)
    {
        GameObject building = interactable.gameObject;

        if (building.CompareTag("Building"))
        {
            if (placed)
                building.transform.SetParent(gridSystem.transform);
            //else
            //    building.transform.SetParent(null); // how is this so buggy
            HasPlaced = placed;
        }
        //Debug.Log(HasPlaced);
        //building.SetOriginalLayer();
    }
}
