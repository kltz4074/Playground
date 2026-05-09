using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Runtime.CompilerServices;

public class InteractionSystem : MonoBehaviour
{
    #region Variables
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InteractionSystemCursor cursor;
    [SerializeField] private GameObject ObjectParent;
    [Space]
    [Header("Item Holding")]
    [SerializeField] private float ItemHoldDistance = 2f;
    [SerializeField] private float DEFAULTitemHoldHeight = -0.5f;
    [SerializeField] private float DEFAULTitemHoldWidth = 0.5f;

    [SerializeField] private float itemFollowPos = 0.1f;
    [SerializeField] private float itemRotationSpeed = 720f;
    [SerializeField] private float itemScaleSmoothTime = 0.3f;
    [SerializeField] private float throwForce = 2f;
    [SerializeField] bool ItemHoldDistanceScroll = true;
    [SerializeField] float maxHoldDistance = 3f;
    [SerializeField] float minHoldDistance = 0.5f;

    private IItem HoldingItem;
    private InputAction interactAction;
    private InputAction dropItemAction;
    private Collider playerCollider;

    private Ray ray;
    private Vector3 itemPositionVelocity = Vector3.zero;
    private float currentItemScale = 1f;
    private float currentItemScaleVelocity = 0f;
    private float Scroll;

    private Vector3 grabbedItemLastPosition;
    private Vector3 grabbedItemVelocity;

    #endregion

    #region Unity Methods
    private void Start()
    {
        playerCamera ??= Camera.main;
        playerCollider = GetComponent<Collider>();

        if (playerInput == null)
            return;

        if (ObjectParent != null)
        {
            grabbedItemLastPosition = ObjectParent.transform.position;
        }
        interactAction = playerInput.actions["Interact"];
        interactAction?.Enable();

        dropItemAction = playerInput.actions["DropItem"];
        dropItemAction?.Enable();

        playerInput.actions["Scroll"].performed += x => Scroll = x.ReadValue<float>();

        if (HoldingItem != null && HoldingItem.item != null) minHoldDistance = HoldingItem.item.minHoldDistance;
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

        if (Scroll != 0f && ItemHoldDistanceScroll)
        {
            OnScroll();
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
            currentItemScale = itemComp.gameObject.transform.localScale.x;
        }

        interactable.OnInteract();
    }

    public void FixedUpdate()
    {
        UpdateObjectParentPosition();
        
        if (ObjectParent != null)
        {
            grabbedItemVelocity = (ObjectParent.transform.position - grabbedItemLastPosition) / Time.fixedDeltaTime;
            grabbedItemLastPosition = ObjectParent.transform.position;
        }
        
        if (HoldingItem != null && HoldingItem.gameObject != null)
            UpdateHoldObject();

    }
    #endregion

    #region Input Handling
    private bool WasPressed(InputAction action, Func<bool> fallback)
    {
        return action != null ? action.triggered : (fallback?.Invoke() ?? false);
    }

    #endregion

    #region Item Interaction Methods
    public void DropCurrentItem()
    {
        if (HoldingItem != null)
        {
            Collider itemCollider = HoldingItem.gameObject.GetComponent<Collider>();
            if (itemCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(itemCollider, playerCollider, false);
            }

            if (HoldingItem.gameObject.GetComponent<Collider>() is Collider col)
            {
                col.enabled = true;
            }
            if (HoldingItem.gameObject.GetComponent<Rigidbody>() is Rigidbody rb)
            {
                rb.isKinematic = false;
                rb.useGravity = true;

                rb.linearVelocity = (grabbedItemVelocity / 2) + playerCamera.transform.forward * throwForce;
                rb.angularVelocity = playerCamera.transform.right * grabbedItemVelocity.magnitude * 0.05f;
            }
            HoldingItem.OnDrop();
            HoldingItem = null;
        }
    }

    public void UpdateObjectParentPosition()
    {
        if (ObjectParent != null)
        {
            Vector3 holdPosition = GetItemHoldPosition();
            ObjectParent.transform.position = playerCamera.transform.position
                + playerCamera.transform.forward * holdPosition.z
                + playerCamera.transform.up * holdPosition.y
                + playerCamera.transform.right * holdPosition.x;
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

        float smoothTime = Mathf.Max(0.0001f, itemFollowPos);

        item.transform.position = Vector3.SmoothDamp(
            item.transform.position,
            ObjectParent.transform.position,
            ref itemPositionVelocity,
            smoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );

        Quaternion targetRotation = GetItemHoldRotation();
        float maxDegreesThisStep = itemRotationSpeed * Time.fixedDeltaTime;
        item.transform.rotation = Quaternion.RotateTowards(item.transform.rotation, targetRotation, maxDegreesThisStep);

        float targetScale = GetItemHoldScale();
        currentItemScale = Mathf.SmoothDamp(currentItemScale, targetScale, ref currentItemScaleVelocity, itemScaleSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);
        item.transform.localScale = Vector3.one * currentItemScale;
    }

    private Vector3 GetItemHoldPosition()
    {
        if (HoldingItem == null || HoldingItem.item == null)
            return new Vector3(DEFAULTitemHoldWidth, DEFAULTitemHoldHeight, ItemHoldDistance);

        if (HoldingItem.item.isCustomPlaceable)
        {
            return new Vector3(HoldingItem.item.InHandPos.x, HoldingItem.item.InHandPos.y, ItemHoldDistance);
        }

        return new Vector3(DEFAULTitemHoldWidth, DEFAULTitemHoldHeight, ItemHoldDistance);
    }

    private Quaternion GetItemHoldRotation()
    {
        Quaternion baseRotation = ObjectParent.transform.rotation;

        if (HoldingItem == null || HoldingItem.item == null)
            return baseRotation;

        if (HoldingItem.item.isCustomPlaceable)
        {
            return baseRotation * Quaternion.Euler(HoldingItem.item.InHandRot.x, HoldingItem.item.InHandRot.y, 0);
        }

        return baseRotation;
    }

    private float GetItemHoldScale()
    {
        if (HoldingItem == null || HoldingItem.item == null)
            return 1f;

        if (HoldingItem.item.isCustomPlaceable)
        {
            return HoldingItem.item.InHandScale;
        }

        return 1f;
    }

    private void OnScroll()
    {
        if (HoldingItem == null)
            return;
        if (HoldingItem.item != null)
            minHoldDistance = HoldingItem.item.minHoldDistance;
        ItemHoldDistance += Scroll * 0.5f;
        ItemHoldDistance = Mathf.Clamp(ItemHoldDistance, minHoldDistance, maxHoldDistance);
        Debug.Log("Scroll: " + Scroll + ", Hold Distance: " + ItemHoldDistance);
        Debug.Log("Current Item Hold Distance: " + ItemHoldDistance);
    }

    #endregion
}