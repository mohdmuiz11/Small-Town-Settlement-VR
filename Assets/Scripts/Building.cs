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
    //private GridSystem gridSystem;
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        //gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        tableUIObj = GameObject.Find("Building UI");
        tableUI = tableUIObj.GetComponent<TableUI>();
        rb = gameObject.GetComponent<Rigidbody>();
    }


    public void freezeAllMovement()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void unfreezeAllMovement()
    {
        rb.constraints = RigidbodyConstraints.None;
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


    public void SetToGrid(bool condition)
    {
        isOnGrid = condition;
        if (!condition && tableUIObj != null)
        {
            tableUIObj.SetActive(false);
        }
    }
}
