using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPuzzleController : MonoBehaviour
{
    public InputActionAsset actions;
    public Transform puzzleCameraTransform;
    public Animator animator;
    public PuzzleInteraction puzzleInteraction;

    public float walkSpeed = 3.5f;
    public float mouseSensitivity = 0.12f;
    public float gravity = -18f;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction lookAction;

    private Vector3 velocity;
    private float pitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        var map = actions.FindActionMap("Player", true);
        moveAction = map.FindAction("Move", true);
        lookAction = map.FindAction("Look", true);
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }

    void Update()
    {
        Look();
        Move();
        ApplyGravity();
    }

    void Look()
    {
        if (puzzleInteraction != null && puzzleInteraction.IsInspecting)
            return;

        Vector2 look = lookAction.ReadValue<Vector2>();

        transform.Rotate(Vector3.up * look.x * mouseSensitivity);

        pitch -= look.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        if (puzzleCameraTransform != null)
            puzzleCameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * walkSpeed * Time.deltaTime);

        if (animator != null)
            animator.SetFloat("Speed", input.magnitude);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}