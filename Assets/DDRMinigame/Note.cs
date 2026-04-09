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
        SetDirection(direction);
        SetColor(noteMod);
    }

    private Quaternion baseRotation;

    void Awake()
    {
        baseRotation = arrowVisual.localRotation;
    }

    public void SetColor(NoteMod mod)
    {
        var renderer = arrowVisual.GetComponent<Renderer>();
        switch (mod)
        {
            case NoteMod.Normal:
                renderer.material.color = Color.skyBlue;
                break;
            case NoteMod.Reversed:
                renderer.material.color = Color.orange;
                break;
        }
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

        if (noteMod == NoteMod.Reversed)
        {
            offset *= Quaternion.Euler(0, 0, 180);
        }

        arrowVisual.localRotation = baseRotation * offset;

        
    }


    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
    }
}