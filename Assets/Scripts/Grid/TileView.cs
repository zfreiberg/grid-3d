using UnityEngine;

public class TileView : MonoBehaviour
{
    [SerializeField] private GameObject reachableHighlightObject;
    [SerializeField] private GameObject pathHighlightObject;
    [SerializeField] private GameObject hoverHighlightObject;
    [SerializeField] private GameObject attackHighlightObject;

    public GridCoord Coord { get; private set; }

    private MeshRenderer visualRenderer;

    public void Init(GridCoord coord)
    {
        Coord = coord;

        var visual = transform.Find("Visual");
        if (visual != null)
            visualRenderer = visual.GetComponent<MeshRenderer>();

        SetReachable(false);
        SetHover(false);
        SetPath(false);
        SetAttackRange(false);
        name = $"Tile_{coord.x}_{coord.z}";
    }

    public void SetVisualMaterial(Material mat)
    {
        if (visualRenderer != null && mat != null)
            visualRenderer.sharedMaterial = mat;
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

    public void SetAttackRange(bool on)
    {
        if (attackHighlightObject != null)
            attackHighlightObject.SetActive(on);
    }
}
