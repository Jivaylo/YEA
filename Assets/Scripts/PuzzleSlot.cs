using UnityEngine;

public class PuzzleSlot : MonoBehaviour
{
    public string requiredPieceId;
    public GameObject ghostVisual;

    [Header("Ghost Appearance")]
    public float unsolvedAlpha = 0.25f;
    public float solvedAlpha = 1f;

    [HideInInspector] public bool solved = false;

    private Renderer[] ghostRenderers;
    private SkeletonPuzzleManager puzzleManager;

    void Awake()
    {
        if (ghostVisual != null)
            ghostRenderers = ghostVisual.GetComponentsInChildren<Renderer>(true);

        puzzleManager = GetComponentInParent<SkeletonPuzzleManager>();

        SetGhostAlpha(unsolvedAlpha);
    }

    public bool CanAccept(PuzzlePiece piece)
    {
        if (piece == null) return false;
        if (solved) return false;
        return piece.pieceId == requiredPieceId;
    }

    public void PlacePiece(PuzzlePiece piece)
    {
        if (piece == null) return;

        solved = true;
        piece.ConsumePlaced();
        SetGhostAlpha(solvedAlpha);

        Debug.Log(name + " solved.");

        if (puzzleManager != null)
            puzzleManager.CheckCompletion();
    }

    public void ResetSlot()
    {
        solved = false;
        SetGhostAlpha(unsolvedAlpha);
    }

    void SetGhostAlpha(float a)
    {
        if (ghostRenderers == null) return;

        foreach (var r in ghostRenderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = a;
                    mat.SetColor("_BaseColor", c);
                }
            }
        }
    }
}