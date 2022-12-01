using UnityEngine;

/// <summary>
/// Manage slots in a grid system
/// </summary>
public class SlotManager : MonoBehaviour
{
    // Editable config
    [SerializeField] private int gridCount = 18;
    [SerializeField] private BoxCollider table;
    [SerializeField] private GameObject socketBuildingPrefab;
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private bool useElevation; // use elevation according to model map

    // Elevation profile - each grid has its own elevation from origin point to sea level (y-axis)
    [SerializeField] private float[] elevations;

    // Initiate lists of slots and interactables
    private GameObject[] socketBuildings;
    private GameObject[] roads;
    //private GameObject[] buildings;
    //private GameObject[] sideEvents;
    //private GameObject[] obstacles;

    // Other vars
    private string currentSlot; // by tag
    public float WidthGrid { get; private set; }

    private void Awake()
    {

        // Initiate arrays
        socketBuildings = new GameObject[gridCount * gridCount];
        roads = new GameObject[gridCount * gridCount];

        // Default state
        WidthGrid = (table.size.z) / (gridCount + 1);
        InitiateSlotList();
        currentSlot = socketBuildingPrefab.tag;
    }

    // get first world position of the grid
    private Vector3 GetFirstGridPos()
    {
        float pos = 0 - (WidthGrid * (gridCount / 2)) + (WidthGrid / 2);
        return new Vector3(pos, transform.position.y, pos);
    }

    // Spawn first into the grid
    private void InitiateSlotList()
    {
        Vector3 firstPos = GetFirstGridPos();
        // Get width of the grid according to table's Z-axis size (front-back)
        Vector3 pos = firstPos;

        int i = 0; // init index for storing arrays

        // Spawn with 2D array method
        for (int z = 0; z < gridCount; z++)
        {
            for (int x = 0; x < gridCount; x++)
            {
                // Spawn socket by default
                SpawnSlot(socketBuildingPrefab, pos, i, x, z, true);

                // Spawn road but inactive
                SpawnSlot(roadPrefab, pos, i, x, z, false);
                i++;
                pos.x += WidthGrid;
            }
            pos.x = firstPos.x;
            pos.z += WidthGrid;
        }
    }

    // Spawn the slot and then set coordinate
    private void SpawnSlot(GameObject objPrefab, Vector3 pos, int index, int x, int z, bool isVisible)
    {
        GameObject slotSpawn;

        // Spawn slot with fixed elevations
        if (index < elevations.Length && useElevation)
        {
            Vector3 elevatePos = new Vector3(pos.x, pos.y+elevations[index], pos.z);
            slotSpawn = Instantiate(objPrefab, elevatePos, objPrefab.transform.rotation, transform);
        }
        else
        {
            slotSpawn = Instantiate(objPrefab, pos, objPrefab.transform.rotation, transform);
        }

        slotSpawn.transform.localScale = new Vector3(WidthGrid, 0.1f, WidthGrid);
        slotSpawn.GetComponent<IGridCoordinate>().SetCoordinate(x, z);
        slotSpawn.SetActive(isVisible);
        if (objPrefab.tag == "Socket")
        {
            socketBuildings[index] = slotSpawn;
        }
        else if (objPrefab.tag == "Road")
        {
            roads[index] = slotSpawn;
        }
    }

    /// <summary>
    /// Switch slot types. Available type = Road, Socket & Teleport.
    /// </summary>
    /// <param name="type">Type of slot in string. Case sensitive.</param>
    public void SwitchSlot(string type)
    {
        // Switch from socket -> road
        if (type == "Road" && currentSlot != roadPrefab.tag )
        {
            for (int i = 0; i < socketBuildings.Length; i++)
            {
                bool occupied = socketBuildings[i].GetComponent<IGridCoordinate>().HasPlaced;
                if (!occupied)
                {
                    socketBuildings[i].SetActive(false);
                    roads[i].SetActive(true);
                }
            }
            currentSlot = roadPrefab.tag;
        }

        // Switch from road -> socket
        else if (type == "Socket" && currentSlot != socketBuildingPrefab.tag)
        {
            for (int i = 0; i < roads.Length; i++)
            {
                bool occupied = roads[i].GetComponent<IGridCoordinate>().HasPlaced;
                if (!occupied)
                {
                    roads[i].SetActive(false);
                    socketBuildings[i].SetActive(true);
                }
            }
            currentSlot = socketBuildingPrefab.tag;
        }

        // Switch from socket/road -> teleport
        else if (type == "Teleport" && currentSlot != "Teleport")
        {
            for (int i = 0; i < gridCount * gridCount; i++) // universal number known to man
            {
                bool occupiedRoad = roads[i].GetComponent<IGridCoordinate>().HasPlaced;
                bool occupiedSocket = socketBuildings[i].GetComponent<IGridCoordinate>().HasPlaced;

                if (!occupiedRoad)
                    roads[i].SetActive(false);
                if (!occupiedSocket)
                    socketBuildings[i].SetActive(false);
            }

            currentSlot = "Teleport";
        }
    }

    /// <summary>
    /// Disable mesh hover for sockets to avoid visual bugs
    /// </summary>
    /// <param name="isHoverable">True to enable. False to disable.</param>
    public void ToggleHoverMeshSocket(bool isHoverable)
    {
        for (int i = 0; i < socketBuildings.Length; i++)
        {
            socketBuildings[i].GetComponent<SocketBuilding>().showInteractableHoverMeshes = isHoverable;
        }
    }
}
