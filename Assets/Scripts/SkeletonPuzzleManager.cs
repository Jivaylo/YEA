using UnityEngine;

public class SkeletonPuzzleManager : MonoBehaviour
{
    public PuzzlePiece[] pieces;
    public PlayerModeSwitcher modeSwitcher;

    [Header("Scatter")]
    public float scatterForce = 3f;
    public float upwardForce = 1.5f;
    public float torqueForce = 2f;

    private PuzzleSlot[] slots;
    private bool started = false;
    private bool completed = false;

    void Awake()
    {
        slots = GetComponentsInChildren<PuzzleSlot>(true);

        if (modeSwitcher == null)
            modeSwitcher = FindFirstObjectByType<PlayerModeSwitcher>();

        Debug.Log("Puzzle manager found slots: " + slots.Length);
    }

    void Start()
    {
        foreach (var piece in pieces)
        {
            if (piece != null)
            {
                piece.PrepareAtStart();
                piece.gameObject.SetActive(true);
            }
        }
    }

    public void StartPuzzle()
    {
        if (started) return;
        started = true;

        Debug.Log("Puzzle started.");

        foreach (var piece in pieces)
        {
            if (piece == null) continue;

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            Vector3 force = randomDir * scatterForce + Vector3.up * upwardForce;
            Vector3 torque = new Vector3(
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce)
            );

            piece.Scatter(force, torque);
        }
    }

    public void CheckCompletion()
    {
        if (!started || completed) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || !slots[i].solved)
                return;
        }

        completed = true;
        Debug.Log("Puzzle complete! Switching back to 3rd person.");

        if (modeSwitcher != null)
            modeSwitcher.ExitPuzzleMode();
        else
            Debug.LogError("No PlayerModeSwitcher found.");
    }
}