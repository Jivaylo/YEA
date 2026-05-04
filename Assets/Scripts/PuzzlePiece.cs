using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PuzzlePiece : MonoBehaviour
{
    public string pieceId;
    public bool isPlaced = false;

    [Header("Inspect / Hold Settings")]
    public Vector3 holdLocalRotation = Vector3.zero;
    public float holdTargetSize = 0.75f;

    [Header("Fall Safety")]
    public float fallYLimit = -5f;

    private Rigidbody rb;
    private Collider col;
    private Renderer[] renderers;
    private Vector3 originalLocalScale;

    private Vector3 safePosition;
    private Quaternion safeRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(true);
        originalLocalScale = transform.localScale;

        safePosition = transform.position;
        safeRotation = transform.rotation;
    }

    void Update()
    {
        if (!isPlaced && gameObject.activeSelf && transform.position.y < fallYLimit)
        {
            RespawnToSafePosition();
        }
    }

    public void PrepareAtStart()
    {
        transform.SetParent(null, true);
        transform.localScale = originalLocalScale;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        col.enabled = true;
        isPlaced = false;
        gameObject.SetActive(true);
    }

    public void SetSafePosition(Vector3 position, Quaternion rotation)
    {
        safePosition = position;
        safeRotation = rotation;
    }

    public void Scatter(Vector3 force, Vector3 torque)
    {
        transform.SetParent(null, true);
        transform.localScale = originalLocalScale;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        col.enabled = true;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);
    }

    public void PickUp(Transform inspectPivot)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        col.enabled = false;

        transform.SetParent(inspectPivot, false);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(holdLocalRotation);

        float maxSize = GetMaxRendererSize();
        float factor = maxSize > 0.0001f ? holdTargetSize / maxSize : 1f;
        transform.localScale = originalLocalScale * factor;

        Bounds bounds = GetRendererBoundsWorld();
        Vector3 worldOffset = bounds.center - inspectPivot.position;
        transform.position -= worldOffset;
    }

    public void DropAt(Vector3 worldPosition, Quaternion worldRotation)
    {
        transform.SetParent(null, true);
        transform.position = worldPosition;
        transform.rotation = worldRotation;
        transform.localScale = originalLocalScale;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        col.enabled = true;
        isPlaced = false;
        gameObject.SetActive(true);
    }

    public void ConsumePlaced()
    {
        isPlaced = true;
        transform.localScale = originalLocalScale;
        gameObject.SetActive(false);
    }

    void RespawnToSafePosition()
    {
        transform.SetParent(null, true);
        transform.position = safePosition;
        transform.rotation = safeRotation;
        transform.localScale = originalLocalScale;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        col.enabled = true;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Debug.Log(name + " respawned because it fell out of the room.");
    }

    float GetMaxRendererSize()
    {
        Bounds b = GetRendererBoundsWorld();
        return Mathf.Max(b.size.x, b.size.y, b.size.z);
    }

    Bounds GetRendererBoundsWorld()
    {
        if (renderers == null || renderers.Length == 0)
            return new Bounds(transform.position, Vector3.one);

        Bounds b = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        return b;
    }
}