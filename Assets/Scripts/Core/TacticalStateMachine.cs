using System.Collections.Generic;
using UnityEngine;

public enum TacticalState
{
    Idle,
    UnitSelected,
    ChoosingMove,
    Moving
}

public class TacticalStateMachine : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GridManager grid;
    [SerializeField] private RangeHighlighter rangeHighlighter;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private PathPreviewRenderer pathPreview;
    [SerializeField] private ActionMenuController actionMenu;
    [SerializeField] private MoveCostTooltip moveCostTooltip;

    public TacticalState State { get; private set; } = TacticalState.Idle;
    public Unit SelectedUnit { get; private set; }

    // Move selection data
    private HashSet<GridCoord> reachable = new();
    private Dictionary<GridCoord, GridCoord> cameFrom = new();
    private Dictionary<GridCoord, int> costSoFar = new();

    void Start()
    {

        turnManager.Begin();

        // Don’t auto-select.
        State = TacticalState.Idle;
        SelectedUnit = null;

        rangeHighlighter.Clear();
        pathPreview.Clear();
        actionMenu.Hide();
    }

    public void SelectUnit(Unit unit)
    {
        if (unit == null) return;
        if (unit.hasActed) return;

        SelectedUnit = unit;
        State = TacticalState.UnitSelected;

        rangeHighlighter.Clear();
        pathPreview.Clear();
        Camera.main.GetComponentInParent<TacticalCameraController>()
            ?.FocusOn(grid.CoordToWorldCenter(unit.currentCoord));

        actionMenu.ShowForUnit(unit, this);
    }

    public void StartMoveSelection()
    {
        if (SelectedUnit == null) return;
        moveCostTooltip.Hide();

        // Compute reachable tiles
        Pathfinding.ComputeReachable(
            grid,
            SelectedUnit.currentCoord,
            SelectedUnit.movePoints,
            out reachable,
            out cameFrom,
            out costSoFar
        );

        // Usually you can always "stay put" — highlight but allow
        rangeHighlighter.ShowReachable(reachable);

        State = TacticalState.ChoosingMove;
        actionMenu.Hide();
    }

    public void HoverTile(GridCoord coord)
    {
        if (State != TacticalState.ChoosingMove) return;

        if (!reachable.Contains(coord))
        {
            pathPreview.Clear();
            rangeHighlighter.ClearPath();
            rangeHighlighter.ClearHover();
            moveCostTooltip.Hide();
            return;
        }

        var path = Pathfinding.ReconstructPath(cameFrom, SelectedUnit.currentCoord, coord);

        // Skip the starting tile so we don't highlight under the unit
        if (path.Count > 0)
            rangeHighlighter.SetPath(path.GetRange(1, path.Count - 1));
        else
            rangeHighlighter.ClearPath();

        rangeHighlighter.SetHover(coord);
        pathPreview.ShowPath(grid, path);

        
        if (costSoFar.TryGetValue(coord, out int cost))
            moveCostTooltip.Show(cost, grid.CoordToWorldCenter(coord), Camera.main, SelectedUnit.movePoints - cost);
        else
            moveCostTooltip.Hide();
    }


    public void ClickTile(GridCoord coord)
    {
        if (State != TacticalState.ChoosingMove) return;
        if (!reachable.Contains(coord)) return;

        // Commit move
        var path = Pathfinding.ReconstructPath(cameFrom, SelectedUnit.currentCoord, coord);
        rangeHighlighter.ClearHover();
        StartCoroutine(MoveUnitRoutine(coord, path));
    }

    private System.Collections.IEnumerator MoveUnitRoutine(GridCoord destination, List<GridCoord> path)
    {
        State = TacticalState.Moving;

        rangeHighlighter.Clear();
        rangeHighlighter.ClearPath();
        rangeHighlighter.ClearHover();
        moveCostTooltip.Hide();

        // Convert to world points
        var worldPts = new List<Vector3>(path.Count);
        foreach (var c in path) worldPts.Add(grid.CoordToWorldCenter(c));

        var view = SelectedUnit.GetComponent<UnitView>();
        yield return view.MoveAlongPath(worldPts);

        SelectedUnit.SetCoord(destination);

        State = TacticalState.UnitSelected;
        actionMenu.ShowForUnit(SelectedUnit, this);
        // DeselectUnit();
    }

    public void Cancel()
    {
        if (State == TacticalState.ChoosingMove)
        {
            State = TacticalState.UnitSelected;
            rangeHighlighter.Clear();
            pathPreview.Clear();
            actionMenu.ShowForUnit(SelectedUnit, this);
        }
        rangeHighlighter.ClearPath();
        rangeHighlighter.ClearHover();
        moveCostTooltip.Hide();
    }

    public void Wait()
    {
        if (SelectedUnit == null) return;

        SelectedUnit.hasActed = true;

        actionMenu.Hide();
        rangeHighlighter.Clear();
        pathPreview.Clear();

        turnManager.AdvanceToNextUnit();
        // SelectUnit(turnManager.CurrentUnit);
        DeselectUnit();
    }

    public void DeselectUnit()
    {
        SelectedUnit = null;
        State = TacticalState.Idle;
        rangeHighlighter.Clear();
        pathPreview.Clear();
        actionMenu.Hide();
    }
}
