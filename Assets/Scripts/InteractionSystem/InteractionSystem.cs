using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Runtime.CompilerServices;

public class InteractionSystem : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractionSystemCursor cursor;
    [Space]
    [Header("Item Holding")]
    [SerializeField] private float itemHoldDistance = 2f;
    [SerializeField] private float itemHoldHeight = -0.5f;
    [SerializeField] private float itemHoldWidth = 0.5f;

    [SerializeField] private float itemFollowSpeed = 0.1f;
    [SerializeField] private float itemRotationSpeed = 720f;

    [SerializeField] private GameObject ObjectParent;

    private IItem HoldingItem;
    private InputAction interactAction;
    private InputAction dropItemAction;
    private Collider playerCollider;

    private Ray ray;
    private Vector3 itemPositionVelocity = Vector3.zero;

    private void Start()
    {
        playerCamera ??= Camera.main;
        playerCollider = GetComponent<Collider>();

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
            DropCurrentItem();
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
            Collider itemCollider = itemComp.gameObject.GetComponent<Collider>();
            if (itemCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(itemCollider, playerCollider, true);
            }
        }

        interactable.OnInteract();
    }

    public void FixedUpdate()
    {
        UpdateObjectParentPosition();
        if (HoldingItem != null && HoldingItem.gameObject != null)
            UpdateHoldObject();
    }

    private bool WasPressed(InputAction action, Func<bool> fallback)
    {
        return action != null ? action.triggered : (fallback?.Invoke() ?? false);
    }

    public void DropCurrentItem()
    {
        if (HoldingItem != null)
        {
            Collider itemCollider = HoldingItem.gameObject.GetComponent<Collider>();
            if (itemCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(itemCollider, playerCollider, false);
            }

            if (HoldingItem.gameObject.GetComponent<Rigidbody>() is Rigidbody rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = playerCamera.transform.forward * 2f;
            }
            HoldingItem.OnDrop();

            HoldingItem = null;
        }
    }


    public void UpdateObjectParentPosition()
    {
        if (ObjectParent != null)
        {
            ObjectParent.transform.position = playerCamera.transform.position + playerCamera.transform.forward * itemHoldDistance + playerCamera.transform.up * itemHoldHeight + playerCamera.transform.right * itemHoldWidth;
            ObjectParent.transform.rotation = playerCamera.transform.rotation;
        }
    }

    public void UpdateHoldObject()
    {
        if (HoldingItem == null)
            return;

        GameObject item = HoldingItem.gameObject;
        if (item == null || ObjectParent == null)
            return;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        float smoothTime = Mathf.Max(0.0001f, itemFollowSpeed);

        item.transform.position = Vector3.SmoothDamp(
            item.transform.position,
            ObjectParent.transform.position,
            ref itemPositionVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );

        Quaternion targetRotation = ObjectParent.transform.rotation;
        float maxDegreesThisStep = itemRotationSpeed * Time.fixedDeltaTime;
        item.transform.rotation = Quaternion.RotateTowards(item.transform.rotation, targetRotation, maxDegreesThisStep);
    }
}