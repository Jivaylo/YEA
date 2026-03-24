using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;

    void Start()
    {
        InvokeRepeating(nameof(SpawnNote), 1f, 1f);
    }

    void SpawnNote()
    {
        GameObject noteObj = Instantiate(notePrefab, transform.position, Quaternion.identity);

        Note note = noteObj.GetComponent<Note>();

        // random direction
        note.direction = (Direction)Random.Range(0, 4);
    }
}