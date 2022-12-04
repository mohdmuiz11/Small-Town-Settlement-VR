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

    // When the controller hovers over to this gameObject, user can activate (by pressing trigger) to create road
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        if (!HasPlaced)
        {
            HasPlaced = true;
            slotManager.UpdateAllRoads();
        }
        else if (HasPlaced)
        {
            roadRenderer.sprite = roadType[0];
            HasPlaced = false;
            slotManager.UpdateAllRoads();
        }
        //Debug.Log(HasPlaced);
        
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
                if (angle - anchor.eulerAngles.y == 180 && !straight)
                    straight = true;
                else if ((angle - anchor.eulerAngles.y == 270 || straight) && placed > 1)
                    angle += 90;

                if (!results[2] && placed > 2)
                    angle += 180;

                // zero out angle if 360, else normal la
                if (angle > 300)
                {
                    anchor.eulerAngles = new Vector3(0, angle - 360, 0);
                    angle = 0;
                }
                else
                    anchor.eulerAngles = new Vector3(0, angle, 0);
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
