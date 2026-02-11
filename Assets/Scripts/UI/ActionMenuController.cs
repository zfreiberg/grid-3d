using UnityEngine;

public class ActionMenuController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform root;

    private TacticalStateMachine stateMachine;
    private Unit unit;

    void Awake()
    {
        if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
        if (root == null && canvas != null) root = canvas.GetComponent<RectTransform>();
        Hide();
    }

    public void ShowForUnit(Unit u, TacticalStateMachine sm)
    {
        unit = u;
        stateMachine = sm;

        if (canvas != null) canvas.enabled = true;

        // Optional: position near unit on screen (simple version)
        // If you want: set your menu rect anchored position using Camera.WorldToScreenPoint.
    }

    public void Hide()
    {
        if (canvas != null) canvas.enabled = false;
    }

    // Hook these up to your UI Buttons
    public void OnMovePressed()
    {
        stateMachine.StartMoveSelection();
    }

    public void OnWaitPressed()
    {
        stateMachine.Wait();
    }
}
