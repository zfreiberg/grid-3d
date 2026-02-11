using UnityEngine;

public sealed class TileData
{
    public GridCoord Coord { get; }
    public Vector3 WorldCenter { get; }
    public int BaseMoveCost { get; set; } = 1;
    public bool BlocksMovement { get; set; } = false;

    public Unit Occupant { get; set; } = null;

    public TileData(GridCoord coord, Vector3 worldCenter)
    {
        Coord = coord;
        WorldCenter = worldCenter;
    }
}
