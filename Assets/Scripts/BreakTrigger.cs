using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BreakTrigger : MonoBehaviour
{
    public SkeletonPuzzleManager puzzleManager;
    public Transform player;
    public float interactDistance = 4f;
    public TextMeshProUGUI interactText;

    private bool hasBroken = false;

    void Update()
    {
        if (player == null || puzzleManager == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactDistance && !hasBroken;

        if (interactText != null)
            interactText.gameObject.SetActive(inRange);

        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            puzzleManager.BreakSkeleton();
            hasBroken = true;

            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }
}