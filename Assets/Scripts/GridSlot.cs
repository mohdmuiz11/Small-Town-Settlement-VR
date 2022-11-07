using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GridSlot : XRSocketInteractor
{
    //Store coordinate of the grid
    [SerializeField] private int posX;
    [SerializeField] private int posZ;

    // Define GRID System
    private GameObject gridSystem;
    private GridSystem componentGridSystem;

    protected override void Awake()
    {
        base.Awake();

        gridSystem = GameObject.Find("GRID System");
        componentGridSystem = gridSystem.GetComponent<GridSystem>();
    }

    // Set coordinate when spawn from GRID system
    public void SetCoordinate(int x, int z)
    {
        this.posX = x;
        this.posZ = z;
    }

    // Get position if needed
    public int getPosX()
    {
        return this.posX;
    }
    public int getPosZ()
    {
        return this.posZ;
    }


    // Update is called once per frame
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Building building = args.interactable.GetComponent<Building>();
        Transform buildingTransform = args.interactableObject.transform;

        //building.SetTargetLayer();
        if (building != null)
        {
            building.SetToGrid(true);
            buildingTransform.SetParent(gridSystem.transform);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Building building = args.interactable.GetComponent<Building>();

        if (building != null)
            building.SetToGrid(false);
        //building.SetOriginalLayer();
    }
}
