using UnityEngine;

/// <summary>
/// Stores the tile type for every cell in the grid.
/// Serialized as a flat array: tiles[z * width + x].
/// Create via: Assets → Create → Tactics/Tile Map Data
/// </summary>
[CreateAssetMenu(menuName = "Tactics/Tile Map Data", fileName = "TileMapData")]
public class TileMapData : ScriptableObject
{
    public int        width;
    public int        height;
    public TileType[] tiles; // flat: [z * width + x]

    public TileType Get(int x, int z)
    {
        if (tiles == null || tiles.Length == 0) return TileType.Grass;
        int idx = z * width + x;
        return idx >= 0 && idx < tiles.Length ? tiles[idx] : TileType.Grass;
    }

    public void Set(int x, int z, TileType type)
    {
        if (tiles == null) return;
        int idx = z * width + x;
        if (idx >= 0 && idx < tiles.Length)
            tiles[idx] = type;
    }

    /// <summary>
    /// Resize the tile array, preserving existing data where possible.
    /// </summary>
    public void Resize(int newWidth, int newHeight)
    {
        var newTiles = new TileType[newWidth * newHeight];
        for (int z = 0; z < newHeight; z++)
            for (int x = 0; x < newWidth; x++)
                if (x < width && z < height)
                    newTiles[z * newWidth + x] = Get(x, z);
        width  = newWidth;
        height = newHeight;
        tiles  = newTiles;
    }
}
