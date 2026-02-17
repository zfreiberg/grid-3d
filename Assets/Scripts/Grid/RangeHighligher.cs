using System.Collections.Generic;
using UnityEngine;

public class RangeHighlighter : MonoBehaviour
{
    [SerializeField] private GridManager grid;

    private readonly HashSet<GridCoord> reachable = new();
    private readonly HashSet<GridCoord> path = new();
    private GridCoord? hover = null;

    public void Clear()
    {
        ClearReachable();
        ClearPath();
        ClearHover();
    }

    public void ShowReachable(IEnumerable<GridCoord> coords)
    {
        ClearReachable();

        foreach (var c in coords)
        {
            var view = grid.GetTileView(c);
            if (view != null)
            {
                view.SetReachable(true);
                reachable.Add(c);
            }
        }
    }

    public void SetPath(IEnumerable<GridCoord> coords)
    {
        ClearPath();

        foreach (var c in coords)
        {
            var view = grid.GetTileView(c);
            if (view != null)
            {
                view.SetPath(true);
                path.Add(c);
            }
        }
    }

    public void SetHover(GridCoord? coord)
    {
        // Turn off old hover
        if (hover.HasValue)
        {
            var oldView = grid.GetTileView(hover.Value);
            if (oldView != null) oldView.SetHover(false);
        }

        hover = coord;

        // Turn on new hover
        if (hover.HasValue)
        {
            var newView = grid.GetTileView(hover.Value);
            if (newView != null) newView.SetHover(true);
        }
    }

    public void ClearPath()
    {
        foreach (var c in path)
        {
            var view = grid.GetTileView(c);
            if (view != null) view.SetPath(false);
        }
        path.Clear();
    }

    public void ClearHover() => SetHover(null);

    private void ClearReachable()
    {
        foreach (var c in reachable)
        {
            var view = grid.GetTileView(c);
            if (view != null) view.SetReachable(false);
        }
        reachable.Clear();
    }
}
