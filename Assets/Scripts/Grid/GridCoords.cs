using System;
using UnityEngine;

[Serializable]
public struct GridCoord : IEquatable<GridCoord>
{
    public int x;
    public int z;

    public GridCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(GridCoord other) => x == other.x && z == other.z;
    public override bool Equals(object obj) => obj is GridCoord other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(x, z);

    public static bool operator ==(GridCoord a, GridCoord b) => a.Equals(b);
    public static bool operator !=(GridCoord a, GridCoord b) => !a.Equals(b);

    public override string ToString() => $"({x},{z})";
}
