using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GridManager grid;
    [SerializeField] private TacticalStateMachine stateMachine;

    private GridCoord? lastHover;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        // If mouse is over UI, don't do world hover/click logic.
        // (This prevents clicking buttons from also clicking tiles behind the menu.)
        if (!overUI)
        {
            // Hover
            if (TryRaycastTile(out var hoverCoord))
            {
                if (!lastHover.HasValue || lastHover.Value != hoverCoord)
                {
                    lastHover = hoverCoord;
                    stateMachine.HoverTile(hoverCoord);
                }
            }
            else
            {
                lastHover = null;
                stateMachine.HoverTile(new GridCoord(-999, -999));
            }

            // Click (world)
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (TryRaycastUnit(out var unit))
                {
                    stateMachine.SelectUnit(unit);
                    return;
                }

                if (TryRaycastTile(out var coord))
                {
                    stateMachine.ClickTile(coord);

                    if (stateMachine.State == TacticalState.UnitSelected)
                        stateMachine.DeselectUnit();

                    return;
                }

                if (stateMachine.State == TacticalState.UnitSelected)
                {
                    stateMachine.DeselectUnit();
                    return;
                }
            }
        }

        // Cancel should still work even over UI
        if (Keyboard.current != null &&
            (Keyboard.current.escapeKey.wasPressedThisFrame ||
            (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)))
        {
            stateMachine.Cancel();
        }
    }


    private bool TryRaycastTile(out GridCoord coord)
    {
        coord = default;

        if (Mouse.current == null) return false;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 500f))
        {
            var tileView = hit.collider.GetComponentInParent<TileView>();
            if (tileView != null)
            {
                coord = tileView.Coord;
                return true;
            }
        }
        return false;
    }

    private bool TryRaycastUnit(out Unit unit)
    {
        unit = null;
        if (Mouse.current == null) return false;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 500f))
        {
            unit = hit.collider.GetComponentInParent<Unit>();
            return unit != null;
        }
        return false;
    }
}
