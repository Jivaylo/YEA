using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleInteraction : MonoBehaviour
{
    public Camera puzzleCamera;
    public Transform holdPoint;

    [Header("Pickup")]
    public float pickupDistance = 6f;
    public float pickupRadius = 0.35f;

    [Header("Place")]
    public float placeSearchRadius = 2.5f;

    [Header("Inspect")]
    public float rotateSpeed = 0.05f;
    public float holdDistance = 1.35f;
    public float zoomSpeed = 0.08f;
    public float minInspectDistance = 0.75f;
    public float maxInspectDistance = 2.2f;

    [Header("Drop")]
    public float dropForwardDistance = 1.0f;
    public float dropUpOffset = 0.3f;

    [Header("Layers")]
    public LayerMask pieceLayers;
    public LayerMask slotLayers;

    private PuzzlePiece heldPiece;
    private Transform inspectPivot;

    private bool inspectMode = false;
    private float inspectYaw = 0f;
    private float inspectPitch = 0f;
    private float currentDistance;

    public bool IsHoldingPiece => heldPiece != null;
    public bool IsInspecting => heldPiece != null && inspectMode;

    void Awake()
    {
        GameObject pivotObj = new GameObject("InspectPivot");
        inspectPivot = pivotObj.transform;
        inspectPivot.SetParent(holdPoint, false);
        inspectPivot.localPosition = Vector3.zero;
        inspectPivot.localRotation = Quaternion.identity;
        inspectPivot.localScale = Vector3.one;
    }

    void Update()
    {
        if (Keyboard.current == null || puzzleCamera == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldPiece == null)
                TryPickUp();
            else if (!inspectMode)
                TryPlaceNearest();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame && heldPiece != null)
            DropHeldPiece();

        if (Keyboard.current.fKey.wasPressedThisFrame && heldPiece != null)
            inspectMode = !inspectMode;

        if (Keyboard.current.rKey.wasPressedThisFrame && heldPiece != null)
            ResetInspect();

        if (inspectMode && heldPiece != null)
            InspectHeldPiece();

        UpdateInspectPivotPosition();
    }

    void TryPickUp()
    {
        Ray ray = puzzleCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.SphereCast(ray, pickupRadius, out RaycastHit hit, pickupDistance, pieceLayers))
        {
            PuzzlePiece piece = hit.collider.GetComponent<PuzzlePiece>();

            if (piece != null && !piece.isPlaced)
            {
                heldPiece = piece;

                inspectMode = false;
                inspectYaw = 0f;
                inspectPitch = 0f;
                currentDistance = holdDistance;

                UpdateInspectPivotPosition();
                inspectPivot.localRotation = Quaternion.identity;

                heldPiece.PickUp(inspectPivot);

                Debug.Log("Picked up: " + piece.name);
            }
        }
    }

    void TryPlaceNearest()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, placeSearchRadius, slotLayers);

        PuzzleSlot bestSlot = null;
        float bestDistance = float.MaxValue;

        foreach (Collider c in nearby)
        {
            PuzzleSlot slot = c.GetComponent<PuzzleSlot>();
            if (slot == null) continue;
            if (!slot.CanAccept(heldPiece)) continue;

            float d = Vector3.Distance(transform.position, slot.transform.position);

            if (d < bestDistance)
            {
                bestDistance = d;
                bestSlot = slot;
            }
        }

        if (bestSlot != null)
        {
            bestSlot.PlacePiece(heldPiece);
            heldPiece = null;
            inspectMode = false;
            inspectPivot.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.Log("No valid slot nearby.");
        }
    }

    void InspectHeldPiece()
    {
        if (Mouse.current == null) return;

        Vector2 mouse = Mouse.current.delta.ReadValue();

        inspectYaw += -mouse.x * rotateSpeed;
        inspectPitch += mouse.y * rotateSpeed;
        inspectPitch = Mathf.Clamp(inspectPitch, -80f, 80f);

        float scroll = Mouse.current.scroll.ReadValue().y;
        currentDistance -= scroll * zoomSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minInspectDistance, maxInspectDistance);

        inspectPivot.localRotation =
            Quaternion.Euler(inspectPitch, inspectYaw, 0f);
    }

    void ResetInspect()
    {
        inspectYaw = 0f;
        inspectPitch = 0f;
        currentDistance = holdDistance;
        inspectPivot.localRotation = Quaternion.identity;
    }

    void UpdateInspectPivotPosition()
    {
        inspectPivot.localPosition = new Vector3(0f, -0.03f, currentDistance);
    }

    void DropHeldPiece()
    {
        Vector3 dropPos = puzzleCamera.transform.position
                        + puzzleCamera.transform.forward * dropForwardDistance
                        + Vector3.up * dropUpOffset;

        Quaternion dropRot = heldPiece.transform.rotation;

        heldPiece.DropAt(dropPos, dropRot);

        heldPiece = null;
        inspectMode = false;
        inspectPivot.localRotation = Quaternion.identity;
    }
}