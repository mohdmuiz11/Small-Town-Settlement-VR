using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// To display road on the GRID system, using XRBaseInteractable
/// </summary>
public class Road : XRBaseInteractable, IGridCoordinate
{

    // Store types of road(turns, straight, junction) and renderer
    [SerializeField] private Sprite[] roadType;
    [SerializeField] private SpriteRenderer roadRenderer;
    [SerializeField] private Transform anchor;

    // For debug purposes
    [SerializeField] private int actualX;
    [SerializeField] private int actualZ;

    // Other vars
    private SlotManager slotManager;
    private bool hasInteracted;

    //Store coordinate of the grid
    public int PosX { get; private set; }
    public int PosZ { get; private set; }
    public bool HasPlaced { get; private set; }

    public void SetCoordinate(int x, int z)
    {
        PosX = x;
        PosZ = z;

        // For debug purposes
        actualX = x;
        actualZ = z;
    }

    protected override void Awake()
    {
        base.Awake();
        slotManager = GameObject.Find("GRID System").GetComponent<SlotManager>();
        HasPlaced = false;
    }

    // Trying to fix not spraying the road issue
    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);

        if (slotManager.controllerActivate > 0 && !hasInteracted)
            PlacingRoad();
    }

    // Trying to fix not spraying the road issue
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);

        if (hasInteracted)
            hasInteracted = false;
    }

    // When the controller hovers over to this gameObject, user can activate (by pressing trigger) to create road
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        // If HasPlaced, remove road, else add them
        slotManager.controllerActivate = HasPlaced ? 1 : 2;
        hasInteracted = true;
        PlacingRoad();
    }

    private void PlacingRoad()
    {
        if (!HasPlaced && slotManager.controllerActivate != 1)
        {
            HasPlaced = true;
            slotManager.UpdateAllRoads();
        }
        else if (HasPlaced && slotManager.controllerActivate != 2)
        {
            roadRenderer.sprite = roadType[0];
            HasPlaced = false;
            slotManager.UpdateAllRoads();
        }
    }

    // Trying to fix not spraying the road issue
    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);
        slotManager.controllerActivate = 0;
    }

    // Since road is based on XRBaseInteractable, grab is enabled by default, so need to disable it first
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return false;
    }

    /// <summary>
    /// Update road patterns
    /// </summary>
    public void RoadUpdate()
    {
        float angle = 0;
        int placed = 0;
        bool straight = false; // straight roads

        bool[] results = slotManager.CheckSurrounding(PosX, PosZ);

        // Check results. Another mess of a code. Maybe dictionary can help?
        for (int i = 0; i < results.Length; i++)
        {

            if (results[i])
            {
                placed++;
                if (angle - anchor.localEulerAngles.y == 180 && !straight)
                    straight = true;
                else if ((angle - anchor.localEulerAngles.y == 270 || straight) && placed > 1)
                    angle += 90;

                if (!results[2] && placed > 2)
                    angle += 180;

                // zero out angle if 360, else normal la
                if (angle > 300)
                {
                    anchor.localEulerAngles = new Vector3(0, angle - 360, 0);
                    angle = 0;
                }
                else
                    anchor.localEulerAngles = new Vector3(0, angle, 0);
            }
            angle += 90;
        }


        if (straight && placed == 2) // if road are straight
            roadRenderer.sprite = roadType[5];
        else if (placed > 0) // normal selection
            roadRenderer.sprite = roadType[placed];
        else
            roadRenderer.sprite = roadType[1]; // default
    }
}
