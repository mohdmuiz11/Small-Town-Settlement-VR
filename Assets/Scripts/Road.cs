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
    public int posX { get; private set; }
    public int posZ { get; private set; }
    public bool hasPlaced { get; private set; }

    public void SetCoordinate(int x, int z)
    {
        posX = x;
        posZ = z;
        actualX = x;
        actualZ = z;
    }

    protected override void Awake()
    {
        base.Awake();
        hasPlaced = false;
    }

    // When the controller hovers over to this gameObject, user can activate (by pressing trigger) to create road
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        if (!hasPlaced)
        {
            roadRenderer.sprite = roadType[1];
            hasPlaced = true;
        }
        else if (hasPlaced)
        {
            roadRenderer.sprite = roadType[0];
            hasPlaced = false;
        }
        
    }
}
