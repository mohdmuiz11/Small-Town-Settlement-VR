using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Set the interaction layer of an interactor
/// </summary>
public class ControllerLayerSelect : MonoBehaviour
{
    [Tooltip("The layer that's switched to")]
    public InteractionLayerMask selectGridLayer = 0;
    public InteractionLayerMask teleportableLayer = 0;

    private XRBaseInteractor interactor = null;
    private InteractionLayerMask originalLayer = 0;

    private void Awake()
    {
        interactor = GetComponent<XRBaseInteractor>();
        originalLayer = interactor.interactionLayers;
    }

    public void SetSelectGridLayer()
    {
        interactor.interactionLayers = selectGridLayer;
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
