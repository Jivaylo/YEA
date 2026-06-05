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

    void Update()
    {
        if (modeSwitcher == null || player == null || puzzleManager == null)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactDistance;

        bool canRestart =
            inRange &&
            !modeSwitcher.inPuzzleMode &&
            GameSessionState.skeletonDone;

        if (interactText != null)
            interactText.gameObject.SetActive(canRestart);

        if (canRestart && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            modeSwitcher.EnterPuzzleMode();
            puzzleManager.StartPuzzle();

            if (interactText != null)
                interactText.gameObject.SetActive(false);
        }
    }
}