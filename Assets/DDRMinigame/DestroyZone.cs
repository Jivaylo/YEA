using UnityEngine;
public class DestroyZone : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    private Note currentNote;
    private TraumaInducer inducer;

    private void Start()
    {
        inducer = GetComponent<TraumaInducer>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Note note))
        {
            currentNote = note;
        }

        inducer.DoTrauma();
        Destroy(currentNote.gameObject);
    }
}