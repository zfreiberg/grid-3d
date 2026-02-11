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
