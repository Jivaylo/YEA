using UnityEngine;
using UnityEngine.InputSystem;

public class HitZone : MonoBehaviour
{
    private Note currentNote;
    private TraumaInducer inducer;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Note note))
        {
            currentNote = note;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Note note))
        {
            if (currentNote == note)
                currentNote = null;
        }
    }

    private void Start()
    {
        inducer = GetComponent<TraumaInducer>();
    }
    void Update()
    {
        if (currentNote == null) return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            Hit(Direction.Left);

        if (Keyboard.current.sKey.wasPressedThisFrame)
            Hit(Direction.Down);

        if (Keyboard.current.wKey.wasPressedThisFrame)
            Hit(Direction.Up);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            Hit(Direction.Right);
    }

    void Hit(Direction dir)
    {
        if (currentNote == null)
            return; // no note to hit

        if (currentNote.direction == dir)
        {
            Debug.Log("Correct Hit! Direction: " + dir);

            // remove the note
            Destroy(currentNote.gameObject);
            // inducer.DoTrauma();

            currentNote = null;

            // TODO: add score, play sound, particle effect, etc.
        }
        else
        {
            Debug.Log("Wrong Key! Pressed: " + dir + ", Note: " + currentNote.direction);
            // TODO: handle miss
        }
    }
}