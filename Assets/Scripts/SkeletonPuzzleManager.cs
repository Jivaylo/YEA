using UnityEngine;

public class SkeletonPuzzleManager : MonoBehaviour
{
    public CarryablePiece[] pieces;
    public bool broken = false;
    public float scatterForce = 2.5f;
    public float upwardForce = 1.5f;

    public void BreakSkeleton()
    {
        if (broken) return;

        broken = true;

        foreach (CarryablePiece piece in pieces)
        {
            if (piece == null) continue;

            Vector3 randomDir = new Vector3(
                Random.Range(-1.5f, 1.5f),
                0f,
                Random.Range(-1.5f, 1.5f)
            ).normalized;

            Vector3 force = randomDir * scatterForce + Vector3.up * upwardForce;
            piece.Drop(force);
        }
    }

    public bool IsComplete()
    {
        foreach (CarryablePiece piece in pieces)
        {
            if (piece == null || !piece.isPlaced)
                return false;
        }

        return true;
    }
}