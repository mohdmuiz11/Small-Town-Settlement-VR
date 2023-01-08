using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage slots in a grid system. Executed before anything else.
/// </summary>
[DefaultExecutionOrder(-20)]
public class SlotManager : MonoBehaviour
{
    // Editable config
    [Header("General")]
    [SerializeField] private int gridCount = 18;
    [SerializeField] private BoxCollider table;
    [SerializeField] private GameObject socketBuildingPrefab;
    [SerializeField] private GameObject roadPrefab;

    [Header("Forest settings")]
    [SerializeField] private GameObject forestPrefab;
    [SerializeField] private int startingRowToPopulate;
    [SerializeField] private Vector2[] forestRangePopulate;

    [Header("Special events")]
    [SerializeField] private LocationSideEvent[] specialEvents;

    [Header("Available Buildings")]
    public Building[] buildingList;
    public bool currentBuildingHasBuilt { get; private set; }

    // Initiate lists of slots and interactables
    private List<GameObject> slots = new(); // literally everything
    public GameObject[] events { get; private set; }
    private GameObject[] socketBuildings;
    private GameObject[] roads;
    //private GameObject[] sideEvents;
    //private GameObject[] obstacles;

    // Other vars
    private string currentSlot; // by tag

    /// <summary>
    /// Get width of a grid to have same scale as props.
    /// </summary>
    public float WidthGrid { get; private set; }
    /// <summary>
    /// Exclusive for road. Is the controller is in a state of activiting? 0 = nope, 1 = remove road, 2 = add road
    /// </summary>
    [HideInInspector] public int controllerActivate = 0;

    private void Awake()
    {

        // Initiate arrays
        socketBuildings = new GameObject[gridCount * gridCount];
        roads = new GameObject[gridCount * gridCount];
        events = new GameObject[gridCount * gridCount];

        // Default state
        WidthGrid = (table.size.z) / (gridCount + 1);
        InitiateSlotList();
        currentSlot = socketBuildingPrefab.tag;
        //Instantiate(specialEvents[0].sideEvent);
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
                bool isEvent = false;
                bool isObstacle = false; //use for things that absolute cannot be interacted

                // hard coded to place forest, this is the most efficient
                if (i >= startingRowToPopulate)
                {
                    isEvent = true;
                    SpawnSlot(forestPrefab, pos, i, x, z, isEvent);
                }

                // selective forest event if there is
                for (int e = 0; e < forestRangePopulate.Length; e++)
                {
                    if (i >= forestRangePopulate[e].x && i <= forestRangePopulate[e].y)
                    {
                        isEvent = true;
                        SpawnSlot(forestPrefab, pos, i, x, z, isEvent);
                    }
                }

                //selective special events idk
                for (int e = 0; e < specialEvents.Length; e++)
                {
                    if (specialEvents[e].location.x == x && specialEvents[e].location.y == z)
                    {
                        if (specialEvents[e].sideEvent == null)
                            isObstacle = true;
                        else
                        {
                            isEvent = true;
                            SpawnSlot(specialEvents[e].sideEvent.gameObject, pos, i, x, z, isEvent);
                        }
                        break;
                    }
                }
                
                // As long as it is not obstacle
                if (!isObstacle)
                {
                    // Spawn socket by default
                    SpawnSlot(socketBuildingPrefab, pos, i, x, z, !isEvent);

                    // Spawn road but inactive
                    SpawnSlot(roadPrefab, pos, i, x, z, false);
                }

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

        slotSpawn = Instantiate(objPrefab, pos, objPrefab.transform.rotation, transform);
        slotSpawn.transform.localScale = new Vector3(WidthGrid, 0.1f, WidthGrid);
        slotSpawn.GetComponent<IGridCoordinate>().SetCoordinate(x, z);
        slotSpawn.SetActive(isVisible);
        slots.Add(slotSpawn);

        if (objPrefab.tag == "Socket")
            socketBuildings[index] = slotSpawn;
        else if (objPrefab.tag == "Road")
            roads[index] = slotSpawn;
        else if (objPrefab.tag == "SideEvent")
            events[index] = slotSpawn;
    }

    /// <summary>
    /// Switch slot types. Available type = Road, Socket & Teleport.
    /// </summary>
    /// <param name="type">Type of slot in string. Case sensitive.</param>
    public void SwitchSlot(string type, bool refresh = false)
    {
        // Switch from socket -> road
        if (type == "Road" && currentSlot != roadPrefab.tag )
        {
            for (int i = 0; i < socketBuildings.Length; i++)
            {
                if (socketBuildings[i] != null)
                {
                    // check for side event
                    bool isEvent = false;
                    if (events[i] != null)
                        isEvent = true;

                    bool occupied = socketBuildings[i].GetComponent<IGridCoordinate>().HasPlaced;
                    if (!occupied)
                    {
                        socketBuildings[i].SetActive(false);
                        roads[i].SetActive(!isEvent);
                    }
                }
            }
            currentSlot = roadPrefab.tag;
        }

        // Switch from road -> socket
        else if (type == "Socket" && (currentSlot != socketBuildingPrefab.tag || refresh))
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] != null)
                {
                    // check for side event
                    bool isEvent = false;
                    if (events[i] != null)
                        isEvent = true;

                    bool occupied = roads[i].GetComponent<IGridCoordinate>().HasPlaced;
                    if (!occupied)
                    {
                        roads[i].SetActive(false);
                        socketBuildings[i].SetActive(!isEvent);
                    }
                }
            }
            currentSlot = socketBuildingPrefab.tag;
        }

        // Switch from socket/road -> teleport
        else if (type == "Teleport" && currentSlot != "Teleport")
        {
            foreach (var slot in slots)
            {
                bool occupied = slot.GetComponent<IGridCoordinate>().HasPlaced;

                if (!occupied)
                    slot.SetActive(false);
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
            if (socketBuildings[i] != null)
                socketBuildings[i].GetComponent<SocketBuilding>().showInteractableHoverMeshes = isHoverable;
        }
    }

    /// <summary>
    /// Return results of checking surrounding the grid
    /// </summary>
    /// <param name="x">GRID Coordinate X</param>
    /// <param name="z">GRID Coordinate Y</param>
    /// <returns>Condition of N, E, S, W.</returns>
    public bool[] CheckSurrounding(int x, int z)
    {
        int size = 4;
        bool[] surroundings = new bool[size]; // should default all to false
        bool[] isChecked = new bool[4];

        // check each slot first available in grid system
        foreach (var slot in slots)
        {
            // if the slot is not active, skip it
            if (!slot.activeSelf)
                continue;

            IGridCoordinate slotCoordinate = slot.GetComponent<IGridCoordinate>();

            for (int s = 0; s < size; s++)
            {
                // ba = bwh atas; kk = kiri-kanan; ve = positive/negative
                int ba = (s % 2 == 0) ? 1 : 0, kk = s % 2, ve = 1;

                if (s > 1) ve = -1;
                // loop 0 : ba=1, kk=0, north
                // loop 1 : ba=0, kk=1, east
                // loop 2 : ba=-1, kk=0, south
                // loop 3 : ba=0, kk=-1, west

                if (slotCoordinate.PosX == (x + kk * ve) && slotCoordinate.PosZ == (z + ba * ve) && !isChecked[s])
                {
                    if (slotCoordinate.HasPlaced)
                    {
                        if (slot.CompareTag("Socket")) // check building direction if it is valid
                            surroundings[s] = CheckDirection(ba * ve, kk * ve, slot.GetComponent<SocketBuilding>());
                        else if (slot.CompareTag("Road"))
                            surroundings[s] = true;
                    }
                    isChecked[s] = true;
                }
            }
        }
        return surroundings;
    }

    // plan to tidy up this code but too lazy
    private bool CheckDirection(int ba, int kk, SocketBuilding socket)
    {
        if (ba == 1 && kk == 0 && socket.attachTransform.localEulerAngles.y == 180) return true;
        if (ba == 0 && kk == 1 && socket.attachTransform.localEulerAngles.y == 270) return true;
        if (ba == -1 && kk == 0 && socket.attachTransform.localEulerAngles.y == 0) return true;
        if (ba == 0 && kk == -1 && socket.attachTransform.localEulerAngles.y == 90) return true;
        return false;
    }

    /// <summary>
    /// Trigger to update all roads.
    /// </summary>
    public void UpdateAllRoads()
    {
        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i] != null)
            {
                Road road = roads[i].GetComponent<Road>();

                if (road.HasPlaced)
                    road.RoadUpdate();
            }
        }
    }

    /// <summary>
    /// Update something next day
    /// </summary>
    public void UpdateNextDay()
    {
        var buildings = GameObject.FindGameObjectsWithTag("Building");

        for (int i = 0; i < buildings.Length; i++)
        {
            Building building = buildings[i].GetComponent<Building>();
            building.UpdateNextDay();
        }
    }

    /// <summary>
    /// Check if the building is available and active
    /// </summary>
    /// <param name="buildingType">Building type</param>
    /// <returns>True if there is a building, false if not</returns>
    public bool CheckAvailableBuilding(BuildingType buildingType)
    {
        var buildings = GameObject.FindGameObjectsWithTag("Building");

        for (int i = 0; i < buildings.Length; i++)
        {
            Building instanceB = buildings[i].GetComponent<Building>();
            if (instanceB.buildingType == buildingType && instanceB.hasBuild)
                return true;
        }

        return false;
    }
}
