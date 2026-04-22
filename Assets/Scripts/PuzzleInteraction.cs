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

    [Header("Rotate")]
    public float rotateSpeed = 180f;

    [Header("Drop")]
    public float dropForwardDistance = 1.0f;
    public float dropUpOffset = 0.3f;

    [Header("Layers")]
    public LayerMask pieceLayers;
    public LayerMask slotLayers;

    private PuzzlePiece heldPiece;
    private bool rotateMode = false;

    private Quaternion baseHoldRotation = Quaternion.identity;
    private Quaternion inspectRotation = Quaternion.identity;

    public bool IsHoldingPiece => heldPiece != null;
    public bool IsInspecting => heldPiece != null && rotateMode;

    void Update()
    {
        if (Keyboard.current == null || puzzleCamera == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldPiece == null)
                TryPickUp();
            else if (!rotateMode)
                TryPlaceNearest();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame && heldPiece != null)
        {
            DropHeldPiece();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame && heldPiece != null)
        {
            rotateMode = !rotateMode;
            Debug.Log("Rotate mode: " + rotateMode);
        }

        if (rotateMode && heldPiece != null && Mouse.current != null)
            RotateHeldPiece();
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
                heldPiece.PickUp(holdPoint);

                rotateMode = false;

                baseHoldRotation = Quaternion.Euler(heldPiece.holdLocalRotation);
                inspectRotation = Quaternion.identity;
                heldPiece.transform.localRotation = baseHoldRotation;

                Debug.Log("Picked up: " + piece.name);
            }
        }
        else
        {
            Debug.Log("Pickup ray found nothing.");
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
            Debug.Log("Placed: " + heldPiece.name + " into " + bestSlot.name);

            heldPiece = null;
            rotateMode = false;
            inspectRotation = Quaternion.identity;
        }
        else
        {
            Debug.Log("No valid slot nearby.");
        }
    }

    void DropHeldPiece()
    {
        Vector3 dropPos = puzzleCamera.transform.position
                        + puzzleCamera.transform.forward * dropForwardDistance
                        + Vector3.up * dropUpOffset;

        Quaternion dropRot = heldPiece.transform.rotation;

        heldPiece.DropAt(dropPos, dropRot);

        heldPiece = null;
        rotateMode = false;
        inspectRotation = Quaternion.identity;

        Debug.Log("Dropped piece");
    }

    void RotateHeldPiece()
    {
        Vector2 look = Mouse.current.delta.ReadValue();

        float yaw = -look.x * rotateSpeed * Time.deltaTime;
        float pitch = look.y * rotateSpeed * Time.deltaTime;

      
        heldPiece.transform.Rotate(Vector3.up, yaw, Space.Self);
        heldPiece.transform.Rotate(Vector3.right, pitch, Space.Self);
    }
}