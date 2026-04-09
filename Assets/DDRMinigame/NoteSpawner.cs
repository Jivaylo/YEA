using System;
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

        // random parameters
        note.direction = (Direction)UnityEngine.Random.Range(0, 4);
        note.noteMod = (NoteMod)UnityEngine.Random.Range(0, Enum.GetNames(typeof(NoteMod)).Length);
    }
}