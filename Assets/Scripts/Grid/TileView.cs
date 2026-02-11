using UnityEngine;

public class TileView : MonoBehaviour
{
    [SerializeField] private GameObject highlightObject;

    public GridCoord Coord { get; private set; }

    public void Init(GridCoord coord)
    {
        Coord = coord;
        SetHighlight(false);
        name = $"Tile_{coord.x}_{coord.z}";
    }

    public void SetHighlight(bool on)
    {
        if (highlightObject != null)
            highlightObject.SetActive(on);
    }
}
