using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string prompt = "Press E";
    public float radius = 2.5f;
    public UnityEvent onInteract = new UnityEvent();

    public void Interact() => onInteract.Invoke();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
