using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SlotInteractor : XRSocketInteractor
{
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return false;
    }
}
