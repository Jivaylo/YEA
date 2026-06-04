using UnityEngine;

public class BrainRotate : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
