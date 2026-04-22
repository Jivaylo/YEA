using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PuzzlePiece : MonoBehaviour
{
    public string pieceId;
    public bool isPlaced = false;

    [Header("Hold Settings")]
    public Vector3 holdLocalRotation = Vector3.zero;
    public float holdDistance = 1.2f;
    public float holdTargetSize = 0.6f;

    private Rigidbody rb;
    private Collider col;
    private Renderer[] renderers;
    private Vector3 originalLocalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>(true);
        originalLocalScale = transform.localScale;
    }

    public void PrepareAtStart()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        col.enabled = true;
        isPlaced = false;
        gameObject.SetActive(true);
        transform.localScale = originalLocalScale;
    }

    public void Scatter(Vector3 force, Vector3 torque)
    {
        transform.SetParent(null, true);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        col.enabled = true;

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);
    }

    public void PickUp(Transform holdPoint)
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        col.enabled = false;

        transform.SetParent(holdPoint, false);

       
        transform.localPosition = new Vector3(0f, 0f, holdDistance);
        transform.localRotation = Quaternion.Euler(holdLocalRotation);

      
        float maxSize = GetMaxRendererSize();
        float factor = maxSize > 0.0001f ? holdTargetSize / maxSize : 1f;
        transform.localScale = originalLocalScale * factor;
    }

    public void DropAt(Vector3 worldPosition, Quaternion worldRotation)
    {
        transform.SetParent(null, true);
        transform.position = worldPosition;
        transform.rotation = worldRotation;
        transform.localScale = originalLocalScale;

        rb.isKinematic = false;
        rb.useGravity = true;
        col.enabled = true;
        isPlaced = false;
        gameObject.SetActive(true);
    }

    public void ConsumePlaced()
    {
        isPlaced = true;
        gameObject.SetActive(false);
        transform.localScale = originalLocalScale;
    }

    float GetMaxRendererSize()
    {
        if (renderers == null || renderers.Length == 0)
            return 1f;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        return Mathf.Max(b.size.x, b.size.y, b.size.z);
    }
}