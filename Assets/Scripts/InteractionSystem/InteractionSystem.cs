using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform objectParent;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;

    [Header("Holding")]
    [SerializeField] private Vector3 defaultHoldOffset = new Vector3(0.5f, -0.5f, 2f);

    [Header("Throw")]
    [SerializeField] private float throwForce = 2f;

    [Header("Input")]
    [SerializeField] private InputActionReference interactActionReference;
    [SerializeField] private InputActionReference dropActionReference;
    [SerializeField] private InputActionReference triggerItemReference;

    private InputAction interactAction;
    private InputAction dropAction;
    private InputAction triggerItemAction;

    private ItemBase holdingItem;
    private Collider playerCollider;

    private void Awake()
    {
        playerCamera ??= Camera.main;
        playerCollider = GetComponent<Collider>();

        interactAction = interactActionReference.action;
        dropAction = dropActionReference.action;
        triggerItemAction = triggerItemReference.action;
    }

    private void OnEnable()
    {
        interactAction.Enable();
        dropAction.Enable();
        triggerItemAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
        dropAction.Disable();
        triggerItemAction.Disable();
    }

    private void Update()
    {
        if (dropAction.triggered)
        {
            DropCurrentItem();
        }

        HandleInteraction();
    }

    private void LateUpdate()
    {
        if (holdingItem != null)
        {
            holdingItem.transform.localPosition = GetHoldPosition();
            holdingItem.transform.localRotation = GetHoldRotation();
        }
    }

    private void HandleInteraction()
    {
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange))
            return;

        Interactable interactable = hit.collider.GetComponent<Interactable>();

        if (interactable == null || holdingItem != null)
            return;

        if (!interactAction.triggered)
            return;

        if (interactable is ItemBase item)
        {
            PickupItem(item);
        }
    }

    private void PickupItem(ItemBase item)
    {
        holdingItem = item;
        item.OnInteract();

        Collider itemCollider = item.GetComponent<Collider>();
        if (itemCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(itemCollider, playerCollider, true);
        }

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        item.transform.SetParent(objectParent);
        item.transform.localPosition = GetHoldPosition();
        item.transform.localRotation = GetHoldRotation();
    }

    private Vector3 GetHoldPosition()
    {
        if (holdingItem != null && holdingItem.ScItem != null && holdingItem.ScItem.isCustomPlaceable)
        {
            return new Vector3(holdingItem.ScItem.InHandPos.x, holdingItem.ScItem.InHandPos.y, defaultHoldOffset.z);
        }

        return defaultHoldOffset;
    }

    private Quaternion GetHoldRotation()
    {
        if (holdingItem != null && holdingItem.ScItem != null && holdingItem.ScItem.isCustomPlaceable)
        {
            return Quaternion.Euler(holdingItem.ScItem.InHandRot);
        }

        return Quaternion.identity;
    }

    public void DropCurrentItem()
    {
        if (holdingItem == null)
            return;

        holdingItem.OnDrop();

        Collider itemCollider = holdingItem.GetComponent<Collider>();
        if (itemCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(itemCollider, playerCollider, false);
        }

        holdingItem.transform.SetParent(null);

        Rigidbody rb = holdingItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = playerCamera.transform.forward * throwForce;
        }

        holdingItem = null;
    }
}