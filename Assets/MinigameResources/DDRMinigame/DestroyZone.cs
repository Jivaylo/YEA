using UnityEngine;
public class DestroyZone : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private NoteSpawner spawner;
    private Note currentNote;
    private TraumaInducer inducer;

    private void Start()
    {
        inducer = GetComponent<TraumaInducer>();
        if (spawner == null) spawner = FindAnyObjectByType<NoteSpawner>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Note note))
        {
            currentNote = note;
        }
        else
        {
            return; // not a note, ignore
        }

        inducer.DoTrauma();
        if (spawner != null) spawner.AddMiss();
        else Debug.LogWarning("DestroyZone: NoteSpawner reference is null — misses won't count.");
        Destroy(currentNote.gameObject);
    }
}