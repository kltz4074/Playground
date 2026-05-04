using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private PlayerInput playerInput;
    
    private InputAction interactAction;
    private Ray ray;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerInput != null)
        {
            interactAction = playerInput.actions["Interact"];
            if (interactAction != null)
                interactAction.Enable();
        }
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
                return;

            bool interactPressed = false;
            if (interactAction != null)
            {
                interactPressed = interactAction.triggered;
            }
            else if (Mouse.current != null)
            {
                interactPressed = Mouse.current.leftButton.wasPressedThisFrame;
            }

            if (interactPressed)
            {
                interactable.OnInteract();
            }
        }
    }
}
