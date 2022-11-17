using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Set the interaction layer of an interactor, select via inspector because idk how to set actual layer lol
/// </summary>
public class ControllerLayerSelect : MonoBehaviour
{
    [Tooltip("The layer that's switched to")]
    public InteractionLayerMask selectRoadLayer = 0;
    public InteractionLayerMask teleportableLayer = 0;

    private XRBaseInteractor interactor = null;
    private InteractionLayerMask originalLayer = 0;

    private void Awake()
    {
        interactor = GetComponent<XRBaseInteractor>();
        originalLayer = interactor.interactionLayers;
    }

    public void SetRoadLayer()
    {
        interactor.interactionLayers = selectRoadLayer;
    }
    public void SetTeleportableLayer()
    {
        interactor.interactionLayers = teleportableLayer;
    }

    public void SetOriginalLayer()
    {
        interactor.interactionLayers = originalLayer;
    }
}
