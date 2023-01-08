using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// The brain of GRID system. Management of slots, building, etc. are done by this class
/// </summary>
public class GridSystem : MonoBehaviour
{
    // Variables for game inspector
    [Header("World settings")]
    [SerializeField] private float worldSize = 100;
    [SerializeField] private GameObject fixedObjectGroup;
    [SerializeField] private GameObject hideWater;

    [Header("Interaction Layers")]
    [SerializeField] private InteractionLayerMask selectRoadLayer = 0; // for roads
    [SerializeField] private InteractionLayerMask selectTpLayer = 0; // teleportation in building
    public InteractionLayerMask selectDefaultLayer { get; private set; } // default int layer for both controller

    [Header("Controller settings")]
    [SerializeField] private float defaultLineWidth = 0.02f;
    [SerializeField] private GameObject grabberModel;
    [SerializeField] private GameObject controllerDefaultModel;
    [SerializeField] private float defaultMaxRaycastDist = 30;
    [SerializeField] private Gradient invisibleGradient;

    // Controller setup
    private XRBaseControllerInteractor leftControllerInteractor;
    private XRBaseControllerInteractor rightControllerInteractor;
    private XRBaseController rightController;
    private XRInteractorLineVisual rightHandLV;
    private Gradient originalValidGradient;
    private Gradient originalInvalidGradient;

    // Private vars
    private GameManager gameManager;
    private SlotManager slotManager;
    private Transform playerTransform;
    private GameObject buildModeObject;
    private Quaternion playerOriginRot;
    private Vector3 playerOriginPos;
    private float tableHeight;
    private bool usingGrabber;

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
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        buildModeObject = GameObject.Find("BuildMode");
        playerTransform = GameObject.Find("XR Rig").GetComponent<Transform>();
        leftControllerInteractor = GameObject.Find("LeftHand Controller").GetComponent<XRBaseControllerInteractor>();
        rightControllerInteractor = GameObject.Find("RightHand Controller").GetComponent<XRBaseControllerInteractor>();
        rightHandLV = rightControllerInteractor.GetComponent<XRInteractorLineVisual>();
        rightController = rightControllerInteractor.xrController as XRBaseController;
        selectDefaultLayer = leftControllerInteractor.interactionLayers;
        selectDefaultLayer = rightControllerInteractor.interactionLayers;

        // Initiate variables from the start of the game
        playerOriginPos = playerTransform.position;
        playerOriginRot = playerTransform.rotation;
        tableHeight = gameObject.transform.position.y;
        hasTraveled = false;

        // controller settings by default
        Debug.Log(controllerDefaultModel.ToString());
        originalValidGradient = rightHandLV.validColorGradient;
        originalInvalidGradient = rightHandLV.invalidColorGradient;
        usingGrabber = true;
        EnableGrabber(!usingGrabber);
    }

    public void ToggleGrabber()
    {
        EnableGrabber(!usingGrabber);
    }

    // Set interaction layer for both controllers, easy peasy
    public void SetControllerInteractionLayer(InteractionLayerMask layerMask)
    {
        // Dirty fix: left controller has no purpose other than teleportation
        if (layerMask == selectTpLayer || layerMask.value == 0)
            leftControllerInteractor.interactionLayers = layerMask;
        else
            leftControllerInteractor.interactionLayers = selectDefaultLayer;
        rightControllerInteractor.interactionLayers = layerMask;
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

    // enables to activate objects while hovering
    private void EnableHoverActivate(bool condition)
    {
        rightControllerInteractor.allowHoveredActivate = condition;
        leftControllerInteractor.allowHoveredActivate = condition;
    }

    // setting up grabber
    public void EnableGrabber(bool enabled)
    {
        if (enabled && !usingGrabber)
        {
            // set grabber's model
            rightController.model = grabberModel.transform;
            grabberModel.SetActive(enabled);
            controllerDefaultModel.SetActive(!enabled);
            rightHandLV.lineWidth = 0.05f;

            // hide line visuals
            rightHandLV.validColorGradient = invisibleGradient;
            rightHandLV.invalidColorGradient = invisibleGradient;
        }
        else if (!enabled && usingGrabber)
        {
            // set default model
            rightController.model = controllerDefaultModel.transform;
            grabberModel.SetActive(enabled);
            controllerDefaultModel.SetActive(!enabled);
            rightControllerInteractor.GetComponent<XRRayInteractor>().maxRaycastDistance = defaultMaxRaycastDist;

            // ayo
            rightHandLV.validColorGradient = originalValidGradient;
            rightHandLV.invalidColorGradient = originalInvalidGradient;
            rightHandLV.lineWidth = defaultLineWidth;
        }
        usingGrabber = enabled;
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
            //ConstraintAllBuildings(false);
            slotManager.SwitchSlot("Socket");
            SetControllerInteractionLayer(selectDefaultLayer);
            //EnableGrabber(true);

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
            //ConstraintAllBuildings(true);
            SetControllerInteractionLayer(selectRoadLayer);
            slotManager.SwitchSlot("Road");
            EnableHoverActivate(true);
            EnableGrabber(false);
            if (interactionMode == 2) mode = 2; //temporary fix
        }
        // Nak teleport
        else if (mode == 2 && interactionMode != 2)
        {
            //ConstraintAllBuildings(true);
            slotManager.ToggleHoverMeshSocket(false);
            slotManager.SwitchSlot("Teleport");
            EnableGrabber(false);

            // road -> travel
            if (interactionMode == 1)
            {
                SetControllerInteractionLayer(selectDefaultLayer);
            }
            EnableHoverActivate(true);
        }

        interactionMode = mode;
        gameManager.NextAction(false);
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
        hideWater.gameObject.SetActive(false);
        fixedObjectGroup.SetActive(true);
        SetControllerInteractionLayer(selectTpLayer);
        playerTransform.position = playerTravelPos.position;
        playerTransform.rotation = playerTravelPos.rotation;
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
        fixedObjectGroup.SetActive(false);
        hideWater.gameObject.SetActive(true);
        buildModeObject.SetActive(true);
        hasTraveled = false;
    }

    // For holding player's position just in case
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
}
