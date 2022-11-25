using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// The brain of GRID system. Management of slots, building, etc. are done by this class
/// </summary>
public class GridSystem : MonoBehaviour
{
    // Variables for game inspector
    [SerializeField] private int gridCount = 18;
    [SerializeField] private float tableLength = 1.0f;
    [SerializeField] private GameObject socketBuildingPrefab;
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private float worldSize = 100;

    // Set interaction layers from the inspector, cus idk how to setup lol
    [SerializeField] private InteractionLayerMask selectRoadLayer = 0;
    [SerializeField] private InteractionLayerMask selectTpLayer = 0;
    private InteractionLayerMask selectDefaultLayer = 0;

    // Private vars
    private XRBaseControllerInteractor leftController;
    private XRBaseControllerInteractor rightController;
    private Transform playerTransform;
    private GameObject buildModeObject;
    private Quaternion playerOriginRot;
    private Vector3 playerOriginPos;
    private Vector3 firstGridPos;
    private float widthGrid;
    private float tableHeight;
    private bool hasTraveled;
    private int interactionMode = 0;

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
        widthGrid = GetWidthGrid();
        firstGridPos = GetFirstGridPos();
        SpawnSlotInGrid();
        hasTraveled = false;
    }

    // 
    private Vector3 GetFirstGridPos()
    {
        float pos = 0 - (widthGrid * (gridCount / 2)) + (widthGrid / 2);
        return new Vector3(pos, gameObject.transform.position.y, pos);
    }

    private float GetWidthGrid()
    {
        return tableLength / (gridCount + 2);
    }

    private void SpawnSlotInGrid()
    {
        Vector3 pos = firstGridPos;
        for (int z=0; z < gridCount; z++)
        {
            for (int x=0; x < gridCount; x++)
            {
                GameObject gridSpawn = Instantiate(socketBuildingPrefab, pos, socketBuildingPrefab.transform.rotation, transform);
                gridSpawn.GetComponent<SocketBuilding>().SetCoordinate(x, z);
                pos.x += widthGrid;
            }
            pos.x = firstGridPos.x;
            pos.z += widthGrid;
        }
    }


    // Replace all slot in the GRID system from targetPrefab to replacePrefab
    private void ReplaceSlot(GameObject targetPrefab, GameObject replacePrefab)
    {
        GameObject[] slots = GameObject.FindGameObjectsWithTag(targetPrefab.tag);

        for (int i = 0; i < slots.Length; i++)
        {
            IGridCoordinate targetCoordinate = slots[i].GetComponent<IGridCoordinate>();

            // As long as the slot is not empty, replace them
            if (!targetCoordinate.HasPlaced)
            {
                SlotSetCoordinate(replacePrefab, slots[i].transform.position, targetCoordinate);
                Destroy(slots[i]);
            }
        }

        // Set allowHoveredActivate state to each controller depending on replacePrefab
        if (replacePrefab.tag == "Slot")
        {
            EnableHoverActivate(false);
        }
        else if (replacePrefab.tag == "Road")
        {
            EnableHoverActivate(true);
        }
    }

    // Set both controller to activate interactables without grabbing it first via hover
    private void EnableHoverActivate(bool condition)
    {
        rightController.allowHoveredActivate = condition;
        leftController.allowHoveredActivate = condition;
    }

    // Set all sockets to not show hover mesh when travel mode is enabled. Very performance intensive
    private void ToggleHoverMeshSocket(bool isHoverable)
    {
        GameObject[] sockets = GameObject.FindGameObjectsWithTag("Slot");

        for (int i = 0; i < sockets.Length; i++)
        {
            sockets[i].GetComponent<SocketBuilding>().showInteractableHoverMeshes = isHoverable;
        }
    }

    // Set coordinate for slot, need gameObject, position, and coordinate
    private void SlotSetCoordinate(GameObject objPrefab, Vector3 worldPos, IGridCoordinate coordinate)
    {
        GameObject replaceSlot = Instantiate(objPrefab, worldPos, objPrefab.transform.rotation, transform);
        replaceSlot.GetComponent<IGridCoordinate>().SetCoordinate(coordinate.PosX, coordinate.PosZ);
    }

    // Set interaction layer for both controllers, easy peasy
    private void SetControllerInteractionLayer(InteractionLayerMask layerMask)
    {
        leftController.interactionLayers = layerMask;
        rightController.interactionLayers = layerMask;
    }

    /// <summary>
    /// Set the GRID system to real world size
    /// </summary>
    /// <param name="playerTravelPos">Pass a travel position</param>
    public void ResizeWorld(Transform playerTravelPos)
    {
        // Make sure road selection is reset back to socket building
        if (interactionMode == 1)
            ReplaceSlot(roadPrefab, socketBuildingPrefab);

        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = new Vector3(worldSize, worldSize, worldSize);
        buildModeObject.SetActive(false);
        SetControllerInteractionLayer(selectTpLayer);
        playerTransform.position = playerTravelPos.position;
        SetInteractionMode(2);
        hasTraveled = true;
    }

    /// <summary>
    /// Set GRID system to table size, and teleport player to origin point
    /// </summary>
    public void SetOriginalSize()
    {
        SetControllerInteractionLayer(selectDefaultLayer);
        playerTransform.position = playerOriginPos;
        playerTransform.rotation = playerOriginRot;
        gameObject.transform.position = new Vector3(0, tableHeight, 0);
        gameObject.transform.localScale = Vector3.one;
        buildModeObject.SetActive(true);
        hasTraveled = false;
    }

    /// <summary>
    /// Get value of current interaction mode
    /// </summary>
    /// <returns>Current interaction mode</returns>
    public int GetInteractionMode()
    {
        return interactionMode;
    }

    /// <summary>
    /// Set interaction mode. Available mode: 0 - build mode, 1 - road mode, 2 - view mode (inside map)
    /// </summary>
    /// <param name="mode">Set mode in integer, between 0-2</param>
    public void SetInteractionMode(int mode)
    {
        // Nak letak Building
        if (mode == 0 && interactionMode != 0)
        {
            ConstraintAllBuildings(false);
            
            // road -> building
            if (interactionMode == 1)
            {
                ReplaceSlot(roadPrefab, socketBuildingPrefab);
                SetControllerInteractionLayer(selectDefaultLayer);
            }
            // travel -> building
            else if (interactionMode == 2)
            {
                EnableHoverActivate(false);
                ToggleHoverMeshSocket(true);
                if (hasTraveled) SetOriginalSize();
            }
        }
        // Nak letak road
        else if (mode == 1 && interactionMode != 1 && !hasTraveled)
        {
            ConstraintAllBuildings(true);
            SetControllerInteractionLayer(selectRoadLayer);
            ReplaceSlot(socketBuildingPrefab, roadPrefab);
            if (interactionMode == 2) mode = 2; //temporary fix
        }
        // Nak teleport
        else if (mode == 2 && interactionMode != 2)
        {
            ConstraintAllBuildings(true);
            ToggleHoverMeshSocket(false);
            
            // road -> travel
            if (interactionMode == 1)
            {
                ReplaceSlot(roadPrefab, socketBuildingPrefab);
                SetControllerInteractionLayer(selectDefaultLayer);
            }
            EnableHoverActivate(true);
        }

        interactionMode = mode;
    }

    // Freeze or unfreeze all building movement
    private void ConstraintAllBuildings(bool constraint)
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        for (int i = 0; i < buildings.Length; i++)
        {
            if (constraint)
                buildings[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            else
                buildings[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    // For holding player's position just in case
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
}
