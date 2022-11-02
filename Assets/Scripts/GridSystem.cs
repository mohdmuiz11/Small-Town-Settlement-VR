using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    // Private variables
    [SerializeField] private int gridCount = 18;
    [SerializeField] private float tableLength = 1.0f;
    [SerializeField] private GameObject slotObject;
    [SerializeField] private float worldSize = 100;

    private Transform thisTranform;
    private GameObject buildModeObject;
    private float widthGrid;
    private Vector3 firstGridPos;
    private float tableHeight;

    /// <summary>
    /// Set interaction mode. Available mode: 0 - build mode, 1 - travel mode, 2 - view mode
    /// </summary>
    private int interactionMode;

    private GameObject[] buildings;


    // Start is called before the first frame update
    void Start()
    {
        buildModeObject = GameObject.Find("BuildMode");
        thisTranform = gameObject.GetComponent<Transform>();
        tableHeight = gameObject.transform.position.y;
        widthGrid = getWidthGrid();
        firstGridPos = getFirstGridPos();
        spawnSlotInGrid();
        buildings = GameObject.FindGameObjectsWithTag("Building");
    }

    private Vector3 getFirstGridPos()
    {
        float pos = 0 - (widthGrid * (gridCount / 2)) + (widthGrid / 2);
        return new Vector3(pos, gameObject.transform.position.y, pos);
    }

    private float getWidthGrid()
    {
        return tableLength / (gridCount + 2);
    }

    private void spawnSlotInGrid()
    {
        Vector3 pos = firstGridPos;
        for (int z=0; z < gridCount; z++)
        {
            for (int x=0; x < gridCount; x++)
            {
                GameObject gridSpawn = Instantiate(slotObject, pos, slotObject.transform.rotation, thisTranform);
                gridSpawn.GetComponent<GridSlot>().SetCoordinate(x, z);
                pos.x += widthGrid;
            }
            pos.x = firstGridPos.x;
            pos.z += widthGrid;
        }
    }

    // Set by buttons
    public void resizeWorld()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = new Vector3(worldSize, worldSize, worldSize);
        buildModeObject.SetActive(false);
    }
    public void originalSize()
    {
        gameObject.transform.position = new Vector3(0, tableHeight, 0);
        gameObject.transform.localScale = Vector3.one;
        buildModeObject.SetActive(true);
    }

    // Changing interaction mode 
    public int getInteractionMode()
    {
        return interactionMode;
    }
    public void setInteractionMode(int mode)
    {
        if (mode == 0)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                buildings[i].GetComponent<Building>().unfreezeAllMovement();
            }
        }
        else if (mode == 1)
        {
            for (int i = 0; i < buildings.Length; i++)
            {
                buildings[i].GetComponent<Building>().freezeAllMovement();
            }
        }
    }
}
