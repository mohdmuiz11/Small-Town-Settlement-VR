using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GridSlot : XRSocketInteractor
{
    // Update is called once per frame
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Transform building = args.interactableObject.transform;

        //Debug.Log(building);

        building.SetParent(GameObject.Find("GRID System").transform);
    }
}
