using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : ItemBase
{
    [SerializeField] private Light flashlight;
    [SerializeField] private InputActionReference activateReference;

    private bool isEnabled = false;

    private void OnEnable()
    {
        activateReference.action.performed += OnActivate;
        activateReference.action.Enable();
    }

    private void OnDisable()
    {
        activateReference.action.performed -= OnActivate;
        activateReference.action.Disable();
    }

    public override void OnItemUpdate()
    {
    }

    private void OnActivate(InputAction.CallbackContext context)
    {
        isEnabled = !isEnabled;
        flashlight.enabled = isEnabled;
    }
}