using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A simple script to detect select state from right controller (or left?)
/// </summary>
public class Grabber : MonoBehaviour
{
    [Header("Attach settings")]
    [SerializeField] private Transform translateAttach;
    [SerializeField] private Transform anchorAttach;
    [SerializeField] private Transform hookAttach;

    [Header("Main models")]
    [SerializeField] private GameObject hookHand;
    [SerializeField] private Transform rootArm;

    [Header("Other settings")]
    [SerializeField] private float maxAnchorDistance = 10; // unit based on anchor's local position (very hard to read)
    [SerializeField] private Animator grabAnimator;


    // classes
    private XRRayInteractor rightHandRay;
    private XRBaseControllerInteractor rightHandCI;
    private ActionBasedController rightHandABC;
    private XRBaseController rightHandController;

    // private vars
    private float originalZPos;
    private float distanceHook;

    // Start is called before the first frame update
    void Start()
    {
        // Reference all classes
        rightHandRay = GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>();
        rightHandCI = rightHandRay.GetComponent<XRBaseControllerInteractor>();
        rightHandController = rightHandRay.GetComponent<XRBaseController>();
        rightHandABC = rightHandController as ActionBasedController;

        // Some vars
        originalZPos = translateAttach.localPosition.z;
        rightHandCI.attachTransform = anchorAttach;
        distanceHook = translateAttach.localPosition.z - hookAttach.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        // Animate grabber
        grabAnimator.SetFloat("Grabbing", rightHandController.selectInteractionState.value);

        // read joystick's y-axis value. this took me an hour to figure this out
        //Debug.Log(rightHandABC.translateAnchorAction.action.ReadValue<Vector2>().y);
        float verticalInput = rightHandABC.translateAnchorAction.action.ReadValue<Vector2>().y;
        translateAttach.transform.Translate(Vector3.forward * verticalInput * Time.deltaTime);

        // Limit distance of objAttach
        if (translateAttach.localPosition.z > maxAnchorDistance)
            translateAttach.localPosition = Vector3.forward * maxAnchorDistance;
        if (translateAttach.localPosition.z < originalZPos)
            translateAttach.localPosition = Vector3.forward * originalZPos;

        // adjust hook's arm and raycast length
        float currentLength = translateAttach.localPosition.z - distanceHook;
        hookAttach.localPosition = Vector3.forward * currentLength;
        rootArm.localScale = new Vector3(rootArm.localScale.x, rootArm.localScale.y, currentLength);
        rightHandRay.maxRaycastDistance = Vector3.Distance(transform.position, translateAttach.transform.position) + 0.03f; // some margin for picking up buildings easier
    }
}
