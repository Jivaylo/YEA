using UnityEngine;
public class DestroyZone : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    private Note currentNote;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Note note))
        {
            currentNote = note;
        }

        Destroy(currentNote.gameObject);
    }
}