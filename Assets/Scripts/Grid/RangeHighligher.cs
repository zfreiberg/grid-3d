using System.Collections.Generic;
using UnityEngine;

public class RangeHighlighter : MonoBehaviour
{
    [SerializeField] private GridManager grid;

    private readonly HashSet<GridCoord> reachable = new();
    private GridCoord? currentHover = null;

    public void Clear()
    {
        // Clear reachable
        foreach (var c in reachable)
        {
            var view = grid.GetTileView(c);
            if (view != null) view.SetReachable(false);
        }
        reachable.Clear();

        // Clear hover
        ClearHover();
    }

    public void ShowReachable(IEnumerable<GridCoord> coords)
    {
        Clear();

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

    public void SetHover(GridCoord? coord)
    {
        // Turn off old hover
        if (currentHover.HasValue)
        {
            var oldView = grid.GetTileView(currentHover.Value);
            if (oldView != null) oldView.SetHover(false);
        }

        currentHover = coord;

        // Turn on new hover
        if (currentHover.HasValue)
        {
            var newView = grid.GetTileView(currentHover.Value);
            if (newView != null) newView.SetHover(true);
        }
    }

    public void ClearHover() => SetHover(null);

    public bool IsReachable(GridCoord c) => reachable.Contains(c);
}
