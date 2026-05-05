using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractionSystemCursor cursor;
    [SerializeField] private GameObject ObjectParent;
    [Space]
    [Header("Non-Editable variables")]
    [ReadOnly] [SerializeField] private IItem HoldingItem;

    private InputAction interactAction;
    private InputAction dropItemAction;

    private Ray ray;
    private void Start()
    {
        playerCamera ??= Camera.main;

        if (playerInput == null)
            return;

        interactAction = playerInput.actions["Interact"];
        interactAction?.Enable();

        dropItemAction = playerInput.actions["DropItem"];
        dropItemAction?.Enable();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (HoldingItem != null)
        {
            HoldingItem.OnItemUpdate();
        }

        var mouse = Mouse.current;

        if (WasPressed(dropItemAction, () => mouse?.rightButton.wasPressedThisFrame ?? false))
        {
            if (HoldingItem != null)
            {
                HoldingItem.OnDrop();
                HoldingItem = null;
            }
            return;
        }

        var pointer = mouse?.position.ReadValue() ?? new Vector2(Screen.width / 2f, Screen.height / 2f);
        ray = playerCamera.ScreenPointToRay(pointer);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            cursor?.ChangeCursorColor(Color.black);
            return;
        }

        var interactable = hit.collider.GetComponent<IInteractable>();
        bool interactPressed = WasPressed(interactAction, () => mouse?.leftButton.wasPressedThisFrame ?? false);

        if (interactable != null && HoldingItem == null)
        {
            cursor.ChangeCursorColor(Color.green);
        }
        else
        {
            cursor.ChangeCursorColor(Color.black);
        }

        if (!interactPressed || interactable == null || HoldingItem != null)
            return;

        if (interactable is IItem itemComp)
        {
            HoldingItem = itemComp;
        }

        interactable.OnInteract();
    }

    private bool WasPressed(InputAction action, Func<bool> fallback)
    {
        return action != null ? action.triggered : (fallback?.Invoke() ?? false);
    }
}
