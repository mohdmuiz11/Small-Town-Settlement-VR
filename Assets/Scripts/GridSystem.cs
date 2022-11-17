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

    // Private vars
    private XRBaseControllerInteractor leftController;
    private XRBaseControllerInteractor rightController;
    private ControllerLayerSelect leftControllerLayerSelect;
    private ControllerLayerSelect rightControllerLayerSelect;
    private Transform playerTransform;
    private GameObject buildModeObject;
    private Vector3 playerOriginPos;
    private Vector3 firstGridPos;
    private float widthGrid;
    private float tableHeight;

    /// <summary>
    /// Set interaction mode. Available mode: 0 - build mode, 1 - road mode, 2 - view mode (inside map)
    /// </summary>
    private int interactionMode = 0;

    private GameObject[] buildings;

    void Start()
    {
        // Find gameobjects to reference
        buildModeObject = GameObject.Find("BuildMode");
        playerTransform = GameObject.Find("XR Rig").GetComponent<Transform>();
        leftController = GameObject.Find("LeftHand Controller").GetComponent<XRBaseControllerInteractor>();
        rightController = GameObject.Find("RightHand Controller").GetComponent<XRBaseControllerInteractor>();
        leftControllerLayerSelect = GameObject.Find("LeftHand Controller").GetComponent<ControllerLayerSelect>();
        rightControllerLayerSelect = GameObject.Find("RightHand Controller").GetComponent<ControllerLayerSelect>();
        buildings = GameObject.FindGameObjectsWithTag("Building");

        // Initiate variables from the start of the game
        playerOriginPos = playerTransform.position;
        tableHeight = gameObject.transform.position.y;
        widthGrid = GetWidthGrid();
        firstGridPos = GetFirstGridPos();
        SpawnSlotInGrid();
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
            if (!targetCoordinate.hasPlaced)
            {
                SlotSetCoordinate(replacePrefab, slots[i].transform.position, targetCoordinate);
                Destroy(slots[i]);
            }
        }

        // Set allowHoveredActivate state to each controller depending on replacePrefab
        if (replacePrefab.tag == "Slot")
        {
            rightController.allowHoveredActivate = false;
            leftController.allowHoveredActivate = false;
        }
        else if (replacePrefab.tag == "Road")
        {
            rightController.allowHoveredActivate = true;
            leftController.allowHoveredActivate = true;
        }
    }

    // Set coordinate for slot, need gameObject, position, and coordinate
    private void SlotSetCoordinate(GameObject objPrefab, Vector3 worldPos, IGridCoordinate coordinate)
    {
        GameObject replaceSlot = Instantiate(objPrefab, worldPos, objPrefab.transform.rotation, transform);
        replaceSlot.GetComponent<IGridCoordinate>().SetCoordinate(coordinate.posX, coordinate.posZ);
    }

    // Set by buttons
    public void ResizeWorld(Transform playerTravelPos)
    {
        if (interactionMode == 1)
            ReplaceSlot(roadPrefab, socketBuildingPrefab);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = new Vector3(worldSize, worldSize, worldSize);
        buildModeObject.SetActive(false);
        leftControllerLayerSelect.SetTeleportableLayer();
        rightControllerLayerSelect.SetTeleportableLayer();
        playerTransform.position = playerTravelPos.position;
        SetInteractionMode(2);
    }
    public void OriginalSize()
    {
        leftControllerLayerSelect.SetOriginalLayer();
        rightControllerLayerSelect.SetOriginalLayer();
        playerTransform.position = playerOriginPos;
        gameObject.transform.position = new Vector3(0, tableHeight, 0);
        gameObject.transform.localScale = Vector3.one;
        buildModeObject.SetActive(true);
    }

    // Changing interaction mode 
    public int GetInteractionMode()
    {
        return interactionMode;
    }

    // Set interaction mode to do something
    public void SetInteractionMode(int mode)
    {
        if (interactionMode == 2 && mode == 0) // view mode in the past and now set to build mode
        {
            OriginalSize();
        }
        // Nak letak Building
        else if (mode == 0 && interactionMode != 0)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                buildings[i].GetComponent<Building>().unfreezeAllMovement();
                ReplaceSlot(roadPrefab, socketBuildingPrefab);
            }
        }
        // Nak letak road
        else if (mode == 1 && interactionMode != 1)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                buildings[i].GetComponent<Building>().freezeAllMovement();
                ReplaceSlot(socketBuildingPrefab, roadPrefab);
            }
        }

        interactionMode = mode;
    }

    // For holding player's position just in case
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
}
