using UnityEngine;

public class AutoPuzzleStartTrigger : MonoBehaviour
{
    public PlayerModeSwitcher modeSwitcher;
    public SkeletonPuzzleManager puzzleManager;

    private bool triggered = false;

    void Start()
    {
        if (GameSessionState.skeletonDone)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (GameSessionState.skeletonDone) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (modeSwitcher != null)
                modeSwitcher.EnterPuzzleMode();

            if (puzzleManager != null)
                puzzleManager.StartPuzzle();

            Debug.Log("Auto puzzle started");

            gameObject.SetActive(false);
        }
    }
}