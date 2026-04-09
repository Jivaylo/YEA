using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    public Camera playerCamera;
    public Transform holdPoint;
    public float interactDistance = 5f;
    public float rotateSpeed = 120f;

    [Header("Layers")]
    public LayerMask pickupLayers;
    public LayerMask slotLayers;

    private CarryablePiece heldPiece;

    void Update()
    {
        if (Keyboard.current == null || playerCamera == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldPiece == null)
                TryPickUp();
            else
                TryPlace();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame && heldPiece != null)
        {
            DropHeldPiece();
        }

        if (heldPiece != null)
            RotateHeldPiece();
    }

    void TryPickUp()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, pickupLayers))
        {
            CarryablePiece piece = hit.collider.GetComponent<CarryablePiece>();

            if (piece != null && !piece.isPlaced)
            {
                heldPiece = piece;
                heldPiece.PickUp(holdPoint);
                Debug.Log("Picked up: " + piece.name);
            }
        }
    }

    void TryPlace()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, slotLayers))
        {
            PieceSlot slot = hit.collider.GetComponent<PieceSlot>();

            if (slot != null)
            {
                if (slot.CanAccept(heldPiece))
                {
                    slot.PlacePiece(heldPiece);
                    Debug.Log("Placed " + heldPiece.name + " into " + slot.name);
                    heldPiece = null;
                }
                else
                {
                    Debug.Log("Wrong slot for this piece.");
                }

                return;
            }
        }

        Debug.Log("No valid slot targeted.");
    }

    void DropHeldPiece()
    {
        heldPiece.Release();
        heldPiece = null;
        Debug.Log("Piece dropped.");
    }

    void RotateHeldPiece()
    {
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            float mouseX = Mouse.current.delta.ReadValue().x;
            float mouseY = Mouse.current.delta.ReadValue().y;

            heldPiece.transform.Rotate(playerCamera.transform.up, -mouseX * rotateSpeed * Time.deltaTime, Space.World);
            heldPiece.transform.Rotate(playerCamera.transform.right, mouseY * rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}