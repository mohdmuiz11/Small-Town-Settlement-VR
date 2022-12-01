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

    // For debug purposes
    [SerializeField] private int actualX;
    [SerializeField] private int actualZ;

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
        HasPlaced = false;
    }

    // When the controller hovers over to this gameObject, user can activate (by pressing trigger) to create road
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        if (!HasPlaced)
        {
            roadRenderer.sprite = roadType[1];
            HasPlaced = true;
        }
        else if (HasPlaced)
        {
            roadRenderer.sprite = roadType[0];
            HasPlaced = false;
        }
        //Debug.Log(HasPlaced);
        
    }

    // Since road is based on XRBaseInteractable, grab is enabled by default, so need to disable it first
    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return false;
    }
}
