using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A simple script to detect select state from right controller (or left?)
/// </summary>
public class Grabber : MonoBehaviour
{
    public Animator grabAnimator;
    private XRBaseController rightHand;

    // Start is called before the first frame update
    void Start()
    {
        rightHand = GameObject.Find("RightHand Controller").GetComponent<XRBaseController>();
    }

    // Update is called once per frame
    void Update()
    {
        grabAnimator.SetFloat("Grabbing", rightHand.selectInteractionState.value);
    }
}
