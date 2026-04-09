using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CarryablePiece : MonoBehaviour
{
    public string pieceId;
    public bool isPlaced = true;

    private Rigidbody rb;
    private Collider col;
    private int defaultLayer;
    private int ignoreRaycastLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        defaultLayer = gameObject.layer;
        ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
    }

    public void Drop(Vector3 force)
    {
        transform.SetParent(null);
        isPlaced = false;

        gameObject.layer = defaultLayer;

        col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(force, ForceMode.Impulse);
    }

    public void PickUp(Transform holdPoint)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        col.enabled = false;

        if (ignoreRaycastLayer != -1)
            gameObject.layer = ignoreRaycastLayer;

        Vector3 worldScale = transform.lossyScale;

        transform.SetParent(holdPoint, true);

        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;

        Vector3 parentScale = holdPoint.lossyScale;
        transform.localScale = new Vector3(
            worldScale.x / parentScale.x,
            worldScale.y / parentScale.y,
            worldScale.z / parentScale.z
        );
    }

    public void Release()
    {
        transform.SetParent(null);

        gameObject.layer = defaultLayer;

        col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    public void SnapToSlot(Transform slotTransform)
    {
        Vector3 worldScale = transform.lossyScale;

        transform.SetParent(slotTransform, true);
        transform.position = slotTransform.position;
        transform.rotation = slotTransform.rotation;

        Vector3 parentScale = slotTransform.lossyScale;
        transform.localScale = new Vector3(
            worldScale.x / parentScale.x,
            worldScale.y / parentScale.y,
            worldScale.z / parentScale.z
        );

        gameObject.layer = defaultLayer;

        col.enabled = false;
        rb.isKinematic = true;
        rb.useGravity = false;

        isPlaced = true;
    }
}