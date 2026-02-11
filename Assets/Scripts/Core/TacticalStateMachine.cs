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

    public TacticalState State { get; private set; } = TacticalState.Idle;
    public Unit SelectedUnit { get; private set; }

    // Move selection data
    private HashSet<GridCoord> reachable = new();
    private Dictionary<GridCoord, GridCoord> cameFrom = new();
    private Dictionary<GridCoord, int> costSoFar = new();

    void Start()
    {
        foreach (var u in FindObjectsOfType<Unit>())
            turnManager.RegisterAlly(u);

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

        actionMenu.ShowForUnit(unit, this);
    }

    public void StartMoveSelection()
    {
        if (SelectedUnit == null) return;

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
        if (!reachable.Contains(coord)) { pathPreview.Clear(); return; }

        var path = Pathfinding.ReconstructPath(cameFrom, SelectedUnit.currentCoord, coord);
        pathPreview.ShowPath(grid, path);
    }

    public void ClickTile(GridCoord coord)
    {
        if (State != TacticalState.ChoosingMove) return;
        if (!reachable.Contains(coord)) return;

        // Commit move
        var path = Pathfinding.ReconstructPath(cameFrom, SelectedUnit.currentCoord, coord);
        StartCoroutine(MoveUnitRoutine(coord, path));
    }

    private System.Collections.IEnumerator MoveUnitRoutine(GridCoord destination, List<GridCoord> path)
    {
        State = TacticalState.Moving;

        rangeHighlighter.Clear();

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
