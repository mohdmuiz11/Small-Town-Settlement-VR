using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Building : XRGrabInteractable
{
    [SerializeField] private TeleportToGrid bruh; 
    private Rigidbody rb;

    protected override void Awake()
    {
        base.Awake();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public void SetTargetLayer()
    {
        bruh.SetTargetLayer();
    }

    public void SetOriginalLayer()
    {
        bruh.SetOriginalLayer();
    }

    public void freezeAllMovement()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void unfreezeAllMovement()
    {
        rb.constraints = RigidbodyConstraints.None;
    }
}
