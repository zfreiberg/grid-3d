using System.Collections.Generic;
using UnityEngine;

public class RangeHighlighter : MonoBehaviour
{
    [SerializeField] private GridManager grid;

    private readonly HashSet<GridCoord> currentlyHighlighted = new();

    public void Clear()
    {
        foreach (var c in currentlyHighlighted)
        {
            var view = grid.GetTileView(c);
            if (view != null) view.SetHighlight(false);
        }
        currentlyHighlighted.Clear();
    }

    public void ShowReachable(IEnumerable<GridCoord> coords)
    {
        Clear();
        foreach (var c in coords)
        {
            var view = grid.GetTileView(c);
            if (view != null)
            {
                view.SetHighlight(true);
                currentlyHighlighted.Add(c);
            }
        }
    }
}
