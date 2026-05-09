using System;
using System.Collections;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

public class DoorOpening : MonoBehaviour
{
    public float openAngle = 90f; // The angle to open the door
    public float openSpeed = 2f; // The speed at which the door opens
    private bool isOpening = false; // Flag to check if the door is opening
    private Quaternion closedRotation; // The initial rotation of the door
    private Quaternion openRotation; // The target rotation when the door is open
    private Coroutine currentCoroutine; // Reference to the current coroutine


    private void Start()
    {
        closedRotation = transform.rotation; // Store the initial rotation
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));  // Calculate the target rotation
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Check for input to open the door
        {
            if (!isOpening)
            {
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine); // Stop any existing coroutine
                }
                currentCoroutine = StartCoroutine(ToggleDoor()); // Start opening the door
            }
        }
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpening ? closedRotation : openRotation; // Determine the target rotation based on the current state
        isOpening = !isOpening; // Toggle the opening state

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f) // Continue until the door is close enough to the target rotation
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime * openSpeed); // Smoothly rotate the door
            yield return null; // Wait for the next frame
        }

        transform.rotation = openRotation; // Ensure the door is fully open at the end
    }
}