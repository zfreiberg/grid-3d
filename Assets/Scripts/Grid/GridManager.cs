using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Prefabs")]
    [SerializeField] private TileView tilePrefab;

    [Header("Tile Map")]
    [SerializeField] private TileMapData tileMapData;

    [Header("Tile Materials")]
    [SerializeField] private Material grassMaterial;
    [SerializeField] private Material roadMaterial;
    [SerializeField] private Material forestMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Material sandMaterial;

    [Header("Tile Move Costs")]
    [SerializeField] private int grassMoveCost  = 1;
    [SerializeField] private int roadMoveCost   = 1;
    [SerializeField] private int forestMoveCost = 2;
    [SerializeField] private int waterMoveCost  = 3;
    [SerializeField] private int sandMoveCost   = 2;
    [SerializeField] private bool waterBlocksMovement = false;

    private readonly Dictionary<GridCoord, TileData> tiles = new();
    private readonly Dictionary<GridCoord, TileView> tileViews = new();

    public int Width => width;
    public int Height => height;
    public float TileSize => tileSize;

    void Awake()
    {
        BuildGrid();
    }

    public void BuildGrid()
    {
        // Clear old
        foreach (var kv in tileViews)
        {
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        }
        tiles.Clear();
        tileViews.Clear();

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                var coord = new GridCoord(x, z);
                Vector3 world = CoordToWorldCenter(coord);

                var data = new TileData(coord, world);
                tiles.Add(coord, data);

                if (tilePrefab != null)
                {
                    TileView view = Instantiate(tilePrefab, world, Quaternion.identity, transform);
                    view.Init(coord);

                    // Scale the tile visually if your prefab is 1x1
                    view.transform.localScale = new Vector3(tileSize, 0.1f, tileSize);

                    tileViews.Add(coord, view);
                }
            }
        }

        ApplyTileMap();
    }

    public Vector3 CoordToWorldCenter(GridCoord coord)
    {
        return origin + new Vector3(coord.x * tileSize, 0f, coord.z * tileSize);
    }

    public bool IsInBounds(GridCoord c)
    {
        return c.x >= 0 && c.x < width && c.z >= 0 && c.z < height;
    }

    public bool TryGetTile(GridCoord coord, out TileData tile) => tiles.TryGetValue(coord, out tile);

    public TileView GetTileView(GridCoord coord)
    {
        tileViews.TryGetValue(coord, out var view);
        return view;
    }

    /// <summary>Convert a world position to the nearest grid coordinate.</summary>
    public GridCoord WorldToCoord(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - origin.x) / tileSize);
        int z = Mathf.RoundToInt((worldPos.z - origin.z) / tileSize);
        return new GridCoord(x, z);
    }

    private Material GetMaterialForType(TileType type) => type switch
    {
        TileType.Grass  => grassMaterial,
        TileType.Road   => roadMaterial,
        TileType.Forest => forestMaterial,
        TileType.Water  => waterMaterial,
        TileType.Sand   => sandMaterial,
        _               => null,
    };

    private (int moveCost, bool blocksMovement) GetStatsForType(TileType type) => type switch
    {
        TileType.Grass  => (grassMoveCost,  false),
        TileType.Road   => (roadMoveCost,   false),
        TileType.Forest => (forestMoveCost, false),
        TileType.Water  => (waterMoveCost,  waterBlocksMovement),
        TileType.Sand   => (sandMoveCost,   false),
        _               => (1, false),
    };

    /// <summary>
    /// Set the tile type at a coordinate, updating move cost, block flag, and visual material.
    /// Pass updateData = false when driving from TileMapData (e.g. in ApplyTileMap) to avoid a loop.
    /// </summary>
    public void SetTileType(GridCoord coord, TileType type, bool updateData = true)
    {
        if (!TryGetTile(coord, out var data)) return;

        data.TileType = type;

        var (moveCost, blocksMovement) = GetStatsForType(type);
        data.BaseMoveCost   = moveCost;
        data.BlocksMovement = blocksMovement;

        var mat = GetMaterialForType(type);
        if (mat != null && tileViews.TryGetValue(coord, out var view))
            view.SetVisualMaterial(mat);

        if (updateData && tileMapData != null)
            tileMapData.Set(coord.x, coord.z, type);
    }

    /// <summary>
    /// Apply the saved TileMapData to every tile in the grid.
    /// Called automatically on BuildGrid(); also exposed for the editor painter.
    /// </summary>
    public void ApplyTileMap()
    {
        if (tileMapData == null) return;

        if (tileMapData.width != width || tileMapData.height != height)
            tileMapData.Resize(width, height);

        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
                SetTileType(new GridCoord(x, z), tileMapData.Get(x, z), false);
    }

    public IEnumerable<GridCoord> GetNeighbors4(GridCoord c)
    {
        // 4-directional
        var n = new GridCoord(c.x + 1, c.z);
        var s = new GridCoord(c.x - 1, c.z);
        var e = new GridCoord(c.x, c.z + 1);
        var w = new GridCoord(c.x, c.z - 1);

        if (IsInBounds(n)) yield return n;
        if (IsInBounds(s)) yield return s;
        if (IsInBounds(e)) yield return e;
        if (IsInBounds(w)) yield return w;
    }
}
