using UnityEngine;

public class AutoPuzzleStartTrigger : MonoBehaviour
{
    public PlayerModeSwitcher modeSwitcher;
    public SkeletonPuzzleManager puzzleManager;

    private bool triggered = false;

    void Start()
    {
        if (PlayerPrefs.GetInt("SkeletonPuzzleCompleted", 0) == 1)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (PlayerPrefs.GetInt("SkeletonPuzzleCompleted", 0) == 1) return;

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