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

    // Define GRID transform
    private GridSystem gridSystem;

    protected override void Awake()
    {
        base.Awake();

        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        HasPlaced = false;
    }

    public void SetCoordinate(int x, int z)
    {
        PosX = x;
        PosZ = z;
        actualX = x;
        actualZ = z;
    }

    // Get status of the building when it enters the socket
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Building building = args.interactableObject.colliders[0].GetComponentInParent<Building>();
        BuildingIsPlaced(true, building);
    }

    // Get status of the building when it exit from socket
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        Building building = args.interactableObject.colliders[0].GetComponentInParent<Building>();
        BuildingIsPlaced(false, building);
    }

    // Set conditions when the building is placed or not
    private void BuildingIsPlaced(bool placed, Building building)
    {
        if (building != null && gridSystem.GetInteractionMode() != 2)
        {
            building.SetToGrid(placed);
            if (placed)
                building.transform.parent = gridSystem.transform;
            HasPlaced = placed;
        }
        //Debug.Log(HasPlaced);
        //building.SetOriginalLayer();
    }
}
