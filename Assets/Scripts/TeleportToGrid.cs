using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportToGrid : XRBaseInteractable
{
    private GridSystem gridSystem;
    private Transform playerTransform;
    private ControllerLayerSelect leftController;
    private ControllerLayerSelect rightController;

    [Tooltip("The layer that's switched to")]
    public InteractionLayerMask targetLayer = 0;

    private XRBaseInteractable interactor = null;
    private InteractionLayerMask originalLayer = 0;

    protected override void Awake()
    {
        base.Awake();
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        playerTransform = GameObject.Find("XR Rig").transform;
        interactor = GetComponent<XRBaseInteractable>();
        originalLayer = interactor.interactionLayers;
        leftController = GameObject.Find("LeftHand Controller").GetComponent<ControllerLayerSelect>();
        rightController = GameObject.Find("RightHand Controller").GetComponent<ControllerLayerSelect>();
    }


    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        gridSystem.resizeWorld();

        playerTransform.position = transform.position;

        leftController.SetTeleportableLayer();
        rightController.SetTeleportableLayer();
        gameObject.SetActive(false);
    }
    public void SetTargetLayer()
    {
        interactor.interactionLayers = targetLayer;

    }

    public void SetOriginalLayer()
    {
        interactor.interactionLayers = originalLayer;
    }


}
