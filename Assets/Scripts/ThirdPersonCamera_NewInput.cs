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
    public float minDistance = 2f;
    public float maxDistance = 7f;

    private InputAction lookAction;
    private InputAction zoomAction;

    private float yaw;
    private float pitch;
    private float currentDistance;

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

        currentDistance = Mathf.Clamp(-cam.localPosition.z, minDistance, maxDistance);
        cam.localPosition = new Vector3(0, 0, -currentDistance);
    }

    void LateUpdate()
    {
        if (target == null || pivot == null || cam == null)
            return;

      
        Vector3 desiredPos = target.position + followOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);


        Vector2 delta = lookAction.ReadValue<Vector2>();
        yaw += delta.x * sensitivity;
        pitch -= delta.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        pivot.localRotation = Quaternion.Euler(pitch, 0, 0);

  
        float scroll = zoomAction.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minDistance, maxDistance);
            cam.localPosition = new Vector3(0, 0, -currentDistance);
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}