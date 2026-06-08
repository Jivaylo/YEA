using UnityEngine;
public enum Direction
{
    Left,
    Down,
    Up,
    Right
}

public enum NoteMod
{
    Normal,
    Reversed
}

public enum SpinAxis
{
    X,
    Y,
    Z
}
public class Note : MonoBehaviour
{
    public float speed = 5f;

    public Direction direction;

    public NoteMod noteMod;

    //color
    //default colors
    [SerializeField] private Color defaultNormalColor = Color.mediumSpringGreen;
    [SerializeField] private Color defaultReversedColor = Color.softRed;

    //settable colors
    [SerializeField] private Color normalColor = Color.mediumSpringGreen;
    [SerializeField] private Color reversedColor = Color.softRed;

    //references
    [Tooltip("The object that gets rotated to point Up/Down/Left/Right.")]
    [SerializeField] private Transform arrowVisual;

    [Tooltip("The renderer whose material color is changed (normal/reversed). If left empty, falls back to arrowVisual's renderer.")]
    [SerializeField] private Renderer colorTarget;

    [Tooltip("Which local axis the arrow rotates around to point Up/Down/Left/Right. Change this if you swap to a model whose arrow points along a different axis.")]
    [SerializeField] private SpinAxis spinAxis = SpinAxis.Z;

    [Tooltip("Flip the spin direction around the chosen axis (clockwise vs counter-clockwise). Use if a model's arrow ends up pointing the wrong way.")]
    [SerializeField] private bool invertSpinDirection = false;

    private Vector3 SpinAxisVector
    {
        get
        {
            switch (spinAxis)
            {
                case SpinAxis.X: return Vector3.right;
                case SpinAxis.Y: return Vector3.up;
                default: return Vector3.forward;
            }
        }
    }

    void Start()
    {
        SetDirection(direction);
        SetColor(noteMod);
    }

    private Quaternion baseRotation;

    void Awake()
    {
        baseRotation = arrowVisual.localRotation;
    }

    private Renderer ColorRenderer => colorTarget != null ? colorTarget : arrowVisual.GetComponent<Renderer>();

    public Color CurrentColor => ColorRenderer.material.color;

    public void SetColor(NoteMod mod)
    {
        var renderer = ColorRenderer;
        switch (mod)
        {
            case NoteMod.Normal:
                renderer.material.color = normalColor;
                break;
            case NoteMod.Reversed:
                renderer.material.color = reversedColor;
                break;
        }
    }

    public void SetDirection(Direction dir)
    {
        float angle = 0f;

        switch (dir)
        {
            case Direction.Up:
                angle = 90f;
                break;

            case Direction.Down:
                angle = -90f;
                break;

            case Direction.Left:
                angle = 0f;
                break;

            case Direction.Right:
                angle = 180f;
                break;
        }

        if (invertSpinDirection)
        {
            angle = -angle;
        }

        if (noteMod == NoteMod.Reversed)
        {
            angle += 180f;
        }

        arrowVisual.localRotation = baseRotation * Quaternion.AngleAxis(angle, SpinAxisVector);
    }


    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
    }
}