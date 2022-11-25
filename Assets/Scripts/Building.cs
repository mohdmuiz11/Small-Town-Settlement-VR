using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Building contains models and attributes needed for playing
/// </summary>
public class Building : XRGrabInteractable
{
    [SerializeField] private string buildingName;
    [SerializeField] [TextArea(5, 20)] private string buildingDescription;
    [SerializeField] private Texture2D buildingThumbnail;
    [SerializeField] private Transform playerTravelPos;

    private TableUI tableUI;
    private GameObject tableUIObj;
    private bool isOnGrid;
    private GridSystem gridSystem;

    protected override void Awake()
    {
        base.Awake();
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        tableUIObj = GameObject.Find("Building UI");
        tableUI = tableUIObj.GetComponent<TableUI>();
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        if (isOnGrid)
        {
            tableUIObj.SetActive(true);
            tableUI.setText(buildingName, buildingDescription, buildingThumbnail, playerTravelPos);
        }
    }

    // When travel mode is activated, the player should travel to this building
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        if (gridSystem.GetInteractionMode() == 2)
            gridSystem.ResizeWorld(playerTravelPos);
    }

    public void SetToGrid(bool condition)
    {
        isOnGrid = condition;
        if (!condition && tableUIObj != null)
        {
            tableUIObj.SetActive(false);
        }
    }

    // Prevent from grabbing when set to travel, other just be normal
    // BUG: the socket will produce hover mesh, which causes visual glitch
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        if (gridSystem.GetInteractionMode() == 2)
            return false;
        else
            return base.IsSelectableBy(interactor);
    }
}
