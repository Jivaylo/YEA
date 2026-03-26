using UnityEngine;
public enum Direction
{
    Left,
    Down,
    Up,
    Right
}

public class Note : MonoBehaviour
{
    public float speed = 5f;

    public Direction direction;

    void Start()
    {
        var renderer = GetComponent<Renderer>();

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

    void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;
    }
}