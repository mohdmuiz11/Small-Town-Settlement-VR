using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A simple script to detect select state from right controller (or left?)
/// </summary>
public class Grabber : MonoBehaviour
{
    [SerializeField] private float maxAnchorDistance = 10; // unit based on anchor's local position (very hard to read)
    [SerializeField] private Animator grabAnimator;
    [SerializeField] private Transform objAttach;

    [SerializeField] private Gradient invisibleGradient; //dunno how to setup invisible duh
    [SerializeField] private GameObject hookHand;
    [SerializeField] private Transform hookAttach;
    [SerializeField] private Transform rootArm;

    // classes
    private Gradient originalValidGradient;
    private Gradient originalInvalidGradient;
    private XRRayInteractor rightHandRay;
    private XRBaseControllerInteractor rightHandCI;
    private ActionBasedController rightHandABC;
    private GridSystem gridSystem;
    private XRBaseController rightHandController;
    private XRInteractorLineVisual interactorLineVisual;
    private MeshRenderer meshRenderer;

    // private vars
    private float originalZPos;
    private bool grabberIsActive;
    private float distanceHook;

    // Start is called before the first frame update
    void Start()
    {
        // Reference all classes
        gridSystem = GameObject.Find("GRID System").GetComponent<GridSystem>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        rightHandRay = GameObject.Find("RightHand Controller").GetComponent<XRRayInteractor>();
        rightHandCI = rightHandRay.GetComponent<XRBaseControllerInteractor>();
        rightHandController = rightHandRay.GetComponent<XRBaseController>();
        rightHandABC = rightHandController as ActionBasedController;
        interactorLineVisual = rightHandRay.GetComponent<XRInteractorLineVisual>();

        // Some vars
        originalValidGradient = interactorLineVisual.validColorGradient;
        originalInvalidGradient = interactorLineVisual.invalidColorGradient;
        originalZPos = objAttach.localPosition.z;
        rightHandCI.attachTransform = objAttach;
        hookHand.transform.SetParent(hookAttach, true);
        distanceHook = objAttach.localPosition.z - hookAttach.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        // Animate grabber
        grabAnimator.SetFloat("Grabbing", rightHandController.selectInteractionState.value);

        // Set visibility for certain modes
        if (!grabberIsActive && gridSystem.interactionMode == 0)
            GrabberVisiblity(true);
        else if (grabberIsActive && gridSystem.interactionMode != 0)
            GrabberVisiblity(false);

        // read joystick's y-axis value. this took me an hour to figure this out
        Debug.Log(rightHandABC.translateAnchorAction.action.ReadValue<Vector2>().y);

        // Limit anchor control
        if (rightHandCI.isSelectActive && grabberIsActive)
        {
            // adjust hook's arm length
            float currentLength = rightHandCI.attachTransform.localPosition.z - distanceHook;
            hookAttach.localPosition = Vector3.forward * currentLength;
            rootArm.localScale = new Vector3(rootArm.localScale.x, rootArm.localScale.y, currentLength);

            if (rightHandCI.attachTransform.localPosition.z > maxAnchorDistance)
                rightHandCI.attachTransform.localPosition = Vector3.forward * maxAnchorDistance;
            if (rightHandCI.attachTransform.localPosition.z < originalZPos)
                rightHandCI.attachTransform.localPosition = Vector3.forward * originalZPos;
        }
    }

    // Toggle visiblity of grabber.
    private void GrabberVisiblity(bool isVisible)
    {
        if (isVisible)
        {
            // Set models visible
            meshRenderer.enabled = true;
            hookHand.SetActive(true);

            // Calculate distance between this pivot object to attach point
            rightHandRay.maxRaycastDistance = Vector3.Distance(transform.position, objAttach.transform.position) + 0.03f; // some margin for picking up buildings easier
            interactorLineVisual.lineWidth = 0.05f;
            //interactorLineVisual.validColorGradient = invisibleGradient;
            //interactorLineVisual.invalidColorGradient = invisibleGradient;
            grabberIsActive = true;
        }
        else
        {
            // Set models invisible
            meshRenderer.enabled = false;
            hookHand.SetActive(false);

            rightHandRay.maxRaycastDistance = 30; // default number
            interactorLineVisual.lineWidth = 0.02f;
            //interactorLineVisual.validColorGradient = originalValidGradient;
            //interactorLineVisual.invalidColorGradient = originalInvalidGradient;
            grabberIsActive = false;
        }
    }
}
