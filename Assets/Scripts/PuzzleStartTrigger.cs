using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PuzzleStartTrigger : MonoBehaviour
{
    public PlayerModeSwitcher modeSwitcher;
    public SkeletonPuzzleManager puzzleManager;
    public Transform player;
    public float interactDistance = 4f;
    public TextMeshProUGUI interactText;

    private bool started = false;

    void Update()
    {
        if (started || modeSwitcher == null || player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactDistance;

        if (interactText != null)
            interactText.gameObject.SetActive(inRange);

        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            started = true;

            modeSwitcher.EnterPuzzleMode();

            if (puzzleManager != null)
                puzzleManager.StartPuzzle();

            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }
}