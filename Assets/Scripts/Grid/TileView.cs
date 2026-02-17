using UnityEngine;

public class TileView : MonoBehaviour
{
    [SerializeField] private GameObject reachableHighlightObject;
    [SerializeField] private GameObject pathHighlightObject;
    [SerializeField] private GameObject hoverHighlightObject;

    public GridCoord Coord { get; private set; }

    public void Init(GridCoord coord)
    {
        Coord = coord;
        SetReachable(false);
        SetHover(false);
        SetPath(false);
        name = $"Tile_{coord.x}_{coord.z}";
    }

    public void SetReachable(bool on)
    {
        if (reachableHighlightObject != null)
            reachableHighlightObject.SetActive(on);
    }

    public void SetPath(bool on)
    {
        if (pathHighlightObject != null)
            pathHighlightObject.SetActive(on);
    }

    public void SetHover(bool on)
    {
        if (hoverHighlightObject != null)
            hoverHighlightObject.SetActive(on);
    }
}
