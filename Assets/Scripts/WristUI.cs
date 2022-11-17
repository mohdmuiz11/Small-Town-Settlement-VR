using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A multi tool UI canvas can be found on the left hand (or right hand)?
/// </summary>
public class WristUI : MonoBehaviour
{
    public InputActionAsset inputActions;

    private Canvas _wristUICanvas;
    private InputAction _menu;
    private void Start()
    {
        _wristUICanvas = GetComponent<Canvas>();
        _menu = inputActions.FindActionMap("XRI LeftHand Interaction").FindAction("Menu");
        _menu.Enable();
        _menu.performed += ToggleMenu;
    }

    private void OnDestroy()
    {
        _menu.performed -= ToggleMenu;
    }

    public void ToggleMenu(InputAction.CallbackContext context)
    {
        _wristUICanvas.enabled = !_wristUICanvas.enabled;
    }
}
