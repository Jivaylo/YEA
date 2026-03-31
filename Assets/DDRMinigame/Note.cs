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
public class Note : MonoBehaviour
{
    public float speed = 5f;

    public Direction direction;

    public NoteMod noteMod;

    [SerializeField] private Transform arrowVisual;

    void Start()
    {
        var renderer = arrowVisual.GetComponent<Renderer>();

        SetDirection(direction);

        switch (direction)
        {
            case Direction.Left:
                renderer.material.color = Color.red;
                break;
            case Direction.Down:
                renderer.material.color = Color.blue;
                break;
            case Direction.Up:
                renderer.material.color = Color.green;
                break;
            case Direction.Right:
                renderer.material.color = Color.yellow;
                break;
        }
    }

    private Quaternion baseRotation;

    void Awake()
    {
        baseRotation = arrowVisual.localRotation;
    }

    public void SetDirection(Direction dir)
    {
        Quaternion offset = Quaternion.identity;

        switch (dir)
        {
            case Direction.Up:
                offset = Quaternion.Euler(0, 0, 90);
                break;

            case Direction.Down:
                offset = Quaternion.Euler(0, 0, -90);
                break;

            case Direction.Left:
                offset = Quaternion.Euler(0, 0, 0);
                break;

            case Direction.Right:
                offset = Quaternion.Euler(0, 0, 180);
                break;
        }

        arrowVisual.localRotation = baseRotation * offset;
    }


    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
    }
}