using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        buildModeObject = GameObject.Find("Build mode");
        thisTranform = gameObject.GetComponent<Transform>();
        tableHeight = gameObject.transform.position.y;
        widthGrid = getWidthGrid();
        firstGridPos = getFirstGridPos();
        spawnSlotInGrid();
        StartCoroutine(Delaybruh());
        StartCoroutine(Delayunbruh());
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

    private IEnumerator Delaybruh()
    {
        yield return new WaitForSeconds(3);
        resizeWorld();
    }

    private IEnumerator Delayunbruh()
    {
        yield return new WaitForSeconds(8);
        originalSize();
    }
}
