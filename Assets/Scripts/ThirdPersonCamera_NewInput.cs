using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera_NewInput : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset actions;

    [Header("References")]
    public Transform target;
    public Transform pivot;
    public Transform cam;

    [Header("Follow")]
    public Vector3 followOffset;
    public float followSmooth = 12f;

    [Header("Look")]
    public float sensitivity = 0.08f;
    public float minPitch = -35f;
    public float maxPitch = 70f;

    [Header("Zoom")]
    public float zoomSpeed = 0.8f;
    public float minDistance = 0.15f;
    public float maxDistance = 7f;

    [Header("Camera Collision")]
    public LayerMask collisionLayers = ~0;
    public float collisionRadius = 0.55f;
    public float collisionOffset = 0.35f;
    public float collisionOriginHeight = 1.4f;

    private InputAction lookAction;
    private InputAction zoomAction;

    private float yaw;
    private float pitch;
    private float targetDistance;

    void Awake()
    {
        var map = actions.FindActionMap("Player", true);
        lookAction = map.FindAction("Look", true);
        zoomAction = map.FindAction("Zoom", true);
    }

    void OnEnable()
    {
        lookAction.Enable();
        zoomAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        lookAction.Disable();
        zoomAction.Disable();
    }

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = pivot.localEulerAngles.x;
        targetDistance = Mathf.Clamp(-cam.localPosition.z, minDistance, maxDistance);
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + followOffset;
    }

    public void SnapTo(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    void LateUpdate()
    {
        if (target == null || pivot == null || cam == null) return;
        if (Time.timeScale == 0f) return;

        transform.position = Vector3.Lerp(
            transform.position,
            target.position + followOffset,
            followSmooth * Time.deltaTime
        );

        Vector2 delta = lookAction.ReadValue<Vector2>();
        yaw += delta.x * sensitivity;
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        pivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        float scroll = zoomAction.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.01f)
            targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed, minDistance, maxDistance);

        float safeDistance = CalculateSafeDistance();

        cam.localPosition = new Vector3(0f, 0f, -safeDistance);
        cam.localRotation = Quaternion.identity;
    }

    float CalculateSafeDistance()
    {
        Vector3 origin = target.position + Vector3.up * collisionOriginHeight;
        Vector3 direction = -pivot.forward;

        LayerMask mask = collisionLayers.value == 0 ? ~0 : collisionLayers;

        float safeDistance = targetDistance;

        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            collisionRadius,
            direction,
            targetDistance,
            mask,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length > 0)
        {
            float closest = targetDistance;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform == target)
                    continue;

                if (hit.distance < closest)
                    closest = hit.distance;
            }

            safeDistance = closest - collisionOffset;
        }

        safeDistance = Mathf.Clamp(safeDistance, minDistance, targetDistance);

        Vector3 finalPos = origin + direction * safeDistance;

        if (Physics.CheckSphere(finalPos, collisionRadius, mask, QueryTriggerInteraction.Ignore))
        {
            safeDistance = minDistance;
        }

        return Mathf.Clamp(safeDistance, minDistance, targetDistance);
    }
}