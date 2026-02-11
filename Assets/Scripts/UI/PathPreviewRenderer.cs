using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathPreviewRenderer : MonoBehaviour
{
    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.useWorldSpace = true;
        lr.widthMultiplier = 0.1f;
    }

    public void ShowPath(GridManager grid, List<GridCoord> path)
    {
        if (path == null || path.Count == 0)
        {
            Clear();
            return;
        }

        lr.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 p = grid.CoordToWorldCenter(path[i]);
            p.y += 0.05f; // lift above tiles
            lr.SetPosition(i, p);
        }
    }

    public void Clear()
    {
        lr.positionCount = 0;
    }
}
