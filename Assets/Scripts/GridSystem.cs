using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// The brain of GRID system. Management of slots, building, etc. are done by this class
/// </summary>
public class GridSystem : MonoBehaviour
{
    // Variables for game inspector
    [SerializeField] private float worldSize = 100;

    // Set interaction layers from the inspector, cus idk how to setup lol
    [SerializeField] private InteractionLayerMask selectRoadLayer = 0; // for roads
    [SerializeField] private InteractionLayerMask selectTpLayer = 0; // teleportation in building
    private InteractionLayerMask selectDefaultLayer = 0; // default int layer for both controller

    // Private vars
    private XRBaseControllerInteractor leftController;
    private XRBaseControllerInteractor rightController;
    private Grabber grabber;
    private SlotManager slotManager;
    private Transform playerTransform;
    private GameObject buildModeObject;
    private Quaternion playerOriginRot;
    private Vector3 playerOriginPos;
    private float tableHeight;

    /// <summary>
    /// Check if the player has traveled to real world.
    /// </summary>
    public bool hasTraveled { get; private set; }

    /// <summary>
    /// Get current interaction mode. Available mode: 0 - build mode, 1 - road mode, 2 - teleport mode
    /// </summary>
    public int interactionMode { get; private set; } = 0;

    /// <summary>
    /// Get width of a grid to have same scale as props.
    /// </summary>
    public float WidthGrid { get; private set; }

    private void Awake()
    {
        slotManager = GetComponent<SlotManager>();
        WidthGrid = slotManager.WidthGrid;
    }

    void Start()
    {
        // Find gameobjects to reference
        buildModeObject = GameObject.Find("BuildMode");
        playerTransform = GameObject.Find("XR Rig").GetComponent<Transform>();
        leftController = GameObject.Find("LeftHand Controller").GetComponent<XRBaseControllerInteractor>();
        rightController = GameObject.Find("RightHand Controller").GetComponent<XRBaseControllerInteractor>();
        selectDefaultLayer = leftController.interactionLayers;
        selectDefaultLayer = rightController.interactionLayers;

        // Initiate variables from the start of the game
        playerOriginPos = playerTransform.position;
        playerOriginRot = playerTransform.rotation;
        tableHeight = gameObject.transform.position.y;
        hasTraveled = false;
    }

    // Set interaction layer for both controllers, easy peasy
    private void SetControllerInteractionLayer(InteractionLayerMask layerMask)
    {
        // Dirty fix: left controller has no purpose other than teleportation
        if (layerMask == selectTpLayer)
            leftController.interactionLayers = layerMask;
        else
            leftController.interactionLayers = selectDefaultLayer;
        rightController.interactionLayers = layerMask;
    }

    // Freeze or unfreeze all building movement
    private void ConstraintAllBuildings(bool constraint)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        for (int i = 0; i < buildings.Length; i++)
        {
            Rigidbody rbBuilding = buildings[i].GetComponent<Rigidbody>();

            if (constraint)
                rbBuilding.constraints = RigidbodyConstraints.FreezeAll;
            else
                rbBuilding.constraints = RigidbodyConstraints.None;
        }
    }

    private void EnableHoverActivate(bool condition)
    {
        rightController.allowHoveredActivate = condition;
        leftController.allowHoveredActivate = condition;
    }

    /// <summary>
    /// Set interaction mode. Available mode: 0 - build mode, 1 - road mode, 2 - teleport mode
    /// </summary>
    /// <param name="mode">Set mode in integer, between 0-2</param>
    public void SetInteractionMode(int mode)
    {
        // Nak letak Building
        if (mode == 0 && interactionMode != 0)
        {
            ConstraintAllBuildings(false);
            slotManager.SwitchSlot("Socket");
            SetControllerInteractionLayer(selectDefaultLayer);

            // travel -> building
            if (interactionMode == 2)
            {
                EnableHoverActivate(false);
                slotManager.ToggleHoverMeshSocket(true);
                if (hasTraveled) SetOriginalSize();
            }
        }
        // Nak letak road
        else if (mode == 1 && interactionMode != 1 && !hasTraveled)
        {
            ConstraintAllBuildings(true);
            SetControllerInteractionLayer(selectRoadLayer);
            slotManager.SwitchSlot("Road");
            EnableHoverActivate(true);
            if (interactionMode == 2) mode = 2; //temporary fix
        }
        // Nak teleport
        else if (mode == 2 && interactionMode != 2)
        {
            ConstraintAllBuildings(true);
            slotManager.ToggleHoverMeshSocket(false);
            slotManager.SwitchSlot("Teleport");

            // road -> travel
            if (interactionMode == 1)
            {
                SetControllerInteractionLayer(selectDefaultLayer);
            }
            EnableHoverActivate(true);
        }

        interactionMode = mode;
    }

    /// <summary>
    /// Set the GRID system to real world size
    /// </summary>
    /// <param name="playerTravelPos">Pass a travel position</param>
    public void ResizeWorld(Transform playerTravelPos)
    {
        // Make sure road selection is reset back to socket building
        //if (interactionMode == 1)
        //    slotManager.SwitchSlot("Socket");

        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = new Vector3(worldSize, worldSize, worldSize);
        buildModeObject.SetActive(false);
        SetControllerInteractionLayer(selectTpLayer);
        playerTransform.position = playerTravelPos.position;
        hasTraveled = true;
    }

    /// <summary>
    /// Set GRID system to table size, and teleport player to origin point
    /// </summary>
    public void SetOriginalSize()
    {
        playerTransform.position = playerOriginPos;
        playerTransform.rotation = playerOriginRot;
        gameObject.transform.position = new Vector3(0, tableHeight, 0);
        gameObject.transform.localScale = Vector3.one;
        buildModeObject.SetActive(true);
        hasTraveled = false;
    }

    // For holding player's position just in case
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
}
