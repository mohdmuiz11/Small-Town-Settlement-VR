using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    // Private variables
    [SerializeField] private int gridCount = 18;
    [SerializeField] private float tableLength = 1.0f;
    [SerializeField] private GameObject slotObject;

    private Transform thisTranform;
    private float widthGrid;
    private Vector3 firstGridPos;

    // Start is called before the first frame update
    void Start()
    {
        thisTranform = gameObject.GetComponent<Transform>();
        widthGrid = getWidthGrid();
        firstGridPos = getFirstGridPos();
        spawnSlotInGrid();
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
                Instantiate(slotObject, pos, slotObject.transform.rotation, thisTranform);
                pos.x += widthGrid;
            }
            pos.x = firstGridPos.x;
            pos.z += widthGrid;
        }
    }
}
