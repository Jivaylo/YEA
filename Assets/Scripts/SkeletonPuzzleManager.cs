using UnityEngine;

public class SkeletonPuzzleManager : MonoBehaviour
{
    public PuzzlePiece[] pieces;
    public PlayerModeSwitcher modeSwitcher;

    [Header("Scatter")]
    public float scatterForce = 0.5f;
    public float upwardForce = 0.2f;
    public float torqueForce = 1f;

    private PuzzleSlot[] slots;
    private bool started = false;
    private bool completed = false;

    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    public bool IsCompleted => completed;

    void Awake()
    {
        slots = GetComponentsInChildren<PuzzleSlot>(true);

        if (modeSwitcher == null)
            modeSwitcher = FindFirstObjectByType<PlayerModeSwitcher>();

        startPositions = new Vector3[pieces.Length];
        startRotations = new Quaternion[pieces.Length];

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;

            startPositions[i] = pieces[i].transform.position;
            startRotations[i] = pieces[i].transform.rotation;
        }

        Debug.Log("Puzzle manager found slots: " + slots.Length);
    }

    void Start()
    {
        ResetPiecesToStartPositions();
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

        ResetPiecesToStartPositions();

        for (int i = 0; i < pieces.Length; i++)
        {
            PuzzlePiece piece = pieces[i];
            if (piece == null) continue;

            piece.SetSafePosition(startPositions[i], startRotations[i]);

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

    void ResetPiecesToStartPositions()
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            PuzzlePiece piece = pieces[i];
            if (piece == null) continue;

            piece.PrepareAtStart();

            piece.transform.position = startPositions[i];
            piece.transform.rotation = startRotations[i];
            piece.transform.localScale = Vector3.one;

            piece.SetSafePosition(startPositions[i], startRotations[i]);
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