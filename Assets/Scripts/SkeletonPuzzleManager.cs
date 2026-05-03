using UnityEngine;

public class SkeletonPuzzleManager : MonoBehaviour
{
    public PuzzlePiece[] pieces;
    public PlayerModeSwitcher modeSwitcher;

    [Header("Respawn")]
    public Transform pieceRespawnPoint;

    [Header("Scatter")]
    public float scatterForce = 3f;
    public float upwardForce = 1.5f;
    public float torqueForce = 2f;

    private PuzzleSlot[] slots;
    private bool started = false;
    private bool completed = false;

    public bool IsCompleted => completed;

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
        Debug.Log("Starting / Restarting puzzle");

        started = true;
        completed = false;

        foreach (var slot in slots)
        {
            if (slot != null)
                slot.ResetSlot();
        }

        foreach (var piece in pieces)
        {
            if (piece == null) continue;

            piece.PrepareAtStart();

            Vector3 respawnPosition = pieceRespawnPoint != null
                ? pieceRespawnPoint.position
                : transform.position + Vector3.up * 1.5f;

            Quaternion respawnRotation = pieceRespawnPoint != null
                ? pieceRespawnPoint.rotation
                : Quaternion.identity;

            piece.SetSafePosition(respawnPosition, respawnRotation);

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

        foreach (var slot in slots)
        {
            if (slot == null || !slot.solved)
                return;
        }

        completed = true;
        Debug.Log("Puzzle complete! Switching back to 3rd person.");

        if (modeSwitcher != null)
            modeSwitcher.ExitPuzzleMode();
    }
}