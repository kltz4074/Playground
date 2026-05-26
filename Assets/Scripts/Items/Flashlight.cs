using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : ItemBase
{
    [SerializeField] private Light flashlight;
    [SerializeField] private InputActionReference activateReference;

    public bool isEnabled = false;


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

    public void Start()
    {
        flashlight.enabled = isEnabled;
    }

    private void OnActivate(InputAction.CallbackContext context)
    {
        if (Grabbed)
        {
            isEnabled = !isEnabled;
            flashlight.enabled = isEnabled;
        }
    }
}