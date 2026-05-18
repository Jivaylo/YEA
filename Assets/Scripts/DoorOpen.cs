using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Transform doorPivot;

    public float openAngle = -90f;
    public float openSpeed = 120f;

    private float currentAngle;
    private float targetAngle;
    private Quaternion closedRotation;

    void Start()
    {
        closedRotation = doorPivot.localRotation;
    }

    void Update()
    {
        float newAngle = Mathf.MoveTowards(
            currentAngle,
            targetAngle,
            openSpeed * Time.deltaTime
        );

        currentAngle = newAngle;
        doorPivot.localRotation = closedRotation * Quaternion.Euler(0f, currentAngle, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            targetAngle = openAngle;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            targetAngle = 0f;
    }
}