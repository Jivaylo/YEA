using UnityEngine;

public class PieceSlot : MonoBehaviour
{
    public string requiredPieceId;
    public bool isFilled = false;

    public bool CanAccept(CarryablePiece piece)
    {
        return !isFilled && piece != null && piece.pieceId == requiredPieceId;
    }

    public void PlacePiece(CarryablePiece piece)
    {
        piece.SnapToSlot(transform);
        isFilled = true;
    }
}