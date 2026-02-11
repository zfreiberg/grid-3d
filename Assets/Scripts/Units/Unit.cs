using UnityEngine;

public enum MoveType
{
    Walking,
    Mounted,
    Flying
}

public class Unit : MonoBehaviour
{
    [Header("Stats")]
    public MoveType moveType = MoveType.Walking;
    public int movePoints = 6;

    [Header("Runtime")]
    public GridCoord currentCoord;
    public bool hasActed;

    public void SetCoord(GridCoord coord)
    {
        currentCoord = coord;
    }
}
