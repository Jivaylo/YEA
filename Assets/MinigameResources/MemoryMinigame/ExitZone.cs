using UnityEngine;

// Place on a trigger collider child inside StudyRoom.
// Player walks through it to advance to the question.
public class ExitZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GetComponentInParent<StudyRoom>().OnPlayerExited();
    }
}
